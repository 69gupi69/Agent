using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace BenchmarkHashes
{
    public class BenchmarkHashes
    {
        private readonly FileInfo[] fileInfos;
        private readonly MD5 md5;
        private readonly MD5Cng md5Cng;
        private readonly SHA256 sha256;
        private readonly SHA1 sha1;

        public BenchmarkHashes()
        {
            md5 = MD5.Create();
            md5Cng = new MD5Cng();
            sha256 = SHA256.Create();
            sha1 = SHA1.Create();

            var path =
                @"\\ctd.tn.corp\dfs\DepDocs\УРЭИС ОРИС\05 Documents\Гнетнев";
            fileInfos = new DirectoryInfo(path).GetFiles(".", SearchOption.AllDirectories);
        }

        [Benchmark(Description = "SHA1")]
        public List<byte[]> ComputeSha()
        {
            var data = new List<byte[]>();
            foreach (var fileInfo in fileInfos)
            {
                var fileStream = new BufferedStream(File.OpenRead(fileInfo.FullName), 1048576);
                data.Add(sha1.ComputeHash(fileStream));
            }
            return data;
        }

        [Benchmark(Description = "MD5")]
        public List<byte[]> ComputeMd5()
        {
            var data = new List<byte[]>();
            foreach (var fileInfo in fileInfos)
            {
                var fileStream = new BufferedStream(File.OpenRead(fileInfo.FullName), 1048576);
                data.Add(md5.ComputeHash(fileStream));
            }
            return data;
        }

        [Benchmark(Description = "CRC32")]
        public List<uint> ComputeCrc32()
        {
            var data = new List<uint>();
            foreach (var fileInfo in fileInfos)
            {
                var fileStream = new BufferedStream(File.OpenRead(fileInfo.FullName), 1048576);
                data.Add(DiCore.Lib.ServerTasks.Crc32.Task.CalculateCrc32File(fileStream));
            }
            return data;
        }
    }
}
