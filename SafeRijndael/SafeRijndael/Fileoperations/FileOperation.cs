using System.IO;
using System.Windows.Forms;

namespace SafeRijndael
{
    public static class FileOperation
    {
        /// <summary>
        /// 指定したファイルを消去します
        /// </summary>
        /// <param name="Path"></param>
        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        /// <summary>
        /// ファイルダイアログを取得します
        /// </summary>
        /// <returns></returns>
        public static OpenFileDialog FileDialog
        {
            get
            {
                OpenFileDialog fileDialog = new OpenFileDialog
                {
                    InitialDirectory = @"C:\",
                    Title = "ファイルを選択してください",
                    RestoreDirectory = true,
                    Multiselect = false,
                    Filter = "safer,zip,または全てのファイル(*.safer;*.zip)(*.*)|*.*"
                };
                return fileDialog;
            }
        }

        /// <summary>
        /// フォルダダイアログを取得します
        /// </summary>
        /// <returns></returns>
        public static FolderBrowserDialog BrowserDialog
        {
            get
            {
                FolderBrowserDialog folderBrowser = new FolderBrowserDialog
                {
                    Description = "フォルダを選択してください",
                    ShowNewFolderButton = false
                };

                return folderBrowser;
            }
        }
    }
}
