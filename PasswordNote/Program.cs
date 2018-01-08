using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PasswordNote
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            QApp.Name = "PasswordNote";
            QApp.Title = "密码笔记本";
            QApp.PackageName = "cn.deali.passwordnote";
            QApp.Description = "使用高强度的AES加密算法加密！所有信息都会加密存储，请放心使用！欢迎访问我的博客：http://blog.deali.cn 查看更多内容。";
            QApp.Init();
            QApp.Activate();
            QApp.CheckUpdatesGUI();

            Application.Run(new FrmMain());
        }
    }
}
