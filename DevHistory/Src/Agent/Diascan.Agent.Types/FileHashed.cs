using System;
using System.IO;

namespace Diascan.Agent.Types
{
    public class FileHashed
    {
        public string FilePath { get; set; }
        public Int64 Size { get; set; }    //  размер в КБ
        public ulong Hashes { get; set; }
        public bool State { get; set; }

        public FileHashed() { }
        public FileHashed(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            FilePath = filePath;
            Size = fileInfo.Length / 1024; //  КБ
        }
    }
}
