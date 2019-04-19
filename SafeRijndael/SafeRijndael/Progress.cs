using System;
using System.IO;
using System.Windows.Forms;

namespace SafeRijndael
{
    public static class Progress
    {
        public static ProgressBar ProgressBar1 { get; set; }
        public static Label Label1 { get; set; }
        public static Label Label2 { get; set; }

        public static void InitializationProgress(string path)
        {
            if (ProgressBar1.Style == ProgressBarStyle.Blocks)
                ProgressBar1.Minimum = 0; ProgressBar1.Value = 0; //最小値及び現在の値

            FileInfo info = new FileInfo(path);
            ProgressBar1.Maximum = (int)info.Length;//プログレスバーの最大値
            Label2.Text = "処理中 = " + Path.GetFileName(path);
            Label1.Text = "暗号化の為のハッシュ値を計算しています...";
        }

        public static void EncryptionProgressUpdate(long progress)
        {
            if (ProgressBar1.Style == ProgressBarStyle.Marquee)
                return;
            ProgressBar1.Value = (int)progress;
            Label1.Text = FormatSize(ProgressBar1.Value) + "/" + FormatSize(ProgressBar1.Maximum);
        }

        public static void ProgressMaximumPatch(string path)
        {
            if (ProgressBar1.Style == ProgressBarStyle.Marquee)
                return;
            FileInfo info = new FileInfo(path);
            ProgressBar1.Value = (int)info.Length;
            ProgressBar1.Maximum = (int)info.Length;
            Label1.Text = FormatSize(ProgressBar1.Value) + "/" + FormatSize(ProgressBar1.Maximum);
        }

        public static void DencryptionProgressUpdate(long progress)
        {
            if (ProgressBar1.Style == ProgressBarStyle.Marquee)
                return;

                if (progress > ProgressBar1.Maximum)
                    ProgressBar1.Maximum = (int)progress;

                ProgressBar1.Value = (int)progress;
                Label1.Text = FormatSize(ProgressBar1.Value) + "/" + FormatSize(ProgressBar1.Maximum);
        }

        private static string FormatSize(long amt)
        {
            if (amt >= Math.Pow(2, 20))
                return Math.Round(amt/ Math.Pow(2, 20), 2).ToString() + "MB"; //megabyte
            if (amt >= Math.Pow(2, 10)) return Math.Round(amt
                / Math.Pow(2, 10), 2).ToString() + " KB"; //kilobyte

            return amt.ToString();
        }

        public static void StyletoMarqueeChange()
        {
            if (FormatSize(ProgressBar1.Maximum).Contains("MB"))
            { ProgressBar1.Style = ProgressBarStyle.Blocks;  return; }

            ProgressBar1.Style = ProgressBarStyle.Marquee;
            Label1.Text = "処理を実行中...\n(ひとつひとつのファイルサイズが小さいため表示を簡略しています)";
        }
    }
}
