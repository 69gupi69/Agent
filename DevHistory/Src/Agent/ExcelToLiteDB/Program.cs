using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Cells;
using Diascan.Agent.Types;
using LiteDB;
using File = System.IO.File;

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

        public bool Convert()
        {
            if (!File.Exists(excelPath))
                Console.WriteLine("Не найден указаный файл");

            var worksheet = new Workbook(excelPath).Worksheets[0];
            var i = 1;

            var dbPath = Path.GetDirectoryName(excelPath);
            File.Delete(dbPath + @"\CarrierData.db");
            var db = new LiteDatabase(dbPath + @"\CarrierData.db");
            var dbCollection = db.GetCollection<CarrierData>("CarrierData");
            var calcStatus = "OK!";
            Console.ForegroundColor = ConsoleColor.Green;
            try
            {
                foreach (Row row in worksheet.Cells.Rows)
                {
                    var Id = int.Parse(row[0].StringValue);
                    var Type = StringToTypes(row[1].StringValue, row[4].StringValue,row[2].StringValue);
                    var Defectoscope = row[3].StringValue;
                    var Sensorcount = int.Parse(row[5].StringValue);
                    var CarrierDiameter = int.Parse(row[6].StringValue);
                    var SpeedMin = double.Parse(row[7].StringValue);
                    var SpeedMax = double.Parse(row[8].StringValue);
                    var Change = !string.IsNullOrEmpty(row[9].StringValue);
                    var NumberSensorsBlock = int.Parse(row[10].Value?.ToString() ?? 0.ToString());

                    var liteDbData = new CarrierData
                    {
                        Id = Id,
                        Type = Type,
                        Defectoscope = Defectoscope,
                        Sensorcount = Sensorcount,
                        CarrierDiameter = CarrierDiameter,
                        SpeedMin = SpeedMin,
                        SpeedMax = SpeedMax,
                        Change = Change,
                        NumberSensorsBlock = NumberSensorsBlock
                    };

                    if (liteDbData.Type == DataTypesExt.None)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        calcStatus = $@"Id: {liteDbData.Id} \n ERROR!!! \n Строка в экселе: {i}";
                        break;
                    }
                    i++;
                    dbCollection.Insert(liteDbData);
                }
                return true;
            }
            catch (Exception ex)
            {

                var a = dbCollection.FindAll().Where(q => q.Id == 280700001).ToArray();
                Console.ForegroundColor = ConsoleColor.Red;
                calcStatus = "ERROR!!!";
                Console.WriteLine($"    Ошибка при конвератации в стоке {i} :\n     {ex}");
                return false;
            }
            finally
            {
                Console.WriteLine(calcStatus);
            }
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
        private DataTypesExt StringToTypes(string strType, string description, string title)
        {
            //////////////////////////////////////////////
            strType = ToEnglish(strType.ToLower());
            description = String.IsNullOrEmpty(description) ? string.Empty : ToEnglish(description.ToLower());
            title = title == null ? null : ToEnglish(title.ToLower());
            var type = DataTypesExt.None;
            //////////////////////////////////////////////
            switch (strType)
            {
                case "cd/wm":
                                if (title.Contains("wm")) type |= DataTypesExt.Wm;
                                else if (title.Contains("cd")) type |= DataTypesExt.Cdl | DataTypesExt.Cdc;
                                break;
                case "wm":
                                if (String.IsNullOrEmpty(description)) type |= DataTypesExt.Wm;
                                else
                                {
                                    type |= DataTypesExt.Wm;
                                    if (description.Contains("cdc")) type |= DataTypesExt.Cdc;
                                    if (description.Contains("cdl")) type |= DataTypesExt.Cdl;
                                }
                                break;
                case "cd":
                                if (String.IsNullOrEmpty(description)) type |= DataTypesExt.Cdl | DataTypesExt.Cdc;
                                else
                                {
                                    if (description.Contains("cdc")) type |= DataTypesExt.Cdc;
                                    if (description.Contains("cdl")) type |= DataTypesExt.Cdl;
                                }
                                break;
                case "spm":     type |= DataTypesExt.Spm;
                                break;
                case "mpm":     type |= DataTypesExt.Mpm;
                                break;
                case "mfl":
                                if (description.Contains("t11")) type |= DataTypesExt.MflT11;
                                //else if (description.Contains("t22")) type |= Types.MflT22;
                                else if (description.Contains("t31")) type |= DataTypesExt.MflT31;
                                else if (description.Contains("t32")) type |= DataTypesExt.MflT32;
                                //else if (description.Contains("t33")) type |= Types.MflT33;
                                //else if (description.Contains("t34")) type |= Types.MflT34;
                                else if (description.Contains("t1")) type |= DataTypesExt.MflT1;
                                else if (description.Contains("t2")) type |= DataTypesExt.MflT2;
                                else if (description.Contains("t3")) type |= DataTypesExt.MflT3;
                                break;
                case "tfi":
                                if (description.Contains("t22")) type |= DataTypesExt.MflT22;
                                else if (description.Contains("t33")) type |= DataTypesExt.MflT33;
                                else if (description.Contains("t34")) type |= DataTypesExt.MflT34;
                                else if (description.Contains("t41")) type |= DataTypesExt.TfiT41;
                                else if (description.Contains("t4")) type |= DataTypesExt.TfiT4;
                                break;
                case "cd360":
                                if (String.IsNullOrEmpty(description))
                                    type |= DataTypesExt.Cd360 | DataTypesExt.Cdc | DataTypesExt.Cds | DataTypesExt.Cdh | DataTypesExt.Cdl | DataTypesExt.Cdg | DataTypesExt.Cdf;
                                else
                                {
                                    type |= DataTypesExt.Cd360;
                                    if (description.Contains("cdc")) type |= DataTypesExt.Cdc;
                                    if (description.Contains("cdl")) type |= DataTypesExt.Cdl;
                                    if (description.Contains("cds")) type |= DataTypesExt.Cds;
                                    if (description.Contains("cdh")) type |= DataTypesExt.Cdh;
                                    if (description.Contains("cdg")) type |= DataTypesExt.Cdg;
                                    if (description.Contains("cdf")) type |= DataTypesExt.Cdf;
                                }
                                break;
                case "cds":     type |= DataTypesExt.Cd360 | DataTypesExt.Cds; break;
                case "cdh":     type |= DataTypesExt.Cd360 | DataTypesExt.Cdh; break;
                case "cdg":     type |= DataTypesExt.Cd360 | DataTypesExt.Cdg; break;
                case "cdf":     type |= DataTypesExt.Cd360 | DataTypesExt.Cdf; break;
                case "cdpa":
                                if (String.IsNullOrEmpty(description))
                                    type |= DataTypesExt.CDPA | DataTypesExt.Cdc | DataTypesExt.Cds | DataTypesExt.Cdh | DataTypesExt.Cdl | DataTypesExt.Cdg | DataTypesExt.Cdf;
                                else
                                {
                                    type |= DataTypesExt.CDPA;
                                    if (description.Contains("cdc")) type |= DataTypesExt.Cdc;
                                    if (description.Contains("cdl")) type |= DataTypesExt.Cdl;
                                    if (description.Contains("cds")) type |= DataTypesExt.Cds;
                                    if (description.Contains("cdh")) type |= DataTypesExt.Cdh;
                                    if (description.Contains("cdg")) type |= DataTypesExt.Cdg;
                                    if (description.Contains("cdf")) type |= DataTypesExt.Cdf;
                                }
                                break;
                case "ema":
                                type |= DataTypesExt.Ema; break;
                default: break;
            }
            return type;
        }
    }
}
