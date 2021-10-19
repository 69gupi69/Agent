using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace BenchmarkHashes
{
    public static class BenchmarkHashesRunner
    {
        public static void Run()
        {
            BenchmarkRunner.Run<BenchmarkHashes>();
            Console.ReadLine();
        }
    }
}
