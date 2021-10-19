using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadCarriersInLocalDB
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var loadCarriersInLocalDB = new Manager();

            if (loadCarriersInLocalDB.Load())
                Manager.ConsoleWriteLine($@"OK", ConsoleColor.Green);
            else
                Manager.ConsoleWriteLine($@"ERROR!!!", ConsoleColor.Red);

            Console.ReadKey();
        }
    }
}
