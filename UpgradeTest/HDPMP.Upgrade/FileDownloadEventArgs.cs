using System;
using System.Collections.Generic;
using System.Text;

namespace Heren.HDPMP.Upgrade
{
    internal class FileDownloadEventArgs : EventArgs
    {
        private readonly string m_fileName;
        public string FileName { get { return m_fileName; } }

        public FileDownloadEventArgs(string fileName)
        {
            m_fileName = fileName;
        }
    }
}
