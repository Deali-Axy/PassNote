using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using QFrameworkOne.Module;

namespace PasswordNote
{
    public partial class FrmKey : Form
    {
        private StringBuilder _key = new StringBuilder();

        public FrmKey()
        {
            InitializeComponent();
            this.Text = string.Format("{0} {1} {2}", QApp.Name, QApp.Title, QApp.Version.ToString());
        }

        public string Key => _key.ToString();

        public string Title
        {
            set { this.lblTitle.Text = value; }
            get => this.lblTitle.Text;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FrmKey_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    this.BtnOK.PerformClick();
                    break;
                case Keys.Escape:
                    this.BtnCancel.PerformClick();
                    break;
            }
        }

        private void TxtKey_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (GenerateSimplePassword.ListAllChars.Contains(e.KeyChar))
            {
                _key.Append(e.KeyChar);
                this.TxtKey.Text += GenerateSimplePassword.RandomPassword(1, true, true, true, false);
                this.TxtKey.Select(this.TxtKey.Text.Length, 0);
                e.Handled = true;
            }
        }

        private void TxtKey_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Back:
                    if (_key.Length > 0)
                    {
                        _key.Remove(_key.Length - 1, 1);
                        this.TxtKey.Text.Remove(this.TxtKey.Text.Length - 1, 1);
                    }
                    break;
            }
        }
    }
}
