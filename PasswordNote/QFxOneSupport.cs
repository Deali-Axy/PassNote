#define Release
#undef Debug

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QFrameworkOne;
using QFrameworkOne.Application;
using QFrameworkOne.Application.Activation;
using QFrameworkOne.Application.Update;
using QFrameworkOne.Application.Locale;
using QFrameworkOne.Module;
using QFrameworkOne.Diagnostics;

/// <summary>
/// QFxOneSupport
/// Author : DealiAxy
/// Code Version : 1.0
/// </summary>

namespace PasswordNote
{
    public static class QApp
    {
        public static QApplication _QApp = new QApplication();
        private static QUpdate _QUpdate;
        public static string ID
        {
            get { return _QApp.ID; }
        }
        public static string Name
        {
            get { return _QApp.Name; }
            set { _QApp.Name = value; }
        }
        public static string PackageName
        {
            get => _QApp.PackageName;
            set { _QApp.PackageName = value; }
        }
        public static string Title
        {
            get { return _QApp.Title; }
            set { _QApp.Title = value; }
        }
        public static string Author
        {
            get { return _QApp.Author; }
            set { _QApp.Author = value; }
        }
        public static string AuthorMail
        {
            get { return _QApp.AuthorMail; }
            set { _QApp.AuthorMail = value; }
        }
        public static string Description
        {
            get { return _QApp.Description; }
            set { _QApp.Description = value; }
        }
        public static QComponentVersion Version
        {
            get { return _QApp.Version; }
            set { _QApp.Version = value; }
        }
        public static string Website
        {
            get { return _QApp.Website; }
            set { _QApp.Website = value; }
        }

        public static QConfig Config => _QApp.Config;
        public static QMultiLang MultiLang => _QApp.MultiLang;

        /// <summary>
        /// 应用初始化，只需要调用这一个方法就够了
        /// </summary>
        /// <returns></returns>
        public static QApplication Init()
        {
            Version.Major = AssemblyVersion.Major;
            Version.Minor = AssemblyVersion.Minor;
            Version.Update = AssemblyVersion.Build;
            _QApp.Register();
            return _QApp;
        }

        public static void About()
        {
            _QApp.AboutForm();
        }

        /// <summary>
        /// 多线程应用激活
        /// </summary>
        /// <returns>激活操作的线程</returns>
        public static Thread Activate()
        {
            Qdb.Log(QDebugLogType.Verbose, "QFrameworkSupport->QApp->Activate() : Create ActivateBackground Thread.");
            Thread t = new Thread(new ThreadStart(ActivateBackground));
            Qdb.Log(QDebugLogType.Verbose, "QFrameworkSupport->QApp->Activate() : Create Thread Finished. Thread ID=" + t.ManagedThreadId);
            t.Start();
            return t;
        }

        /// <summary>
        /// 获取最新版本号，阻塞式
        /// </summary>
        /// <returns></returns>
        public static string GetLastestVersion()
        {
            _QUpdate = new QUpdate(_QApp);
            Qdb.Log(QDebugLogType.Verbose, "开始获取最新版本...");
            return _QUpdate.GetLastestVersion();
        }

        /// <summary>
        /// 检查是否有版本更新，非异步
        /// </summary>
        /// <returns></returns>
        public static bool CheckUpdates()
        {
            _QUpdate = new QUpdate(_QApp);
            return _QUpdate.CheckUpdates();
        }

        /// <summary>
        /// 检查更新GUI，默认为异步获取更新信息，不用阻塞用户界面线程
        /// </summary>
        public static async void CheckUpdatesGUI()
        {
            _QUpdate = new QUpdate(_QApp);
            await _QUpdate.CheckUpdatesGUIAsync();
        }

        /// <summary>
        /// 初始化更新信息，默认为异步获取更新信息，不用阻塞用户界面线程
        /// </summary>
        /// <returns></returns>
        public static QUpdate InitUpdateInfo()
        {
            _QUpdate = new QUpdate(_QApp);
            _QUpdate.InitUpdateInfoAsync();
            return _QUpdate;
        }

        /// <summary>
        /// 显示更新信息窗口，默认为异步获取更新信息，不用阻塞用户界面线程
        /// </summary>
        public static void ShowUpdateInfo()
        {
            _QUpdate = new QUpdate(_QApp);
            _QUpdate.ShowUpdateInfoAsync();
        }

        /// <summary>
        /// 静默激活
        /// </summary>
        /// <returns></returns>
        public static Thread ActivateSilent()
        {
            Qdb.Log(QDebugLogType.Verbose, "QFrameworkSupport->QApp->ActivateSilent() : Create ActivateSilentBackground Thread.");
            Thread t = new Thread(new ThreadStart(ActivateSilentBackground));
            Qdb.Log(QDebugLogType.Verbose, "QFrameworkSupport->QApp->ActivateSilent() : Create Thread Finished. Thread ID=" + t.ManagedThreadId);
            t.Start();
            return t;
        }

        private static void ActivateBackground()
        {
            Qdb.Log(QDebugLogType.Verbose, "QFrameworkSupport->QApp->ActivateBackground() : ActivateBackground Thread Start.");
            if (!QSettings.IsActivated)//首次安装激活
            {
                //开始尝试激活
                QActivation Acti = new QActivation(_QApp);
                Acti.ActiveType = ActiveTypes.NewInstallation;
                ActiveResult aR = Acti.Activate();
                //保存用户在QFrmActivate窗口中输入的用户名和邮箱，供以后更新激活用
                QSettings.Username = Acti.UserName;
                QSettings.UserMail = Acti.UserMail;
                QSettings.AppVersion = _QApp.Version.ToString();
                QSettings.Save();
                if (aR == ActiveResult.Success)
                {
                    QSettings.IsActivated = true;
                    QSettings.Save();
                }
            }
            else if (_QApp.Version.ToString() != QSettings.AppVersion) //更新激活
            {
                //先把激活状态设置为false
                QSettings.IsActivated = false;
                QSettings.Save();
                //开始尝试激活
                QActivation Acti = new QActivation(_QApp);
                Acti.ActiveType = ActiveTypes.UpdateInstallation;
                Acti.UserName = QSettings.Username;
                Acti.UserMail = QSettings.UserMail;
                ActiveResult aR = Acti.Activate();
                if (aR == ActiveResult.Success)
                {
                    QSettings.IsActivated = true;
                    QSettings.AppVersion = _QApp.Version.ToString();
                    QSettings.Save();
                }
            }
            else
                Qdb.Log(QDebugLogType.Verbose, "QFrameworkSupport->QApp->ActivateBackground() : 应用已经激活。跳过激活步骤。");
        }

        private static void ActivateSilentBackground()
        {
            Qdb.Log(QDebugLogType.Verbose, "QFrameworkSupport->QApp->ActivateSilentBackground() : ActivateBackground Thread Start.");
            if (!QSettings.IsActivated)//首次安装激活
            {
                Qdb.Log(QDebugLogType.Verbose, "QFrameworkSupport->QApp->ActivateSilentBackground() : 首次安装激活");
                //开始尝试激活
                QActivation Acti = new QActivation(_QApp);
                Acti.ActiveType = ActiveTypes.Silence;
                ActiveResult aR = Acti.Activate();
                //保存用户在QFrmActivate窗口中输入的用户名和邮箱，供以后更新激活用
                QSettings.Username = "SilenceActivate";
                QSettings.UserMail = "SilenceActivate";
                QSettings.AppVersion = _QApp.Version.ToString();
                QSettings.Save();
                if (aR == ActiveResult.Success)
                {
                    QSettings.IsActivated = true;
                    QSettings.Save();
                }
            }
            else if (_QApp.Version.ToString() != QSettings.AppVersion) //更新激活
            {
                Qdb.Log(QDebugLogType.Verbose, "QFrameworkSupport->QApp->ActivateSilentBackground() : 更新激活");
                //先把激活状态设置为false
                QSettings.IsActivated = false;
                QSettings.Save();
                //开始尝试激活
                QActivation Acti = new QActivation(_QApp);
                Acti.ActiveType = ActiveTypes.UpdateInstallation;
                Acti.UserName = QSettings.Username;
                Acti.UserMail = QSettings.UserMail;
                ActiveResult aR = Acti.Activate();
                if (aR == ActiveResult.Success)
                {
                    QSettings.IsActivated = true;
                    QSettings.AppVersion = _QApp.Version.ToString();
                    QSettings.Save();
                }
            }
            else
                Qdb.Log(QDebugLogType.Verbose, "QFrameworkSupport->QApp->ActivateSilentBackground() : 应用已经激活。跳过激活步骤。");
        }

        #region 程序集特性访问器
        /// <summary>
        /// 获取程序集版本信息
        /// </summary>
        private static Version AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }
        #endregion
    }

    public static class Qdb
    {
        private static QDebug qdb = new QDebug()
        {
            DebugCodeLocation = "QFxOneSupport",
#if (Debug)
            ConsoleOutput = true,
            DebugOutput = true,
            FileOutput = false
#endif
        };

        public static void Log(QDebugLogType type, string msg, string location = "")
        {
            qdb.Log(msg, type, location);
        }

        public static void Error(QDebugErrorType type, string msg, string location = "")
        {
            qdb.Error(msg, type, location);
        }
    }

    public static class QSettings
    {
        private static string Node = "QFxOneSupport";

        public static bool IsActivated;
        public static string AppVersion;
        public static string Username;
        public static string UserMail;

        public static QData Default => QApp.Config["Default"];

        public static QData Account => QApp.Config["Account"];

        static QSettings() => Load();

        public static void Load()
        {
            IsActivated = QApp.Config[Node]["Activate"]["isactivated"] == "true";
            AppVersion = QApp.Config[Node]["Activate"]["appversion"];
            Username = QApp.Config[Node]["Activate"]["username"];
            UserMail = QApp.Config[Node]["Activate"]["usermail"];
        }

        public static void Save()
        {
            QApp.Config[Node]["Activate"]["isactivated"] = IsActivated ? "true" : "false";
            QApp.Config[Node]["Activate"]["username"] = Username;
            QApp.Config[Node]["Activate"]["usermail"] = UserMail;
            QApp.Config[Node]["Activate"]["appversion"] = AppVersion;
            QApp.Config[Node].Close();
        }
    }
}
