using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System;
using System.Text;

namespace SafeRijndael
{
    public sealed class FolderCrypto : Crypto
    {
        public override async Task WriteStreamAsync(string password, string readPath, string outPath, CancellationToken token)
        {
            foreach (string file in GetAllFiles(readPath, outPath))//ファイルを列挙
            {
                Progress.Initialization(file);//プログレスバーに初期値をセット 
                Progress.ChangeToMarquee(); //状況に合わせてプログレスバーの表示を変更

                await base.WriteStreamAsync(password, file, OutDirPath(outPath, RelativeDirPath(new Uri(outPath), new Uri(file))), token); //列挙されたファイルを一つずつ処理

                if (token.IsCancellationRequested) break;//処理の中断

                switch (CryptoMode)
                {
                    case CryptoMode.ENCRYPTION://暗号化
                        ZipExit.Exit(outPath + @"\" + ZipFileName(readPath) + ".zip", file + ".safer");//出力された.saferファイルをzipファイルに追加
                        DeleteTemporaryFile(file + ".safer");//一時的に作成された.saferファイルを消去
                        break;
                    case CryptoMode.DENCRYPTION:
                        DeleteTemporaryFile(file);//解凍された.saferファイルを消去
                        break;
                }
            }
        }

        /// <summary>
        /// 指定されたフォルダのサブディレクトリを含めたすべてのファイルを取得します
        /// </summary>
        /// <param name="readPath"></param>
        /// <param name="outPath"></param>
        /// <returns></returns>
        private IEnumerable<string> GetAllFiles(string readPath, string outPath)
        {
            if (CryptoMode == CryptoMode.ENCRYPTION)
            {
                return Directory.EnumerateFiles(readPath, "*", SearchOption.AllDirectories);
            }
            else
            {
                ZipExit.Extract(readPath , outPath + @"\" + ZipFileName(readPath));　//.zipファイルを解凍しフォルダに出力
                return Directory.EnumerateFiles(outPath + @"\" + ZipFileName(readPath), "*", SearchOption.AllDirectories);
            }
        }

        /// <summary>
        /// ファイル名を含まないディレクトリのパスを取得します
        /// </summary>
        /// <param name="outPath"></param>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        private string OutDirPath(string outPath, string relativePath)
        {
            return Path.GetDirectoryName
                (Path.Combine(Path.GetDirectoryName(outPath), relativePath));
        }

        /// <summary>
        /// 出力先フォルダの相対的なパスを取得します
        /// </summary>
        /// <param name="outUri"></param>
        /// <param name="fileUri"></param>
        /// <returns></returns>
        private string RelativeDirPath(Uri outUri, Uri fileUri)
        {
            return HttpUtility.UrlDecode(outUri.MakeRelativeUri
                (fileUri).ToString().Replace("/", @"\"), Encoding.UTF8);
        }


        /// <summary>
        /// zipファイルのパスから拡張子を除いたファイル名を取得します
        /// </summary>
        /// <param name="zipPath"></param>
        /// <returns></returns>
        private string ZipFileName(string zipPath)
        {
            return Path.GetFileNameWithoutExtension(zipPath);
        }

        /// <summary>
        /// 作成された一時的なファイルを消去します
        /// </summary>
        /// <param name="path"></param>
        private void DeleteTemporaryFile(string path)
        {
            File.Delete(path);
        }
    }
}
