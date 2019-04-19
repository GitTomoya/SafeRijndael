using System;
using System.Windows.Forms;

namespace SafeRijndael
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private readonly SafeRijndael.Configs.AppSettings settings = SafeRijndael.Configs.AppSettings.Default;

        private void button2_Click(object sender, EventArgs e)
        {
            settings.二重起動の防止 = checkBox1.Checked ? true : false;

            settings.ドラッグアンドドロップを許可 = checkBox3.Checked ? true : false;

            settings.プログレスウィンドウを出力後に閉じる = checkBox4.Checked ? true : false;

            settings.進捗終了時のメッセージ = checkBox2.Checked ? true : false;

            settings.ハッシュ化アルゴリズム = radioButton1.Checked ? 0 : 1;//0は"PBKDF2"。1は"Argon2"。

            settings.Save();
            MessageBox.Show("設定を変更しました", "確認", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = settings.二重起動の防止 == true ? true : false;

            checkBox3.Checked = settings.ドラッグアンドドロップを許可 == true ? true : false;

            checkBox4.Checked = settings.プログレスウィンドウを出力後に閉じる == true ? true : false;

            checkBox2.Checked = settings.進捗終了時のメッセージ == true ? true : false;

            switch (settings.ハッシュ化アルゴリズム)
            {
                case 0:
                    radioButton1.Checked = true; break;
                case 1:
                    radioButton2.Checked = true; break;
            }
        }
    }
}
