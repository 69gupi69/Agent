using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diascan.Agent.ModelDB;

namespace Diascan.Agent.ModelDB
{
    public class File
    {
        public string FilePath { get; set; }
        public Int64 Size { get; set; }    //  размер в КБ
        public byte[] Hashes { get; set; }
        public bool State { get; set; }        

        public File() { }
        public File(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            FilePath = filePath;
            Size = fileInfo.Length / 1024; //  КБ
        }
    }
}
