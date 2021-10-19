using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Diascan.Agent.ComputeHashes
{
    public enum enHasheType
    {
        Md5 = 0,
        Crc32 = 1,
        Sha256 = 2,
    }

    public class ComputeHashes
    {
        private FileInfo[] fileInfos;
        private readonly MD5 md5;
        private readonly SHA256 sha256;

        public ComputeHashes()
        {
            md5 = MD5.Create();
            sha256 = SHA256.Create();
        }

        private IEnumerable<byte[]> ComputeMd5()
        {
            foreach (var fileInfo in fileInfos)
            {
                yield return md5.ComputeHash(File.OpenRead(fileInfo.FullName));
            }
        }

        private IEnumerable<uint> ComputeCrc32()
        {
            foreach (var fileInfo in fileInfos)
            {
                yield return DiCore.Lib.ServerTasks.Crc32.Task.CalculateCrc32File(File.OpenRead(fileInfo.FullName));
            }
        }

        private IEnumerable<byte[]> ComputeSha256()
        {
            foreach (var fileInfo in fileInfos)
            {
                yield return sha256.ComputeHash(File.OpenRead(fileInfo.FullName));
            }
        }

        public void GetHashes(string path, enHasheType type)
        {
            fileInfos = new DirectoryInfo(path).GetFiles(".", SearchOption.AllDirectories);

            var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{type.ToString()}.txt");
            var fileStream = File.AppendText(outputPath);

            switch (type)
            {
                case enHasheType.Md5:
                    foreach (var data in ComputeMd5())
                        fileStream.WriteLine(String.Join("-", data.Select(item => item.ToString("x"))));
                    break;
                case enHasheType.Crc32:
                    foreach (var data in ComputeCrc32())
                        fileStream.WriteLine(data.ToString());
                    break;
                case enHasheType.Sha256:
                    foreach (var data in ComputeSha256())
                        fileStream.WriteLine(String.Join("-", data.Select(item => item.ToString("x"))));
                    break;
            }

            fileStream.Close();
        }
    }
}
