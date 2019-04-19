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
        public static void ZipFileExit(string readZip ,string readFile, string outDir)
        {
            using (ZipArchive archive = ZipFile.Open(readZip, ZipArchiveMode.Update))
            {
                archive.CreateEntryFromFile(readFile,
                    outDir + ".safer", CompressionLevel.Optimal);
            }
        }

        /// <summary>
        /// zipファイルを解凍しフォルダに出力します
        /// </summary>
        /// <param name="readPath"></param>
        /// <param name="outPath"></param>
        public static void UnZipExtract(string readPath ,string outPath)
        {
            ZipFile.ExtractToDirectory(readPath, outPath);
        }
    }
}
