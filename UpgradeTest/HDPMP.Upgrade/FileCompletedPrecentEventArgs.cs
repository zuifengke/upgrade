using System;
using System.Collections.Generic;
using System.Text;

namespace Heren.HDPMP.Upgrade
{
    internal class FileCompletedPrecentEventArgs : EventArgs
    {
        private readonly int m_curPrecent;
        public int CurPrecent { get { return m_curPrecent; } }

        public FileCompletedPrecentEventArgs(int curPrecent)
        {
            m_curPrecent = curPrecent;
        }
    }

}
