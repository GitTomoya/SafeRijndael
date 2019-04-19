using System;
using System.Windows.Forms;

namespace SafeRijndael
{
    public partial class Form1 : Form
    {
        private readonly SafeRijndael.Configs.AppSettings settings = SafeRijndael.Configs.AppSettings.Default;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("ファイルまたはフォルダを指定してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Question);
                return;
            }

            using (Form3 f3 = new Form3())
            {
                f3.StartPosition = FormStartPosition.CenterParent;
                f3.CheckBox1 = this.checkBox1; f3.CheckBox2 = this.checkBox2;
                f3.TextBox1 = this.textBox1; f3.TextBox2 = this.textBox2;

                f3.ShowDialog();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //フォームの固定
            this.MaximumSize = this.Size; 
            this.MinimumSize = this.Size;
        }

        private void 詳細設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Form2 f2 = new Form2())
            {
                f2.StartPosition = FormStartPosition.CenterParent;
                f2.ShowDialog();
            }
        }

        private void ファイルを開くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = FileOperation.FileDialog)
            {
                try
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        textBox1.Text = dialog.FileName;//選択したファイルのパスをテキストボックスに表示
                        checkBox1.Enabled = true;
                    }
                }
                catch
                {
                    MessageBox.Show("ファイルにアクセスできません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = FileOperation.BrowserDialog)
            {
                if (checkBox2.Checked)
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        textBox2.Text = dialog.SelectedPath;//選択したフォルダのパスをテキストボックスに表示
                    }
                    else
                    {
                        checkBox2.Checked = false;
                    }
                }
            }
        }

        private void フォルダを開くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = FileOperation.BrowserDialog)
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = dialog.SelectedPath;//選択したフォルダのパスをテキストボックスに表示
                    checkBox1.Enabled = false; checkBox1.Checked = false;
                }
            }
        }

        private void バージョン情報ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("現在のバージョンは" + Application.ProductVersion + "です。" + "\nAlpha版", "バージョン情報" ,MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (!settings.ドラッグアンドドロップを許可)
                return;

            string[] drag = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            textBox1.Text = drag[0];
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (!settings.ドラッグアンドドロップを許可)
                return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
    }
}
