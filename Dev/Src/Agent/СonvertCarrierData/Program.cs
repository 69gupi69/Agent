using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Cells;
using Diascan.Agent.LiteDbAccess.DataBaseAccess;
using Diascan.Agent.Types;
using СonvertCarrierData.Properties;
using LiteDB;
using File = System.IO.File;

namespace СonvertCarrierData
{
    //  C:\Users\ZolotukhinKV\Desktop\111.xlsx
    public class Program
    {
        private static void Main(string[] args)
        {
            var path = $@"{AppDomain.CurrentDomain.BaseDirectory + @"CarrierData.db"}";
            var excelToLiteDb = new СonvertCarrierData(path);

            СonvertCarrierData.ConsoleWriteLine($@"Путь к файлу : {path}", ConsoleColor.Green);

            if (excelToLiteDb.ConvertLiteDBToExcel())
                СonvertCarrierData.ConsoleWriteLine($@"OK", ConsoleColor.Green);
            else
                СonvertCarrierData.ConsoleWriteLine($@"ERROR!!!", ConsoleColor.Red);

            Console.ReadKey();
        }
    }
}
