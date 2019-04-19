using System;
using System.Windows.Forms;

namespace SafeRijndael
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            string mutexName = Application.ProductName;
            //Mutexオブジェクトを作成する
            System.Threading.Mutex mutex = new System.Threading.Mutex(false, mutexName);
            bool hasHandle = false;

            try
            {
                if (SafeRijndael.Configs.AppSettings.Default.二重起動の防止)
                {
                    try
                    {
                        //ミューテックスの所有権を要求する
                        hasHandle = mutex.WaitOne(0, false);
                    }
                    catch (System.Threading.AbandonedMutexException)
                    {
                        //別のアプリケーションがミューテックスを解放しないで終了した時
                        hasHandle = true;
                    }
                    //ミューテックスを得られたか調べる
                    if (hasHandle == false)
                    {
                        //得られなかった場合は、すでに起動していると判断して終了
                        MessageBox.Show("多重起動は禁止されています！(オプションから変更可能です)", "確認", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            finally
            {
                if (hasHandle)
                {
                    //ミューテックスを解放する
                    mutex.ReleaseMutex();
                }
                mutex.Close();
            }
        }
    }
}
