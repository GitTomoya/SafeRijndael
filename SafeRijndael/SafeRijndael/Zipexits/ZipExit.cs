using System.IO;
using System.IO.Compression;

namespace SafeRijndael
{
    public static class ZipExit
    {
        /// <summary>
        /// 指定されたファイルを作成されたzipファイルに追加し出力します
        /// </summary>
        /// <param name="readZip"></param>
        /// <param name="readFile"></param>
        /// <param name="outDir"></param>
        public static void Exit(string readZip ,string readFile)
        {
            if (!File.Exists(readZip))
            {
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                {
                    zip.Save(readZip);
                }
            }

            using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(readZip))
            {
                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                zip.AddFile(readFile);
                zip.Save();
            }
        }

        /// <summary>
        /// zipファイルを解凍しフォルダに出力します
        /// </summary>
        /// <param name="readPath"></param>
        /// <param name="outPath"></param>
        public static void Extract(string readPath ,string outPath)
        {
            ZipFile.ExtractToDirectory(readPath, outPath);
        }
    }
}
