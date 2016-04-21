using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace Heren.HDPMP.Upgrade
{
    /// <summary>
    /// ����ѡ���ȡ������
    /// </summary>
    internal class Optioner
    {
        private bool m_init = false;
        private SyncOption m_syncOption = SyncOption.Files;
        public SyncOption SyncOption
        {
            get
            {
                if (!m_init)
                {
                    InitOption();
                    m_init = !m_init;
                }

                return m_syncOption;
            }
        }

        private string m_metaPath = string.Empty;

        public Optioner(string metaPath)
        {
            m_metaPath = metaPath;
        }

        /// <summary>
        /// ��ȡ����ѡ��жϹ���ֻ��һ���ļ�����Ϊ�ǰ�װ����ʽ
        /// </summary>
        private void InitOption()
        {
            if (File.Exists(m_metaPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(m_metaPath);
                XmlNode root = doc.SelectSingleNode("Files");
                XmlNodeList files = root.SelectNodes("file");

                int count = 0;
                foreach (XmlNode f in files)
                {
                    if (!f.Attributes["path"].Value.EndsWith("update.config") &&
                        !f.Attributes["path"].Value.EndsWith("Upgrade.exe") &&
                        !f.Attributes["path"].Value.EndsWith("HashGener.exe"))
                    {
                        count++;
                    }
                }

                if (count == 1)
                    m_syncOption = SyncOption.Installer;
            }
        }
    }
}
