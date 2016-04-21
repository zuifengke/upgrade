using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Heren.HDPMP.Upgrade
{
    /// <summary>
    /// Éý¼¶Ê§°Ü»Ø¹ö¸¨ÖúÀà
    /// </summary>
    internal class MessSaver : IDisposable
    {
        private string m_tmpDir;
        private string m_target;

        public MessSaver(string target)
        {
            m_target = target;
            m_tmpDir = m_target + "\\_tmp";
        }

        public void SetupEnv()
        {
#if Debug
            Console.WriteLine("SetupEnv");
#endif
            if (Directory.Exists(m_tmpDir))
                RollBack();
            else
                Directory.CreateDirectory(m_tmpDir);

        }

        public void RollBack()
        {
            if (Directory.Exists(m_tmpDir))
            {
                try
                {
                    RollBack(m_tmpDir);
                }
                catch (Exception ex)
                {
                    Logger.Insert("»Ø¹öÊ§°Ü£¡" + ex.Message);
                }
            }
        }

        public void Backup(string fileName)
        {
            string shortName = fileName.Replace(m_target, string.Empty);

            string tmpPath = m_tmpDir + shortName;
            string tmpDir = m_tmpDir + Path.GetDirectoryName(shortName);

            if (!File.Exists(tmpPath))
            {
                if (!Directory.Exists(tmpDir))
                    Directory.CreateDirectory(tmpDir);
                File.Move(fileName, tmpPath);
            }
        }

        private void RollBack(string tmpDir)
        {
            if (!Directory.Exists(tmpDir))
                return;

            string[] files = Directory.GetFiles(tmpDir);

            if (files.Length == 0)
            {
                string dir = tmpDir.Replace("\\_tmp", "");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
            else
            {
                foreach (string f in files)
                {
                    string locFile = f.Replace("\\_tmp", "");

                    if (File.Exists(locFile))
                        File.Delete(locFile);

                    string dir = Path.GetDirectoryName(locFile);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.Move(f, locFile);
                }
            }

            string[] dirs = Directory.GetDirectories(tmpDir);
            foreach (string dir in dirs)
            {
                RollBack(dir);
            }
        }

        public void Dispose()
        {
            if (Directory.Exists(m_tmpDir))
                Directory.Delete(m_tmpDir, true);
        }
    }
}
