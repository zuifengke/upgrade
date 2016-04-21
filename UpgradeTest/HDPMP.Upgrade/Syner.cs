using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Windows.Forms;
using System.Security.Cryptography;
using Heren.Common.Libraries;

namespace Heren.HDPMP.Upgrade
{
    /// <summary>
    /// ��������
    /// </summary>
    internal class Syner
    {
        private string ftpDirRoot;
        private string userName;
        private string password;
        private string localPath;
        private string versionNo;

        private ProgressForm rateBar;

        private int syncCount = 0;
        private volatile int finishedCount = 0;
        private AutoResetEvent downWait = new AutoResetEvent(false);

        private bool isRollbackOn = false;

        private ConfigInfo configs;
        private Optioner updOption;
        private MessSaver cleaner;

        private string exePath = Path.GetDirectoryName(Application.ExecutablePath);
        public Syner(string version, string ftpRoot, string locPath, string user, string pwd)
        {
            versionNo = version;
            ftpDirRoot = ftpRoot;
            localPath = locPath;
            userName = user;
            password = pwd;
        }

        public void Sync()
        {
            try
            {
                SyncFtp(localPath, ftpDirRoot);
            }
            catch (Exception ex)
            {
                Logger.Insert(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void SyncFtp(string localDir, string ftpRoot)
        {
            if (!Directory.Exists(localDir))
                Directory.CreateDirectory(localDir);

            cleaner = new MessSaver(localDir);
            cleaner.SetupEnv();

            Downloader ftp = new Downloader(userName, password, ftpRoot);
            if (!LoadUpdateConfigs(ftp, ref configs))
            {
                Logger.Insert("��ȡFTP�����ļ�ʧ�ܣ�");
                return;
            }

            Dictionary<string, string> remoteHashes = new Dictionary<string, string>();
            Dictionary<string, string> localHashes = new Dictionary<string, string>();

            if (!LoadMetaList(ftp, ref remoteHashes, ref localHashes))
                return;

            if (SelfUpdate(remoteHashes, ftp))
                return;

            CloseApplications();

            rateBar = new ProgressForm();
            ThreadPool.QueueUserWorkItem(delegate(object obj)
                {
                    rateBar.ShowDialog();
                });

            List<string> localFiles = new List<string>();
            LoadAllFiles(localDir, localDir, ref localFiles);

            syncCount = GetSyncCount(localDir, localFiles, remoteHashes);

            NetMoniter mon = new NetMoniter(ftpRoot);
            mon.OnNetworkConnectedFailed += OnNetworkConnectedFailed;
            ThreadPool.QueueUserWorkItem(
                delegate
                {
                    mon.Monitor();
                }
                );

            foreach (KeyValuePair<string,string> f in remoteHashes)
            {
                if (f.Key.Contains("dir.meta"))
                    continue;

                if (f.Value == string.Empty)
                {
                    string extDir = localDir + f.Key.TrimEnd('\\');
                    if (!Directory.Exists(extDir))
                    {
                        Logger.Start("���� " + f.Key);
                        Directory.CreateDirectory(extDir);
                        Logger.Stop("���� " + f.Key);
                    }
                    continue;
                }

                string locName = localDir + f.Key;
                string saveDir = localDir + Path.GetDirectoryName(f.Key);

                if (localFiles.Contains(f.Key))
                {
                    if (IsFileShouldIgnore(f.Key))
                        continue;

                    string localHash = GetMD5Hash(locName);
                    if (localHash == f.Value)
                        continue;
                    else
                        cleaner.Backup(locName);
                }

                if (!Directory.Exists(saveDir))
                    Directory.CreateDirectory(saveDir);

                if (f.Key != "\\Upgrade.exe")
                {
                    string fileName = ftpRoot + f.Key;
                    string saveDest = saveDir;

                    ftp.AfterDownLoadFailed += AfterDownLoadFailed;

                    if (updOption.SyncOption == SyncOption.Installer)
                    {
                        ftp.ReportRate = true;
                        ftp.CurRateCompleted += SingleFileCurRateCompleted;
                    }
                    //ftp.Download(fileName, saveDest);
                    //int curPrecent = (int)(((double)finishedCount / (double)syncCount) * 100);
                    //SetProgress(curPrecent, "����ļ�: ", f.Key);
                    //Action<string, string> actDownload = new Action<string, string>(ftp.Download);
                    //actDownload.BeginInvoke(fileName, saveDest, AfterFileDownloaded, f.Key);
                    delDown del = new delDown(ftp.Download);
                    del.BeginInvoke(fileName, saveDest, AfterFileDownloaded, f.Key);
                }
            }

            if (syncCount > 0)
            {
                downWait.WaitOne();
            }

            mon.OnNetworkConnectedFailed -= OnNetworkConnectedFailed;

            ftp.AfterDownLoadFailed -= AfterDownLoadFailed;
            if (updOption.SyncOption == SyncOption.Installer)
            {
                ftp.CurRateCompleted -= SingleFileCurRateCompleted;
            }

            SetProgress("�������...");
            Thread.Sleep(2000);
            rateBar.CloseForm();
            AfterSyncFinished(localDir);

            ExecuteCmds();
        }
        delegate void delDown(string path, string dir);
        private void AfterFileDownloaded(IAsyncResult ar)
        {
            if (++finishedCount == syncCount)
            {
                downWait.Set();
            }
            int curPrecent = (int)(((double)finishedCount / (double)syncCount) * 100);
            SetProgress(curPrecent, "����ļ�: ", ar.AsyncState.ToString());
        }

        private void OnNetworkConnectedFailed(object sender, EventArgs e)
        {
#if Debug
            Console.WriteLine("OnNetworkConnectedFailed");
#endif
            AfterDownLoadFailed(null, null);
        }

        private void SingleFileCurRateCompleted(object sender, FileCompletedPrecentEventArgs args)
        {
            SetProgress(args.CurPrecent, string.Empty);
        }

        private void AfterDownLoadFailed(object sender, FileDownloadEventArgs args)
        {
            if (isRollbackOn)
                return;

            isRollbackOn = true;

            try
            {
#if Debug
                Console.WriteLine("��ʼ�ع�");
#endif
                SetProgress("��ʼ�ع�...");
                cleaner.RollBack();
                SetProgress("��ɻع�...");

#if Debug
                Console.WriteLine("��ɻع�");
#endif

                if (rateBar != null)
                    rateBar.CloseForm();
            }
            catch (Exception ex)
            {
                Logger.Insert("�ع��쳣��\r\n" + ex.Message);
            }

#if Debug
            Console.WriteLine("Application.Exit");
#endif
            Environment.Exit(0);
        }

        private void SetProgress(int curPrecent, string operation, string fileName)
        {
            if (fileName != null)
            {
                fileName = fileName.TrimStart('\\');
            }

            SetProgress(curPrecent, operation + fileName);
        }

        private bool LoadMetaList(Downloader ftp, ref Dictionary<string, string> remoteHashes, ref Dictionary<string, string> localHashes)
        {
            string localDir = exePath;

            string dir = localDir + "\\" + "dir.meta";
            string bak = localDir + "\\" + "bak.meta";
            string remote = localDir + "\\" + "remote.meta";

            if (File.Exists(remote))
                File.Delete(remote);

            if (File.Exists(bak))
                File.Delete(bak);

            if (File.Exists(dir))
                File.Move(dir, bak);

            ftp.Download("dir.meta");

            if (File.Exists(dir))
                File.Move(dir, remote);

            if (!File.Exists(remote))
            {
                Logger.Insert("����Ŀ¼�б�ʧ�ܣ�");
                return false;
            }

            updOption = new Optioner(localDir + "\\" + "remote.meta");

            if (File.Exists(bak) && updOption.SyncOption != SyncOption.Installer)
            {
                string bakHash = GetMD5Hash(bak);
                string remoteHash = GetMD5Hash(remote);
                if (bakHash == remoteHash)
                {
                    AfterSyncFinished(localDir);
                    return false;
                }
            }
            else
            {
                File.Delete(bak);
            }

            LoadFileHashes(remote, ref remoteHashes);
            LoadFileHashes(bak, ref localHashes);

            return true;
        }

        private bool LoadUpdateConfigs(Downloader ftp, ref ConfigInfo configs)
        {
            string configName = "update.config";
            string conPath = exePath + "\\" + configName;

            if (File.Exists(conPath))
                File.Delete(conPath);

            ftp.Download(configName);

            if (!File.Exists(conPath))
                return false;

            configs = new ConfigInfo();
            configs.Load(conPath);

            return true;
        }

        private bool SelfUpdate(Dictionary<string, string> remoteHashes, Downloader ftp)
        {
            bool result = false;
            string syerName = "\\Upgrade.exe";

            if (remoteHashes.ContainsKey(syerName))
            {
                string localSynerHash = GetMD5Hash(exePath + syerName);
                string remoteSynerHash = remoteHashes[syerName];

                if (!string.IsNullOrEmpty(remoteSynerHash) && localSynerHash != remoteSynerHash)
                {
                    string synerPath = exePath + syerName;
                    string delSynerPath = synerPath + ".delete";

                    try
                    {
                        if (File.Exists(delSynerPath))
                            File.Delete(delSynerPath);
                        File.Move(synerPath, delSynerPath);
                        ftp.Download(ftpDirRoot + syerName, exePath);
                        Application.Restart();
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Insert(string.Format("������������ʧ��:{0}\r\n{1}", ex.Message, ex.StackTrace));
                        return result;
                    }
                }
            }

            return result;
        }

        private void AfterSyncFinished(string localDir)
        {
            string bak = localDir + "\\" + "bak.meta";
            string remote = localDir + "\\" + "remote.meta";
            string dir = localDir + "\\" + "dir.meta";

            if (File.Exists(bak))
                File.Delete(bak);

            if (File.Exists(dir))
                File.Delete(dir);

            if (File.Exists(remote))
                File.Move(remote, dir);

            if (File.Exists(dir))
                File.Delete(dir);


            cleaner.Dispose();
            SyncVersionNo(localDir);

            if (updOption.SyncOption == SyncOption.Installer)
                CleanInstaller();
        }

        /// <summary>
        /// ��װ��ģʽ������ɺ�ɾ���ϵİ�װ��
        /// ���ݰ汾���Ʋⰲװ������
        /// </summary>
        private void CleanInstaller()
        {
            if (configs.RunCmds.Count == 0)
                return;

            string curInstaller = configs.RunCmds[0];
            if (!curInstaller.EndsWith(".exe"))
                return;

            string[] parts = curInstaller.Split('.');
            if (parts.Length >= 2)
            {
                int nVersion = 0;
                string curVersion = parts[parts.Length - 2];
                if (int.TryParse(curVersion, out nVersion))
                {
                    for (int i = 1; i < nVersion; i++)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int j = 0; j < parts.Length; j++)
                        {
                            if (j == parts.Length - 2)
                                sb.Append(i.ToString() + ".");
                            else if (j == parts.Length - 1)
                                sb.Append(parts[j]);
                            else
                                sb.Append(parts[j] + ".");
                        }

                        string install = exePath + "\\" + sb.ToString();

                        if (File.Exists(install))
                        {
                            try
                            {
                                Logger.Insert("ɾ����װ��:" + install);
                                File.Delete(install);
                            }
                            catch (Exception ex)
                            {
                                Logger.Insert("ɾ����װ��ʧ�ܣ�" + ex.Message);
                            }
                        }
                    }
                }
            }
        }

        public void SyncVersionNo()
        {
            SyncVersionNo(exePath, true);
        }

        private void SyncVersionNo(string localDir)
        {
            SyncVersionNo(localDir, false);
        }

        /// <summary>
        /// ������ɺ󣬸��±��ذ汾��
        /// </summary>
        /// <param name="localDir">�����ļ�Ŀ¼</param>
        /// <param name="force">�Ƿ�ǿ�Ƹ���</param>
        private void SyncVersionNo(string localDir, bool force)
        {
            if (!string.IsNullOrEmpty(versionNo))
            {
                SystemConfig.Instance.Write("Current.Version", versionNo);
            }
        }

        private int GetSyncCount(string localDir, List<string> localFiles, Dictionary<string, string> remoteHashes)
        {
            int count = 0;
            foreach (KeyValuePair<string, string> f in remoteHashes)
            {
                if (f.Key.Contains("dir.meta") || f.Key.Contains("Upgrade.exe"))
                    continue;

                if (f.Value == string.Empty)
                    continue;

                if (localFiles.Contains(f.Key))
                {
                    if (IsFileShouldIgnore(f.Key))
                        continue;

                    string locName = localDir + f.Key;
                    string localHash = GetMD5Hash(locName);
                    if (localHash == f.Value)
                        continue;
                }

                count++;
            }
            return count;
        }

        private void SetProgress(int curPrecent, string operation)
        {
            if (rateBar != null)
            {
                if (curPrecent > 0)
                    rateBar.CurPrecent = curPrecent;
                rateBar.SyncOperation = operation;
            }
        }

        private void SetProgress(string operation)
        {
            SetProgress(0, operation);
        }

        private void CloseApplications()
        {
            if (configs.CloseApps != null)
            {
                foreach (string app in configs.CloseApps)
                    CloseAppByName(app);
            }
        }

        private void CloseAppByName(string appName)
        {
            if (appName.EndsWith(".exe"))
                appName = appName.Replace(".exe", string.Empty);

            foreach (Process p in Process.GetProcessesByName(appName))
            {
                try
                {
                    p.Kill();
                    p.WaitForExit();
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    Logger.Insert(string.Format("�رճ���{0}����", appName) + ex.Message);
                }
            }
        }

        private void ExecuteCmds()
        {
            if (configs.RunCmds != null)
            {
                foreach (string cmd in configs.RunCmds)
                {
                    try
                    {
                        string path = cmd;
                        if (path.StartsWith(@"\"))
                            path = localPath + cmd;
#if Debug
                        Logger.Insert("ִ�����" + path);
#endif
                        Process.Start(path);
                    }
                    catch (Exception ex)
                    {
                        Logger.Insert(string.Format("ִ������ {0} ����", cmd) + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// �ж��ļ��Ƿ���Ҫ����
        /// </summary>
        /// <param name="fileName">�ļ���</param>
        /// <returns>�Ƿ����</returns>
        private bool IsFileShouldIgnore(string fileName)
        {
            if (configs.IgnoreItems == null)
                return false;

            if (configs.IgnoreItems.Contains(fileName))
                return true;

            foreach (string item in configs.IgnoreItems)
            {
                if (item.EndsWith("\\"))
                {
                    if (fileName.StartsWith(item))
                        return true;
                }
            }

            return false;
        }

        private List<string> LoadRemoteDirectories(Dictionary<string, string> remoteFiles)
        {
            List<string> dirs = new List<string>();

            if (remoteFiles != null)
            {
                foreach (KeyValuePair<string, string> f in remoteFiles)
                {
                    string dir = Path.GetDirectoryName(f.Key);
                    if (!dirs.Contains(dir))
                        dirs.Add(dir);

                    LoadAllDirectories(dir, ref dirs);
                }
            }

            return dirs;
        }

        private void LoadAllDirectories(string dir, ref List<string> dirs)
        {
            if (!dir.Contains("\\") || dir.LastIndexOf("\\") == 0)
                return;

            DirectoryInfo parent = Directory.GetParent(dir);
            if (parent != null)
            {
                if (!dirs.Contains("\\" + parent.Name))
                    dirs.Add("\\" + parent.Name);

                LoadAllDirectories(parent.Name, ref dirs);
            }
        }

        private void LoadFileHashes(string file, ref Dictionary<string, string> fileHashes)
        {
            if (!File.Exists(file))
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            XmlNode root = doc.SelectSingleNode("Files");
            XmlNodeList files = root.SelectNodes("file");

            foreach (XmlNode f in files)
            {
                fileHashes.Add(f.Attributes["path"].Value, f.Attributes["hash"].Value);
            }
        }

        /// <summary>
        /// �����ļ� hash ֵ�������ж��ļ��Ƿ���ͬ
        /// </summary>
        /// <param name="path">�ļ�·��</param>
        /// <returns>hash ֵ</returns>
        private string GetMD5Hash(string path)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                byte[] hash = new MD5CryptoServiceProvider().ComputeHash(File.ReadAllBytes(path));
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2").ToLower());
                }
            }
            catch (Exception ex)
            {
                Logger.Insert(ex.Message);
                return string.Empty;
            }

            return sb.ToString();
        }

        private void LoadAllFiles(string root, string dirPath, ref List<string> fileList)
        {
            string[] files = Directory.GetFiles(dirPath);
            foreach (string f in files)
            {
                string fileName = Path.GetFileName(f);
                if (fileName == "Upgrade.exe.lnk" || fileName == "Upgrade.exe")
                    continue;
                fileList.Add(f.Replace(root, ""));
            }

            string[] dirs = Directory.GetDirectories(dirPath);

            if (dirs.Length > 0)
            {
                foreach (string dir in dirs)
                {
                    LoadAllFiles(root, dir, ref fileList);
                }
            }
        }
    }
}
