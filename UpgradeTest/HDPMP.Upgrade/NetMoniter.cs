using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Heren.HDPMP.Upgrade
{
    /// <summary>
    /// 网络连接情况监测类
    /// </summary>
    internal class NetMoniter
    {
        private int m_failCount = 0;
        public event EventHandler OnNetworkConnectedFailed;
        private string m_observeIP;
        private System.Threading.Timer m_timer;
        private AutoResetEvent m_event = new AutoResetEvent(false);

        public NetMoniter(string IP)
        {
            Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            MatchCollection result = ip.Matches(IP);
            if (result.Count > 0)
                m_observeIP = result[0].ToString();
        }

        public void Monitor()
        {
            //using (m_timer = new System.Threading.Timer(o => CheckNetwork(), null, 0, 1000))
            //m_event.WaitOne();
            TimerCallback tcb = this.CheckNetwork;
            using (m_timer = new System.Threading.Timer(tcb, null, 0, 1000))
            {
                m_event.WaitOne();
            }
        }

        /// <summary>
        /// 监测网络是否正常连接
        /// </summary>
        private void CheckNetwork(object o)
        {
            Ping myPing = new Ping();
            byte[] buffer = new byte[32];
            int timeout = 1000;
            PingOptions pingOptions = new PingOptions();
            PingReply reply = myPing.Send(m_observeIP, timeout, buffer, pingOptions);

            if (reply.Status != IPStatus.Success)
            {
#if Debug
                Console.WriteLine(m_failCount + " times network connection fail .");
#endif
                if (m_failCount++ >= 5)
                {
                    m_event.Set();
                    if (OnNetworkConnectedFailed != null)
                        OnNetworkConnectedFailed(this, new EventArgs());
                }
            }
        }
    }
}
