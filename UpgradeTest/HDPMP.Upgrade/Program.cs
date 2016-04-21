using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using Heren.Common.Libraries;

namespace Heren.HDPMP.Upgrade
{

    class Program
    {
        static Mutex mutex;

        static string usage = "用法：\r\n" +
            " VersionNo:     版本号\r\n" +
            " FtpDirRoot:    ftp路径\r\n" +
            " LocalDirPath:  本地同步文件夹路径\r\n" +
            " UserName:      ftp用户名\r\n" +
            " Password:      ftp密码\r\n";

        static string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        static void Main(string[] args)
        {
            LogManager.Instance.TextLogOnly = true;
            SystemConfig.Instance.ConfigFile = string.Format("{0}\\MedQCSys.xml", Application.StartupPath);

            mutex = new Mutex(true, "Upgrade.exe");
            if (!mutex.WaitOne(TimeSpan.Zero, false))
                return;
            //程序退出，保存日志
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
         
            if (args.Length >= 1 && args.Length <= 5)
            {
                try
                {
                    Dictionary<string,string> configs = ParseCmdLine(args);
                    Syner syner = new Syner(configs["VersionNo"], configs["FtpDirRoot"], configs["LocalDirPath"], configs["UserName"], configs["Password"]);

                    if (args.Length == 1)
                        syner.SyncVersionNo();
                    else
                        syner.Sync();
                }
                catch (Exception ex)
                {
                    Logger.Insert(ex.Message + "\r\n" + ex.StackTrace);
                }
            }
            else
            {
                MessageBox.Show("传入参数错误！ \r\n" + usage + "\r\n");
            }
        }
        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logger.Save();
        }

        private static Dictionary<string, string> ParseCmdLine(string[] args)
        {
            Dictionary<string, string> configs = new Dictionary<string, string>();

            if (args != null)
            {
                foreach (string arg in args)
                {
                    if (arg.StartsWith("FtpDirRoot:"))
                        configs.Add("FtpDirRoot", arg.Replace("FtpDirRoot:", "").TrimEnd('\\'));
                    else if (arg.StartsWith("UserName:"))
                        configs.Add("UserName", arg.Replace("UserName:", "").TrimEnd('\\'));
                    else if (arg.StartsWith("Password:"))
                        configs.Add("Password", arg.Replace("Password:", "").TrimEnd('\\'));
                    else if (arg.StartsWith("LocalDirPath:"))
                        configs.Add("LocalDirPath", arg.Replace("LocalDirPath:", "").TrimEnd('\\'));
                    else if (arg.StartsWith("VersionNo:"))
                        configs.Add("VersionNo", arg.Replace("VersionNo:", "").TrimEnd('\\'));
                }
            }

            if (!configs.ContainsKey("LocalDirPath"))
                configs.Add("LocalDirPath", exePath);
            if (!configs.ContainsKey("VersionNo"))
                configs.Add("VersionNo", string.Empty);
            if (!configs.ContainsKey("UserName"))
                configs.Add("UserName", string.Empty);
            if (!configs.ContainsKey("Password"))
                configs.Add("Password", string.Empty);
            if (!configs.ContainsKey("FtpDirRoot"))
                configs.Add("FtpDirRoot", string.Empty);

            return configs;
        }
    }
}
