using System;
using System.Collections.Generic;
using System.Text;

namespace Heren.HDPMP.HashGener
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> configs = ParseCmdLine(args);
            HashGener gen = new HashGener(configs["TargetDirPath"], configs["SaveDirPath"], configs["MetaFileName"]);
            Logger.Start("Generate");
            gen.Generate();
            Logger.Stop("Generate");
            Console.WriteLine(Logger.Msg);
            Console.WriteLine("目录文件生成成功！");
            Console.Read();
        }
        private static Dictionary<string, string> ParseCmdLine(string[] args)
        {
            Dictionary<string, string> configs = new Dictionary<string, string>();

            if (args != null)
            {
                foreach (string arg in args)
                {
                    if (arg.StartsWith("TargetDirPath:"))
                        configs.Add("TargetDirPath", arg.Replace("TargetDirPath:", "").TrimEnd('\\'));
                    else if (arg.StartsWith("SaveDirPath:"))
                        configs.Add("SaveDirPath", arg.Replace("SaveDirPath:", "").TrimEnd('\\'));
                    else if (arg.StartsWith("MetaFileName:"))
                        configs.Add("MetaFileName", arg.Replace("MetaFileName:", "").TrimEnd('\\'));
                }
            }

            if (!configs.ContainsKey("TargetDirPath"))
                configs.Add("TargetDirPath", Environment.CurrentDirectory);
            if (!configs.ContainsKey("SaveDirPath"))
                configs.Add("SaveDirPath", Environment.CurrentDirectory);
            if (!configs.ContainsKey("MetaFileName"))
                configs.Add("MetaFileName", "dir.meta");

            return configs;
        }
    }
}
