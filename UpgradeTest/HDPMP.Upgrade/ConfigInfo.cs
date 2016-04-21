using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Heren.HDPMP.Upgrade
{
    /// <summary>
    /// ������Ϣ��
    /// </summary>
    internal class ConfigInfo
    {
        private List<string> m_ignoreItems;

        /// <summary>
        /// ����������Ŀ
        /// </summary>
        public List<string> IgnoreItems
        {
            get
            {
                return m_ignoreItems;
            }
        }

        private List<string> m_runCmds;

        /// <summary>
        /// ������ɺ����е�����
        /// </summary>
        public List<string> RunCmds
        {
            get
            {
                return m_runCmds;
            }
        }

        private List<string> m_closeApps;

        /// <summary>
        /// ����ǰ�رյĳ���
        /// </summary>
        public List<string> CloseApps
        {
            get
            {
                return m_closeApps;
            }
        }

        /// <summary>
        /// ���������ļ�
        /// </summary>
        /// <param name="configPath">�����ļ�·��</param>
        public void Load(string configPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(configPath);

            if (doc != null)
            {
                XmlNode config = doc.SelectSingleNode("configuration");

                LoadNodes("ignoreitems", "item", config, out m_ignoreItems);
                IgnoreItems.Add("\\Upgrade.exe");
                IgnoreItems.Add("\\Upgrade.vshost.exe");
                IgnoreItems.Add("\\Upgrade.exe.lnk");

                for (int i = 0; i < IgnoreItems.Count; i++)
                {
                    if (!IgnoreItems[i].StartsWith("\\"))
                        IgnoreItems[i] = "\\" + IgnoreItems[i];
                }

                LoadNodes("runcmds", "cmd", config, out m_runCmds);
                LoadNodes("closeApps", "app", config, out m_closeApps);
            }
        }

        private void LoadNodes(string fatherNodeName, string nodeName, XmlNode baseNode, out List<string> fillList)
        {
            fillList = new List<string>();
            XmlNode node = baseNode.SelectSingleNode(fatherNodeName);

            if (node != null)
            {
                XmlNodeList childs = node.SelectNodes(nodeName);
                foreach (XmlNode i in childs)
                {
                    fillList.Add(i.InnerText);
                }
            }
        }
    }
}
