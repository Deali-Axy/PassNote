using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QFrameworkOne.Application.Component;
using QFrameworkOne.Module;

namespace PasswordNote
{
    public partial class FrmMain : Form
    {
        private string Key = "";
        private Dictionary<string, CAccount> AccountList = new Dictionary<string, CAccount>();
        private CAccount SelectedAccount;

        public FrmMain()
        {
            InitializeComponent();

            this.Text = string.Format("{0} {1} {2} {3}", QApp.Name, QApp.Title, QApp.Version.ToString(), QApp.Description);

            SetBtnEnabled(false);
            ShowMsg("请输入密匙开启！");
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            this.BtnEnterKey.PerformClick();
        }

        private void ShowMsg(string msg, params string[] param)
        {
            this.TxtMsg.Text = string.Format(msg, param);
        }

        private async void Init()
        {
            await InitAsync();
        }

        private Task InitAsync()
        {
            Task t = new Task(() =>
            {
                AccountList.Clear();

                this.Invoke(new MethodInvoker(() =>
                {
                    this.ListView.Items.Clear();
                }));

                try
                {
                    foreach (KeyValuePair<string, QDataSection> p in QSettings.Account.Sections)
                    {
                        QDataSection ds = p.Value;
                        CAccount account = new CAccount()
                        {
                            ID = p.Key,
                            Platform = AES.AESDecrypt(ds["platform"], Key),
                            Account = AES.AESDecrypt(ds["account"], Key),
                            Password = AES.AESDecrypt(ds["password"], Key),
                            Remarks = AES.AESDecrypt(ds["remarks"], Key)
                        };

                        ListViewItem item = new ListViewItem(account.Platform);
                        item.SubItems.Add(account.Account);
                        item.SubItems.Add("不给看");
                        item.SubItems.Add(account.Remarks);
                        item.Tag = p.Key;

                        AccountList.Add(p.Key, account);

                        this.Invoke(new MethodInvoker(() =>
                        {
                            this.ListView.Items.Add(item);
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Qdb.Error(QFrameworkOne.Diagnostics.QDebugErrorType.Fatal, ex.Message, "FrmMain.InitAsync");
                    this.Invoke(new MethodInvoker(() =>
                    {
                        ShowMsg("密码错误！");
                    }));
                }

                //this.Invoke(new MethodInvoker(() => { ShowMsg("双击可以复制密码!"); }));
            });

            t.Start();
            return t;
        }

        private void Save()
        {
            foreach (KeyValuePair<string, CAccount> p in AccountList)
            {
                CAccount account = p.Value;

                Application.DoEvents();
                ShowMsg("正在保存 {0}", account.Account);

                QSettings.Account[p.Key]["platform"] = AES.AESEncrypt(account.Platform, Key);
                QSettings.Account[p.Key]["account"] = AES.AESEncrypt(account.Account, Key);
                QSettings.Account[p.Key]["password"] = AES.AESEncrypt(account.Password, Key);
                QSettings.Account[p.Key]["remarks"] = AES.AESEncrypt(account.Remarks, Key);
            }
        }

        private void SetBtnEnabled(bool status)
        {
            this.BtnEdit.Enabled = status;
            this.BtnDel.Enabled = status;
        }

        private void BtnEnterKey_Click(object sender, EventArgs e)
        {
            FrmKey fKey = new FrmKey();
            fKey.Title = "请输入密码解锁";
            fKey.ShowDialog();
            if (fKey.DialogResult == DialogResult.OK)
            {
                Key = fKey.Key;
                ShowMsg("解锁成功！现在是：{0}", DateTime.Now.ToString());
                Init();
            }
        }

        private void BtnRndPwd_Click(object sender, EventArgs e)
        {
            //生成10位随机密码
            this.TxtPassword.Text = GenerateSimplePassword.RandomPassword(10, true, true, true, false);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            CAccount account = new CAccount()
            {
                ID = CAccount.GetID(),
                Platform = this.CmbPlatform.Text,
                Account = this.TxtAccount.Text,
                Password = this.TxtPassword.Text,
                Remarks = this.TxtRemark.Text
            };
            AccountList.Add(account.ID, account);

            ListViewItem item = new ListViewItem(account.Platform);
            item.SubItems.Add(account.Account);
            item.SubItems.Add(account.Password);
            item.SubItems.Add(account.Remarks);
            item.Tag = account.ID;

            this.ListView.Items.Add(item);
            ShowMsg("添加账号 {0}", account.ID);

            this.CmbPlatform.Text = "";
            this.TxtAccount.Text = "";
            this.TxtPassword.Text = "";
            this.TxtRemark.Text = "";
        }

        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Save();
        }

        private void ListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (SelectedAccount != null)
            {
                Clipboard.SetText(SelectedAccount.Password);
                ShowMsg("已经把 {0} 的密码复制到剪贴板！", SelectedAccount.Account);
            }
        }

        private void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ListView.SelectedItems.Count > 0)
            {
                ListViewItem item = this.ListView.SelectedItems[0];
                string id = item.Tag.ToString();
                if (AccountList.ContainsKey(id))
                {
                    SelectedAccount = AccountList[id];
                    ShowMsg("当前选择：{0}", SelectedAccount.Account);
                    this.CmbPlatform.Text = SelectedAccount.Platform;
                    this.TxtAccount.Text = SelectedAccount.Account;
                    this.TxtPassword.Text = SelectedAccount.Password;
                    this.TxtRemark.Text = SelectedAccount.Remarks;
                    SetBtnEnabled(true);
                }
                else
                {
                    ShowMsg("账号 {0} 不存在！", id);
                    SetBtnEnabled(false);
                }
            }
            else
            {
                this.CmbPlatform.Text = "";
                this.TxtAccount.Text = "";
                this.TxtPassword.Text = "";
                this.TxtRemark.Text = "";

                SelectedAccount = null;
                SetBtnEnabled(false);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (SelectedAccount != null)
            {
                CAccount account = new CAccount()
                {
                    ID = SelectedAccount.ID,
                    Platform = this.CmbPlatform.Text,
                    Account = this.TxtAccount.Text,
                    Password = this.TxtPassword.Text,
                    Remarks = this.TxtRemark.Text
                };

                SelectedAccount = account;
                AccountList[account.ID] = account;

                ShowMsg("账号 {0} - {1} 已经更新。", account.ID, account.Account);
            }
        }

        private void BtnModifyKey_Click(object sender, EventArgs e)
        {
            string pwd1;
            string pwd2;

            FrmKey fKey = new FrmKey();
            fKey.Title = "请输入新的密码：";
            fKey.ShowDialog();
            if (fKey.DialogResult == DialogResult.OK)
            {
                pwd1 = fKey.Key;
                fKey = new FrmKey();
                fKey.Title = "请再次输入新密码";
                fKey.ShowDialog();
                if (fKey.DialogResult == DialogResult.OK)
                {
                    pwd2 = fKey.Key;
                    if (pwd1 == pwd2)
                    {
                        this.Key = pwd2;
                        ShowMsg("密码修改成功！将在下次启动软件生效。");
                    }
                    else
                        ShowMsg("两次密码不一样！");
                }
                else
                    ShowMsg("操作取消了 pwd2");
            }
            else
                ShowMsg("操作取消了 pwd1");
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            if (SelectedAccount != null)
            {
                CAccount account = SelectedAccount;
                DialogResult dr = MessageBox.Show("确认删除 " + SelectedAccount.Account + " ？", "想清楚好吗", MessageBoxButtons.YesNo);

                if (dr == DialogResult.Yes)
                {
                    AccountList.Remove(account.ID);
                    this.ListView.Items.Remove(this.ListView.SelectedItems[0]);

                    ShowMsg("已经删除 {0} 了", account.Account);
                }
                else if (dr == DialogResult.No)
                    ShowMsg("大家当无事发生过。");
            }
        }

        private void BtnAutoRemarks_Click(object sender, EventArgs e)
        {
            this.TxtRemark.Text += DateTime.Now.ToString();
        }

        private void BtnAbout_Click(object sender, EventArgs e)
        {
            QApp.About();
        }
    }
}
