using System;
using Diascan.Agent.Logger;

namespace Diascan.TestMemoryAllocator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string yes = "yes";
            var exit = true;
            Logger.InitLogger(AppDomain.CurrentDomain.BaseDirectory + "Log");

            var testMemoryAllocator = new TestMemoryAllocator();
            testMemoryAllocator.CallBack += CallBack;
            testMemoryAllocator.CallBackColor += CallBackColor;

            testMemoryAllocator.Start();

            Console.WriteLine("Start");
            while (exit)
            {
                CallBack("Завершить работу теста MemoryAllocator?");
                CallBack("Введите: Yes(Y) No(N)?");
                var s = Console.ReadLine();
                if (yes.Contains(s.ToLower()))
                {
                    Console.Clear();
                    testMemoryAllocator.Stop();
                    exit = false;
                }
            }
            Console.ReadLine();
        }
        private static void CallBack(string message)
        {
            Console.ResetColor();
            Console.WriteLine(message);
        }
        private static void CallBackColor(string message, ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(message);
        }
    }
}
