using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Heren.HDPMP.Upgrade
{
    /// <summary>
    /// »’÷æ∏®÷˙¿‡
    /// </summary>
    internal class Logger
    {
        private static StringBuilder msg = new StringBuilder();
        private static Stopwatch sw = Stopwatch.StartNew();
        private static Dictionary<string, long> stamps = new Dictionary<string, long>();

        public static string Msg
        {
            get
            {
                return msg.ToString();
            }
        }

        public static void Start(string location)
        {
            sw.Stop();

            long start = sw.ElapsedMilliseconds;
            if (stamps.ContainsKey(location))
                return;

            stamps.Add(location, start);

            sw.Start();
        }

        public static void Stop(string location)
        {
            sw.Stop();

            long stop = sw.ElapsedMilliseconds;
            if (stamps.ContainsKey(location))
            {
                long start = stamps[location];
                stamps.Remove(location);

                long elapsed = stop - start;
                string fmtLoc = (location + ":").PadRight(25, ' ');
                msg.Append(fmtLoc + elapsed.ToString() + (elapsed > 300 ? "*" : "") + "\r\n");
            }

            if (stamps.Count > 0)
                sw.Start();
            else
                sw.Reset();
        }

        public static void Insert(string location)
        {
            sw.Stop();

            string fmtLoc = location.PadRight(100, ' ') + (sw.ElapsedMilliseconds / 1000).ToString() + "\r\n";
            msg.Append(fmtLoc);

            sw.Start();
        }

        public static void Save()
        {
            if (!string.IsNullOrEmpty(Msg))
            {
                string path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\";
                string fileName = path + "UpdateLog.txt";

                if (File.Exists(fileName))
                    File.Delete(fileName);
                File.WriteAllText(fileName, Msg);
            }
        }
    }

}
