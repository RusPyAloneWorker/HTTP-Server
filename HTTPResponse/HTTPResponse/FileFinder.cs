using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace HTTPResponse
{
    public class FileFinder
    {
        public static byte[] GetFile(string rawPath, string fileName, string path)
        {
            byte[] result = null;
            string fullPath = path + rawPath;
            Console.WriteLine(fullPath);
            if (Directory.Exists(fullPath))
            {
                fullPath += path;
                if (Directory.Exists(fullPath))
                {
                    result = File.ReadAllBytes(fullPath);
                }
            }
            else if (File.Exists(fullPath))
            {
                result = File.ReadAllBytes(fullPath);
            }
            return result;
        }
    }
}
