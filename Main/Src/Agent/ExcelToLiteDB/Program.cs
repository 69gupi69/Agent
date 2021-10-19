using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Cells;
using LiteDB;

namespace ExcelToLiteDB
{
    //  C:\Users\ZolotukhinKV\Desktop\111.xlsx
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.Write(@"Путь к файлу : ");
            var excelToLiteDb = new ExcelToLiteDB(Console.ReadLine());
            //var excelToLiteDb = new ExcelToLiteDB(@"D:\111.xlsx");
            excelToLiteDb.Convert();
            
            Console.ReadKey();
        }
    }

    public class ExcelToLiteDB
    {
        private string excelPath;

        public ExcelToLiteDB(string excelPath)
        {
            this.excelPath = excelPath;
        }

        public void Convert()
        {
            if (!File.Exists(excelPath))
                Console.WriteLine("Не найден указаный файл");

            var worksheet = new Workbook(excelPath).Worksheets[0];
            var i = 1;

            var dbPath = Path.GetDirectoryName(excelPath);
            File.Delete(dbPath + @"\SensorMediaIdentifiers.db");
            var db = new LiteDatabase(dbPath + @"\SensorMediaIdentifiers.db");
            var dbCollection = db.GetCollection<SensorMediaIdentifier>("SensorMediaIdentifier");
            var calcStatus = "OK!";
            Console.ForegroundColor = ConsoleColor.Green;
            try
            {
                foreach (Row row in worksheet.Cells.Rows)
                {
                    var liteDbData = new SensorMediaIdentifier
                    {
                        Id = int.Parse(row[0].Value.ToString()),
                        Type = StringToTypes(row[1].Value?.ToString(), row[4].Value?.ToString(), row[2].Value?.ToString()),
                        Defectoscope = row[3].Value?.ToString(),
                        NumberOfSensors = int.Parse(row[5].Value.ToString()),
                        CarrierDiameter = int.Parse(row[6].Value.ToString()),
                        SpeedMin = double.Parse(row[7].Value.ToString()),
                        SpeedMax = double.Parse(row[8].Value.ToString()),
                        Change = row[9].Value != null
                    };

                    if (liteDbData.Type == Types.None)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        calcStatus = $@"Id: {liteDbData.Id} \n ERROR!!! \n Строка в экселе: {i}";
                        break;
                    }
                    i++;
                    dbCollection.Insert(liteDbData);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка при конвератации в стоке {i} :{ex}");
            }
            Console.WriteLine(calcStatus);
        }

        private string ToEnglish(string value)
        {
            var returnValue = new char[value.Length];
            for (var i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case 'с':
                        returnValue[i] = 'c';
                       break;
                    case 'о':
                        returnValue[i] = 'o';
                        break;
                    case 'а':
                        returnValue[i] = 'a';
                        break;
                    case 'е':
                        returnValue[i] = 'e';
                        break;
                    case 'р':
                        returnValue[i] = 'p';
                        break;
                    case 'х':
                        returnValue[i] = 'x';
                        break;
                    default:
                        returnValue[i] = value[i];
                        break;
                }
            }
            return new string(returnValue);
        }
        
        //  пока не используется
        private Types StringToTypes(string strType, string description, string title)
        {
            //////////////////////////////////////////////
            strType = ToEnglish(strType.ToLower());
            description = description == null ? null : ToEnglish(description.ToLower());
            title = title == null ? null : ToEnglish(title.ToLower());
            var type = Types.None;
            //////////////////////////////////////////////


            if (strType.Contains("wm") && strType.Contains("cd"))
            {
                if(title.Contains("wm")) type |= Types.Wm;
                else if (title.Contains("cd")) type |= Types.Cdl | Types.Cdc;
            }
            else if (strType.Contains("wm"))
            {
                if(description == null) type |= Types.Wm;
                else
                {
                    type |= Types.Wm;
                    if (description.Contains("cdc")) type |= Types.Cdc;
                    if (description.Contains("cdl")) type |= Types.Cdl;
                }
            }
            else if (strType.Contains("cd"))
            {
                if (description == null) type |= Types.Cdl | Types.Cdc;
                else
                {
                    if (description.Contains("cdc")) type |= Types.Cdc;
                    if (description.Contains("cdl")) type |= Types.Cdl;
                }
            }

            if (strType.Contains("spm")) type |= Types.Spm;
            if (strType.Contains("mpm")) type |= Types.Mpm;
            if (strType.Contains("mfl"))
            {
                if (description.Contains("t11")) type |= Types.MflT11;
                //else if (description.Contains("t22")) type |= Types.MflT22;
                else if(description.Contains("t31")) type |= Types.MflT31;
                else if (description.Contains("t32")) type |= Types.MflT32;
                //else if (description.Contains("t33")) type |= Types.MflT33;
                //else if (description.Contains("t34")) type |= Types.MflT34;
                else if (description.Contains("t1")) type |= Types.MflT1;
                else if (description.Contains("t2")) type |= Types.MflT2;
                else if (description.Contains("t3")) type |= Types.MflT3;
            }
            if (strType.Contains("tfi"))
            {
                if (description.Contains("t22")) type |= Types.MflT22;
                else if (description.Contains("t33")) type |= Types.MflT33;
                else if (description.Contains("t34")) type |= Types.MflT34;
                else if (description.Contains("t41")) type |= Types.TfiT41;
                else if (description.Contains("t4")) type |= Types.TfiT4;
            }
            if (strType.Contains("cd360"))
            {
                if (description == null)
                    type |= Types.Cd360 | Types.Cdc | Types.Cds | Types.Cdh | Types.Cdl | Types.Cdg | Types.Cdf;
                else
                {
                    type |= Types.Cd360;
                    if (description.Contains("cdc")) type |= Types.Cdc;
                    if (description.Contains("cdl")) type |= Types.Cdl;
                    if (description.Contains("cds")) type |= Types.Cds;
                    if (description.Contains("cdh")) type |= Types.Cdh;
                    if (description.Contains("cdg")) type |= Types.Cdg;
                    if (description.Contains("cdf")) type |= Types.Cdf;
                }
            }
            else if(strType.Contains("cds")) type |= Types.Cd360 | Types.Cds;
            else if(strType.Contains("cdh")) type |= Types.Cd360 | Types.Cdh;
            else if(strType.Contains("cdg")) type |= Types.Cd360 | Types.Cdg;
            else if(strType.Contains("cdf")) type |= Types.Cd360 | Types.Cdf;
            
            if (strType.Contains("ema")) type |= Types.Ema;
            return type;
        }
    }

    public class SensorMediaIdentifier
    {
        public int Id { get; set; }
        public Types Type { get; set; }
        public string Defectoscope { get; set; }
        public int NumberOfSensors { get; set; }
        public int CarrierDiameter { get; set; }
        public double SpeedMin { get; set; }
        public double SpeedMax { get; set; }
        public bool Change { get; set; }
    }

    [Flags]
    public enum Types
    {
        None = 0,
        Spm = 1 << 1,
        Mpm = 1 << 2,
        Wm = 1 << 3,
        MflT1 = 1 << 4,
        MflT11 = 1 << 5,
        MflT2 = 1 << 6,
        MflT22 = 1 << 7,
        MflT3 = 1 << 8,
        MflT31 = 1 << 9,
        MflT32 = 1 << 10,
        MflT33 = 1 << 11,
        MflT34 = 1 << 12,
        TfiT4 = 1 << 13,
        TfiT41 = 1 << 14,
        Cd360 = 1 << 15,
        Cdc = 1 << 16,
        Cds = 1 << 17,
        Cdh = 1 << 18,
        Cdl = 1 << 19,
        Cdg = 1 << 20,
        Cdf = 1 << 21,
        Ema = 1 << 22
    }
}
