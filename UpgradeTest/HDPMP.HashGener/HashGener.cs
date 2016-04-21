using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Heren.HDPMP.HashGener
{
    class HashGener
    {
        private string targetDirPath;
        private string saveDirPath;
        private string metaFileName;

        public HashGener(string target, string save, string meta)
        {
            targetDirPath = target;
            saveDirPath = save;
            metaFileName = meta;
        }

        public void Generate()
        {
            if (!Directory.Exists(targetDirPath) || !Directory.Exists(saveDirPath))
                return;

            Dictionary<string, string> hashMap = new Dictionary<string, string>();
            LoadFilesHash(targetDirPath, targetDirPath, ref hashMap);
            if (hashMap.Count > 0)
            {
                SaveHashDict(hashMap);
            }
        }

        private void SaveHashDict(Dictionary<string, string> hashMap)
        {
            StringBuilder result = new StringBuilder();
            result.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n");
            result.Append("<Files>\r\n");

            foreach (KeyValuePair<string,string> file in hashMap)
            {
                if (file.Value.Contains("metaFileName"))
                    continue;

                result.Append(string.Format("   <file path=\"{0}\" hash=\"{1}\"/>\r\n", file.Key, file.Value));
            }

            result.Append("</Files>");

            string saveFile = saveDirPath + @"\" + metaFileName;

            if (!Directory.Exists(saveDirPath))
                Directory.CreateDirectory(saveDirPath);

            if (File.Exists(saveFile))
                File.Delete(saveFile);

            File.WriteAllText(saveFile, result.ToString());
        }

        private void LoadFilesHash(string root, string dirPath, ref Dictionary<string, string> hashMap)
        {
            string[] files = Directory.GetFiles(dirPath);
            foreach (string f in files)
            {
                if (f.Contains("HashGener.exe") || f.Contains("HashGener.exe.lnk"))
                    continue;

                Console.WriteLine(f + "\r");

                string hash = GetMD5Hash(f);
                hashMap.Add(f.Replace(root, ""), hash);
            }

            string[] dirs = Directory.GetDirectories(dirPath);

            if (dirs.Length == 0 && files.Length == 0)
            {
                hashMap.Add(dirPath.Replace(root, "") + "\\", "");
            }
            else
            {
                foreach (string dir in dirs)
                {
                    LoadFilesHash(root, dir, ref hashMap);
                }
            }
        }

        private string GetMD5Hash(string path)
        {
            byte[] hash = new MD5CryptoServiceProvider().ComputeHash(File.ReadAllBytes(path));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("x2").ToLower());
            }
            return sb.ToString();
        }
    }
}
