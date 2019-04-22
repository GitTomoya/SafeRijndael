using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Permissions;

namespace SafeRijndael
{
    public partial class Form3 : Form
    {
        public CheckBox CheckBox1 { get; set; }
        public CheckBox CheckBox2 { get; set; }
        public TextBox TextBox1 { get; set; }
        public TextBox TextBox2 { get; set; }

        private CancellationTokenSource tokenSource;

        private readonly SafeRijndael.Configs.AppSettings settings = SafeRijndael.Configs.AppSettings.Default;

        public Form3()
        {
            InitializeComponent();
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            panel2.Visible = false; panel1.Visible = true;
            Progress.ProgressBar1 = this.progressBar1;
            Progress.Label1 = this.label1; Progress.Label2 = this.label2;

            await CryptoExecution();//暗号化、または復号処理

            button2.Enabled = false;

            await Task.Delay(1500);//視覚的に分かりやすいよう僅かに待機

            if (settings.進捗終了時のメッセージ) MessageBox.Show("完了！", "確認", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            if (settings.プログレスウィンドウを出力後に閉じる) this.Close();
        }

        private async Task CryptoExecution()
        {
            using (this.tokenSource = new CancellationTokenSource())
            {
                if (Directory.Exists(TextBox1.Text) | TextBox1.Text.Contains(".zip"))
                {//フォルダの場合の暗号化および復号
                    ICrypto fcrypto = new FolderCrypto
                    {
                        CryptoMode = TextBox1.Text.Contains(".zip")
                    ? CryptoMode.DENCRYPTION : CryptoMode.ENCRYPTION
                    };

                    switch (fcrypto.CryptoMode)
                    {
                        case CryptoMode.DENCRYPTION://拡張子が.zipの場合は復号
                            await FileBranchProcessingAsync(fcrypto, tokenSource);
                            break;
                        case CryptoMode.ENCRYPTION://それ以外の場合は暗号化
                            await FolderBranchProcessingAsync(fcrypto, tokenSource);
                            break;
                    }
                }
                else
                {
                    ICrypto crypto = new Crypto
                    {
                        CryptoMode = TextBox1.Text.Contains(".safer")
                        ? CryptoMode.DENCRYPTION : CryptoMode.ENCRYPTION
                    };
                    Progress.Initialization(TextBox1.Text);
                    await FileBranchProcessingAsync(crypto, tokenSource);//分岐処理まとめた関数
                }
            }
        }

        private async Task FileBranchProcessingAsync(ICrypto crypto, CancellationTokenSource token)
        {
            if (CheckBox1.Checked & !CheckBox2.Checked)//元のファイル消去の処理
            {
                await crypto.WriteStreamAsync
                    (textBox1.Text, TextBox1.Text, Path.GetDirectoryName(TextBox1.Text), token.Token);
                FileOperation.DeleteFile(TextBox1.Text);
            }
            else if (CheckBox1.Checked & CheckBox2.Checked)//元のファイルを消去する処理と出力変更の処理
            {
                await crypto.WriteStreamAsync
                    (textBox1.Text, TextBox1.Text, TextBox2.Text, token.Token);
                FileOperation.DeleteFile(TextBox1.Text);
            }
            else if (CheckBox2.Checked & !CheckBox1.Checked)//出力先変更の処理
            {
                await crypto.WriteStreamAsync
                    (textBox1.Text, TextBox1.Text, TextBox2.Text, token.Token);
            }
            else//デフォルトの処理
            {
                await crypto.WriteStreamAsync
                    (textBox1.Text, TextBox1.Text,Path.GetDirectoryName(TextBox1.Text), token.Token);
            }
        }

        private async Task FolderBranchProcessingAsync(ICrypto crypto, CancellationTokenSource token)
        {
            if (CheckBox2.Checked & !CheckBox1.Checked)//出力先変更の処理
            {
                await crypto.WriteStreamAsync
                    (textBox1.Text, TextBox1.Text, TextBox2.Text, token.Token);
            }
            else//デフォルトの処理
            {
                await crypto.WriteStreamAsync
                    (textBox1.Text, TextBox1.Text, TextBox1.Text, token.Token);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (tokenSource == null)
                return;

            tokenSource.Cancel();//処理の中断
            tokenSource.Dispose();
            this.Close();
        }

        protected override CreateParams CreateParams
        {
            //フォームを閉じるボタンを無効化
            [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                const int CS_NOCLOSE = 0x200;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle = cp.ClassStyle | CS_NOCLOSE;

                return cp;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.PasswordChar = checkBox1.Checked ? '\0' : '*';
        }
    }
}
