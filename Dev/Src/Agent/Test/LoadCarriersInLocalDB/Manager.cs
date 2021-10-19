using System;
using System.IO;
using System.Windows.Forms;
using DiCore.Lib.NDT.Carrier;
using File = System.IO.File;

namespace LoadCarriersInLocalDB
{
    public class Manager
    {
        private const string DbName = "Carriers.db";
        private string path;
        private Loader loader;
        private LocalLoader localLoader;

        public Manager()
        {
            path = Path.Combine(String.IsNullOrEmpty("") ? Application.StartupPath : "", DbName);

            Manager.ConsoleWriteLine($@"Путь к файлу : {path}", ConsoleColor.Green);

            if (File.Exists(path))
                File.Delete(path);

            loader      = new Loader();
            localLoader = new LocalLoader();
        }

        static public void ConsoleWriteLine(string text, ConsoleColor consoleColor)
        {
            Console.ResetColor();
            Console.ForegroundColor = consoleColor;
            Console.WriteLine($@"{text}");
        }

        public bool Load()
        {
            var res = false;
            var carriers = loader.GetAllCarriers();
            if (carriers != null)
            {
                foreach (var carrier in carriers)
                {
                    localLoader.AddCarrier(carrier);
                    ConsoleWriteLine(carrier.Id + " / "+carrier.Name, ConsoleColor.Green);
                }
                res = true;
            }

            return res;
        }
    }
}

