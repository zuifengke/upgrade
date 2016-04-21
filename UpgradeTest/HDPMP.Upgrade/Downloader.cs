using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

namespace Heren.HDPMP.Upgrade
{
    /// <summary>
    /// ftp���ظ�����
    /// </summary>
    internal class Downloader
    {
        private readonly string rootPath;
        private readonly string username;
        private readonly string password;

        private bool isReportRate = false;
        public bool ReportRate
        {
            set
            {
                isReportRate = value;
            }
        }

        private FtpWebRequest ftpReq;

        public event EventHandler<FileDownloadEventArgs> AfterDownLoadFailed;
        public event EventHandler<FileCompletedPrecentEventArgs> CurRateCompleted;

        public Downloader(string user, string pwd, string root)
        {
            rootPath = root;
            username = user;
            password = pwd;
        }

        private void CreateNewRequest(string path)
        {
            ftpReq = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            ftpReq.UseBinary = true;
            ftpReq.Credentials = new NetworkCredential(username, password);
            ftpReq.Proxy = null;
            ftpReq.KeepAlive = true;
            ftpReq.UsePassive = false;
            ftpReq.Method = WebRequestMethods.Ftp.DownloadFile;
        }

        /// <summary>
        /// ��ȡ�ļ���С
        /// </summary>
        /// <param name="path">�ļ�·��</param>
        /// <returns>�ļ���С</returns>
        public long GetFileSize(string path)
        {
            long length = 0;

            ftpReq = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            ftpReq.UseBinary = true;
            ftpReq.Credentials = new NetworkCredential(username, password);
            ftpReq.Proxy = null;
            ftpReq.KeepAlive = true;
            ftpReq.UsePassive = false;
            ftpReq.Method = WebRequestMethods.Ftp.GetFileSize;

            using (FtpWebResponse response = (FtpWebResponse)ftpReq.GetResponse())
            {
                length = response.ContentLength;
            }
            return length;
        }

        /// <summary>
        /// ftp �����ļ�
        /// </summary>
        /// <param name="name">�ļ���</param>
        public void Download(string name)
        {
            Download(rootPath + name, Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
        }

        /// <summary>
        /// ftp �����ļ�
        /// </summary>
        /// <param name="path">�ļ�·��</param>
        /// <param name="dir">�����ļ���</param>
        public void Download(string path, string dir)
        {
            int retry = 0;

            while (true)
            {
                if (string.IsNullOrEmpty(path))
                    return;

                if (retry > 0)
                {
                    Logger.Insert(string.Format("�� {0} �����������ļ� {1}", retry.ToString(), path));
                }

                string destPath = dir + @"\" + Path.GetFileName(path);

                Logger.Insert("���� " + path);

                try
                {
                    long size = 0;
                    if (isReportRate)
                        size = GetFileSize(path);

                    CreateNewRequest(path);
                    using (WebResponse response = ftpReq.GetResponse())
                    using (FileStream writeStream = new FileStream(destPath, FileMode.Create))
                    {
                        Stream responseStream = response.GetResponseStream();

                        int Length = 2048;
                        Byte[] buffer = new Byte[Length];
                        int bytesRead = responseStream.Read(buffer, 0, Length);

                        int count = 0;
                        while (bytesRead > 0)
                        {
                            writeStream.Write(buffer, 0, bytesRead);
                            bytesRead = responseStream.Read(buffer, 0, Length);

                            if (isReportRate)
                            {
                                count++;
                                if (count % 1000 == 0)
                                {
                                    int curSize = (int)(((double)(count * Length) / (double)size * 100) * 0.75);
                                    CurRateCompleted(this, new FileCompletedPrecentEventArgs(curSize));
                                }
                            }
                        }
                    }
#if Debug
                    //Console.WriteLine("end:" + path);
#endif
                    break;
                }
                catch (Exception ex)
                {
#if Debug
                    Console.WriteLine("exception:" + ex.Message);
                    //Console.WriteLine("exception:" + ex.Data.ToString());
#endif

                    Logger.Insert(string.Format("�����쳣��path:{0}\r\n{1}", path, ex.Message));
                    Thread.Sleep(2000);

                    if (++retry > 5)
                    {
#if Debug
                        Logger.Insert("����ʧ��" + path);
#endif

                        if (AfterDownLoadFailed != null)
                            AfterDownLoadFailed(this, new FileDownloadEventArgs(path.Replace(rootPath, "")));

                        break;
                    }
                }
            }
        }
    }
}
