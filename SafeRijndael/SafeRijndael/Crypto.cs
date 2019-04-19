using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.IO.Compression;
using System;
using System.Windows.Forms;
using Konscious.Security.Cryptography;
using System.Text;

namespace SafeRijndael
{
    public enum CryptoMode
    {
        ENCRYPTION, DENCRYPTION // 暗号化か復号か
    }

    public class Crypto : ICrypto
    {
        private Rijndael rijndael;
        private Argon2d argon2;

        private readonly SafeRijndael.Configs.AppSettings settings = SafeRijndael.Configs.AppSettings.Default;

        public CryptoMode CryptoMode {get; set; }

        public Crypto()
        {
            rijndael = new RijndaelManaged
            {
                BlockSize = 256,
                KeySize = 256,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };
        }
        /// <summary>
        /// 指定されたパスワード、元のファイル、出力先で暗号化及び復号を非同期的に行います
        /// </summary>
        /// <param name="password"></param>
        /// <param name="outfile"></param>
        /// <returns></returns>
        public virtual async Task WriteStreamAsync(string password, string readPath, string outPath, CancellationToken token)
        {
            try
            {
                switch (CryptoMode) 
                {
                    case CryptoMode.ENCRYPTION://暗号化
                        await FileEncryption(password, readPath, outPath + "\\" +　
                            Path.GetFileName(readPath) + ".safer", token);
                        break;
                    case CryptoMode.DENCRYPTION://復号
                        await FileDecryption(password, readPath, outPath + "\\" + 
                            Path.GetFileName(readPath).Replace(".safer", ""), token);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("操作を中止しました。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (CryptographicException)
            {
                if (token.IsCancellationRequested) { MessageBox.Show("操作を中止しました。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); return; }
                MessageBox.Show("パスワードが間違っていませんか？", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                rijndael.Dispose();
                argon2?.Dispose();
            }
        }

        /// <summary>
        /// saltを取得します
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private byte[] GetSalt(int size)
        {
            var bytes = new byte[size];
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// ハッシュ値を取得します
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private (Task<byte[]> hash, byte[] salt) GetArgon2Hash(string password)
        {
            argon2 = new Argon2d(Encoding.UTF8.GetBytes(password))
            {
                DegreeOfParallelism = 16,
                MemorySize = 8192,
                Iterations = 40,
                Salt = GetSalt(32)
            };
            return (argon2.GetBytesAsync(32), argon2.Salt);
        }

        private Task<byte[]> GetArgon2Hash(string password, FileStream fs)
        {
            argon2 = new Argon2d(Encoding.UTF8.GetBytes(password))
            {
                DegreeOfParallelism = 16,
                MemorySize = 8192,
                Iterations = 40
            };

            byte[] salt = new byte[32];
            fs.Read(salt, 0 ,32);//埋め込んでいたSaltを読み取る
            argon2.Salt = salt;

            return argon2.GetBytesAsync(32);
        }

        private (byte[] hash, byte[] salt) GetPbkdf2Hash(string password)
        {
            Rfc2898DeriveBytes derive = new Rfc2898DeriveBytes(password, GetSalt(32), 10000);

            return (derive.GetBytes(32), derive.Salt);
        }
         
        private byte[] GetPbkdf2Hash(string password, FileStream fs)
        {
            byte[] salt = new byte[32];
            Rfc2898DeriveBytes derive = new Rfc2898DeriveBytes(password, salt, 10000);

            fs.Read(salt, 0, 32);//埋め込んでいたSaltを読み取る
            derive.Salt = salt;

            return derive.GetBytes(32);
        }

        /// <summary>
        /// ファイルの暗号化の基本操作を行います
        /// </summary>
        private async Task FileEncryption(string password, string readPath, string outPath, CancellationToken token)
        {
            using (FileStream writefs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
            {
                using (CryptoStream cse = new CryptoStream
                        (await EncryptionWriteAsync(password, writefs), rijndael.CreateEncryptor(rijndael.Key, rijndael.IV), CryptoStreamMode.Write))
                {
                    using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Compress))
                    {
                        using (FileStream readfs = new FileStream(readPath, FileMode.Open, FileAccess.Read))
                        {
                            long progress = 0; int len = 0; byte[] buffer = new byte[4096];
                            while ((len = readfs.Read(buffer, 0, 4096)) > 0)
                            {
                                await ds.WriteAsync(buffer, 0, len);
                                Progress.EncryptionProgressUpdate(progress); progress += len;//進捗状況

                                token.ThrowIfCancellationRequested();//処理の中断
                            }
                            Progress.ProgressMaximumPatch(outPath);//余ったプログレスバーの最大値を埋める
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ファイルの復号の基本操作を行います
        /// </summary>
        /// <param name="password"></param>
        /// <param name="readPath"></param>
        /// <param name="outPath"></param>
        /// <returns></returns>
        private async Task FileDecryption(string password, string readPath, string outPath, CancellationToken token)
        {
            using (FileStream writefs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
            {
                using (FileStream readfs = new FileStream(readPath, FileMode.Open, FileAccess.Read))
                {
                    using (CryptoStream cse = new CryptoStream
                   (await DecryptionReadAsync(password, readfs), rijndael.CreateDecryptor(rijndael.Key, rijndael.IV), CryptoStreamMode.Read))
                    {
                        using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Decompress))
                        {
                            long progress = 0; int len = 0; byte[] buffer = new byte[4096];
                            while ((len = ds.Read(buffer, 0, 4096)) > 0)
                            {
                                await writefs.WriteAsync(buffer, 0, len);
                                Progress.DencryptionProgressUpdate(progress); progress += len;//進捗状況

                                token.ThrowIfCancellationRequested();//処理の中断
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// ファイルにsaltと初期化ベクトルを埋め込みます
        /// </summary>
        /// <param name="password"></param>
        /// <param name="fs"></param>
        private async Task<FileStream> EncryptionWriteAsync(string password, FileStream fs)
        {
            switch (settings.ハッシュ化アルゴリズム)
            {
                case 0:
                    (byte[] hash, byte[] salt) derive = GetPbkdf2Hash(password);

                    rijndael.Key = derive.hash;//ハッシュ値を取得
                    fs.Write(derive.salt, 0, 32);//saltを埋め込む
                    break;
                case 1:
                    (Task<byte[]> hash, byte[] salt) argon2 = GetArgon2Hash(password);

                    rijndael.Key = await argon2.hash;//ハッシュ値を取得
                    fs.Write(argon2.salt, 0, 32);//saltを埋め込む
                    break;

            }

            rijndael.GenerateIV();//ランダムにivを生成
            fs.Write(rijndael.IV, 0, 32);//ivを埋め込む

            return fs;
        }

        /// <summary>
        /// ファイルからsaltと初期化ベクトルを読み取ります
        /// </summary>
        /// <param name="password"></param>
        /// <param name="fs"></param>
        private async Task<FileStream> DecryptionReadAsync(string password, FileStream fs)
        {
            switch (settings.ハッシュ化アルゴリズム)
            {
                case 0:
                    rijndael.Key = GetPbkdf2Hash(password, fs);
                    break;
                case 1:
                    rijndael.Key = await GetArgon2Hash(password, fs);
                    break;
            }

            byte[] iv = new byte[32];
            fs.Read(iv, 0, 32);//ivを読み取る
            rijndael.IV = iv;

            return fs;
        }
    }
}
