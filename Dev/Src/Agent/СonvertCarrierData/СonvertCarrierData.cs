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
    public class СonvertCarrierData
    {
        private string Path { get; set; }

        public СonvertCarrierData(string path)
        {
            this.Path = path;
        }

        public static void ConsoleWriteLine(string text, ConsoleColor consoleColor)
        {
            Console.ResetColor();
            Console.ForegroundColor = consoleColor;
            Console.WriteLine($@"{text}");
        }

        private IEnumerable<CarrierData> ReadExcel(Worksheet worksheet)
        {
            var res = new List<CarrierData>(worksheet.Cells.Rows.Count);
            for (var i = 2; i < worksheet.Cells.Rows.Count; i++)
            {
                var Id = int.Parse(worksheet.Cells.Rows[i][1].StringValue);
                var Type = StringToTypes(worksheet.Cells.Rows[i][2].StringValue, worksheet.Cells.Rows[i][5].StringValue, worksheet.Cells.Rows[i][3].StringValue);
                var Defectoscope = worksheet.Cells.Rows[i][4].StringValue;
                var Sensorcount = int.Parse(worksheet.Cells.Rows[i][6].StringValue);
                var CarrierDiameter = int.Parse(worksheet.Cells.Rows[i][7].StringValue);
                var SpeedMin = double.Parse(worksheet.Cells.Rows[i][8].StringValue);
                var SpeedMax = double.Parse(worksheet.Cells.Rows[i][9].StringValue);
                var Change = !string.IsNullOrEmpty(worksheet.Cells.Rows[i][10].StringValue);
                var NumberSensorsBlock = int.Parse(worksheet.Cells.Rows[i][11].Value?.ToString() ?? 0.ToString());

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
                    throw new Exception($" Ошибка чтения данных из Excel \n CarrierData Id: {liteDbData.Id} \n Ошибка в строке: {i}");

                res.Add(liteDbData);
            }
            return res;
        }

        private void WriteExcel(Worksheet workSheet, IEnumerable<CarrierData> data, ref int totalRow)
        {
            workSheet.Name = "CarrierData";
            workSheet.Cells[0,  0].Value = "№ п/п";
            workSheet.Cells[0,  1].Value = "Идентификатор носителя";
            workSheet.Cells[0,  2].Value = "Тип носителя";
            workSheet.Cells[0,  3].Value = "Наименование";
            workSheet.Cells[0,  4].Value = "Дефектоскоп";
            workSheet.Cells[0,  5].Value = "Описание";
            workSheet.Cells[0,  6].Value = "Количество датчиков";
            workSheet.Cells[0,  7].Value = "Диаметр носителя, дюймы";
            workSheet.Cells[0,  8].Value = "Мин. значение доп. скорости ";
            workSheet.Cells[0,  9].Value = "Макс. значение доп. скорости";
            workSheet.Cells[0, 10].Value = "Изменение (CDL/CDS)";
            workSheet.Cells[0, 11].Value = "Кол-во датчиков в блоке";

            for (var j = 0; j < 12; j++)
                workSheet.Cells[1, j].Value = j+1;

            var i = 1;
            foreach (var item in data)
            {
                workSheet.Cells[totalRow,  0].Value = i;
                workSheet.Cells[totalRow,  1].Value = item.Id;
                workSheet.Cells[totalRow,  2].Value = TypeToString(item.Type);
                workSheet.Cells[totalRow,  3].Value = TypeToStringTypeName(item.Type, item.CarrierDiameter);
                workSheet.Cells[totalRow,  4].Value = item.Defectoscope;
                workSheet.Cells[totalRow,  5].Value = TypeToStringDescriptionType(item.Type);
                workSheet.Cells[totalRow,  6].Value = item.Sensorcount;
                workSheet.Cells[totalRow,  7].Value = item.CarrierDiameter;
                workSheet.Cells[totalRow,  8].Value = item.SpeedMin;
                workSheet.Cells[totalRow,  9].Value = item.SpeedMax;
                workSheet.Cells[totalRow, 10].Value = item.Change?'+':'-';
                workSheet.Cells[totalRow, 11].Value = item.NumberSensorsBlock;
                totalRow++;
                i++;
            }
        }

        public IEnumerable<CarrierData> ExcelTo()
        {
            if (!File.Exists(Path))
                throw new Exception($" Не найден указаный файл Excel: \n {Path}");

            return ReadExcel(new Workbook(Path).Worksheets[0]);
        }

        public void ToExcel( IEnumerable<CarrierData> data)
        {
            if(data == null)
                throw new Exception($" данные отсутствует : \n {Path}");

            if (!data.Any())
                throw new Exception($" в списке нет данных : \n {Path}");

            var license = new Aspose.Cells.License();
            license.SetLicense(new System.IO.MemoryStream(Resources.Aspose_Total));

            var workBook = new Workbook();
            var totalRow = 2;

            WriteExcel(workBook.Worksheets[0], data,ref totalRow);
            TableDesign(workBook, 0, totalRow, 12);
            TableDesign(workBook, 0, 2, 12, 0,0, DifferentFontBorderType(10));
            workBook.Save(Path, SaveFormat.Xlsx);
        }


        /// <summary>
        /// Перенос данных из Excel в LiteDB
        /// </summary>
        public bool ConvertToLiteDB()
        {
            if (!File.Exists(Path))
            {
                ConsoleWriteLine("Не найден указаный файл", ConsoleColor.Red);
                return false;
            }

            var worksheet = new Workbook(Path).Worksheets[0];
            var i = 1;

            var dbPath = System.IO.Path.GetDirectoryName(Path);
            File.Delete(dbPath + @"\CarrierData.db");
            var db = new LiteDatabase(dbPath + @"\CarrierData.db");
            var dbCollection = db.GetCollection<CarrierData>("CarrierData");

            try
            {
                var data = ReadExcel(worksheet);
                foreach (var item in data)
                    dbCollection.Insert(item);
                    
                return true;
            }
            catch (Exception ex)
            {
                var a = dbCollection.FindAll().Where(q => q.Id == 280700001).ToArray();
                ConsoleWriteLine($"    Ошибка при конвератации в стоке {i} :\n     {ex}", ConsoleColor.Red);
                return false;
            }
        }

        /// <summary>
        /// Перенос данных из LiteDB в Excel
        /// </summary>
        /// <returns></returns>
        public bool ConvertLiteDBToExcel()
        {
            if (!File.Exists(Path))
            {
                ConsoleWriteLine("Не найден указаный файл", ConsoleColor.Red);
                return false;
            }
            var license = new Aspose.Cells.License();
            license.SetLicense(new System.IO.MemoryStream(Resources.Aspose_Total));

            var workBook = new Workbook();
            var totalRow = 1;

            var data = GetCarrierData();

            try
            {
                WriteExcel(workBook.Worksheets[0], data, ref totalRow);
                TableDesign(workBook, 0, totalRow, 9);

                File.WriteAllBytes($@"{System.IO.Path.GetDirectoryName(Path) + @"\CarrierData.xls"}", workBook.SaveToStream().ToArray());
                return true;
            }
            catch (Exception ex)
            {
                ConsoleWriteLine($@"    Ошибка при конвератации в стоке {totalRow} :\n     {ex}", ConsoleColor.Red);
                return false;
            }
        }

        private void TableDesign(Workbook workBook, int workSheetNumber, int totalRow, int totalColumn, int firstRow = 0, int firstColumn = 0, Style style = null)
        {
            if (style == null)
                style = BaseStyle();
            for (var i = firstRow; i < firstRow + totalRow; i++)
            {
                for (var j = firstColumn; j < firstColumn + totalColumn; j++)
                {
                    var newStyle = workBook.Worksheets[workSheetNumber].Cells[i, j].GetStyle();
                    style.ForegroundColor = newStyle.ForegroundColor;
                    workBook.Worksheets[workSheetNumber].Cells[i, j].SetStyle(style);
                }
            }
        }
        public Style DifferentFontStyle(int size = 10)
        {
            var style = BaseStyle(size);
            style.Font.IsBold = true;
            style.IsTextWrapped = true;
            return style;
        }

        public Style DifferentFontBorderType(int size = 10)
        {
            var style = DifferentFontStyle(size);
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Double;
            style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Double;
            style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Double;
            style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Double;
            return style;
        }

        private Style BaseStyle(int size = 10)
        {
            var style = new Style();
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
            style.Font.Name = "Franklin Gothic Book";
            style.Font.Size = size;
            style.Pattern = BackgroundType.Solid;
            style.VerticalAlignment = TextAlignmentType.Center;
            style.HorizontalAlignment = TextAlignmentType.Center;
            style.IsTextWrapped = true;
            return style;
        }

        private List<CarrierData> GetCarrierData()
        {
            var carrierDataModelAccess = new CarrierDataModelAccess();
            carrierDataModelAccess.OpenConnection();
            return carrierDataModelAccess.CarrierAccess.GetAll().ToList();
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

        /// <summary>
        /// Преобразование enum "Тип носителя" в Строковое "Описание" 
        /// </summary>
        /// <param name="type">enum Тип носителя</param>
        /// <returns>Описание</returns>
        private string TypeToStringDescriptionType(DataTypesExt type)
        {
            switch (type)
            {
                case DataTypesExt.Cdl: return "Сdl";
                case DataTypesExt.Cdc: return "Сdc";

                case DataTypesExt.Wm:
                case DataTypesExt.Wm | DataTypesExt.Cdc: return "Сdc";
                case DataTypesExt.Wm | DataTypesExt.Cdl: return "Сdl";

                case DataTypesExt.MflT11: return "T11";
                case DataTypesExt.MflT31: return "T31";
                case DataTypesExt.MflT32: return "T32";
                case DataTypesExt.MflT1 : return "T1";
                case DataTypesExt.MflT2 : return "T2";
                case DataTypesExt.MflT3 : return "T3";

                case DataTypesExt.MflT22: return "T22";
                case DataTypesExt.MflT33: return "T33";
                case DataTypesExt.MflT34: return "T34";
                case DataTypesExt.TfiT41: return "T41";
                case DataTypesExt.TfiT4 : return  "T4";

                case DataTypesExt.Cd360 | DataTypesExt.Cdc: return "Cdc";
                case DataTypesExt.Cd360 | DataTypesExt.Cdl: return "Cdl";
                case DataTypesExt.Cd360 | DataTypesExt.Cds: return "Cds";
                case DataTypesExt.Cd360 | DataTypesExt.Cdh: return "Cdh";
                case DataTypesExt.Cd360 | DataTypesExt.Cdg: return "Cdg";
                case DataTypesExt.Cd360 | DataTypesExt.Cdf: return "Cdf";

                case DataTypesExt.CDPA | DataTypesExt.Cdc: return "Cdc";
                case DataTypesExt.CDPA | DataTypesExt.Cdl: return "Cdl";
                case DataTypesExt.CDPA | DataTypesExt.Cds: return "Cds";
                case DataTypesExt.CDPA | DataTypesExt.Cdh: return "Cdh";
                case DataTypesExt.CDPA | DataTypesExt.Cdg: return "Cdg";
                case DataTypesExt.CDPA | DataTypesExt.Cdf: return "Cdf";
                default: break;
            }
            return string.Empty;
        }

        /// <summary>
        /// Преобразование enum "Тип носителя" и Диаметр носителя, дюймы в "Наименование"
        /// </summary>
        /// <param name="type">enum Тип носителя</param>
        /// <param name="carrierDiameter">Диаметр носителя, дюймы</param>
        /// <returns>Наименование</returns>
        private string TypeToStringTypeName(DataTypesExt type, int carrierDiameter)
        {
            switch (type)
            {
                case DataTypesExt.Cdl:
                case DataTypesExt.Cdc:
                case DataTypesExt.Cdl | DataTypesExt.Cdc: return $@"{carrierDiameter}” CD";

                case DataTypesExt.Wm:
                case DataTypesExt.Wm | DataTypesExt.Cdc:
                case DataTypesExt.Wm | DataTypesExt.Cdl: return $@"{carrierDiameter}” WM";

                case DataTypesExt.Spm: return $@"{carrierDiameter}” SPM";

                case DataTypesExt.Mpm: return $@"{carrierDiameter}” MPM";

                case DataTypesExt.MflT11:
                case DataTypesExt.MflT31:
                case DataTypesExt.MflT32:
                case DataTypesExt.MflT1:
                case DataTypesExt.MflT2:
                case DataTypesExt.MflT3: return $@"{carrierDiameter}” MFL";

                case DataTypesExt.MflT22:
                case DataTypesExt.MflT33:
                case DataTypesExt.MflT34:
                case DataTypesExt.TfiT41:
                case DataTypesExt.TfiT4: return $@"{carrierDiameter}” TFI";

                case DataTypesExt.Cd360:
                case DataTypesExt.Cd360 | DataTypesExt.Cdc | DataTypesExt.Cds | DataTypesExt.Cdh | DataTypesExt.Cdl | DataTypesExt.Cdg | DataTypesExt.Cdf:
                case DataTypesExt.Cd360 | DataTypesExt.Cdc:
                case DataTypesExt.Cd360 | DataTypesExt.Cdl:
                case DataTypesExt.Cd360 | DataTypesExt.Cds:
                case DataTypesExt.Cd360 | DataTypesExt.Cdh:
                case DataTypesExt.Cd360 | DataTypesExt.Cdg:
                case DataTypesExt.Cd360 | DataTypesExt.Cdf: return $@"{carrierDiameter}” CD360";

                case DataTypesExt.CDPA :
                case DataTypesExt.CDPA | DataTypesExt.Cdc | DataTypesExt.Cds | DataTypesExt.Cdh | DataTypesExt.Cdl | DataTypesExt.Cdg | DataTypesExt.Cdf:
                case DataTypesExt.CDPA | DataTypesExt.Cdc:
                case DataTypesExt.CDPA | DataTypesExt.Cdl:
                case DataTypesExt.CDPA | DataTypesExt.Cds:
                case DataTypesExt.CDPA | DataTypesExt.Cdh:
                case DataTypesExt.CDPA | DataTypesExt.Cdg:
                case DataTypesExt.CDPA | DataTypesExt.Cdf: return $@"{carrierDiameter}” CDPA";
                case DataTypesExt.Ema: return $@"{carrierDiameter}” EMA";
                default: break;
            }
            return String.Empty;
        }

        /// <summary>
        /// Преобразование enum "Тип носителя" в строковый "Тип носителя" 
        /// </summary>
        /// <param name="type">enum Тип носителя</param>
        /// <returns>Строковый "Тип носителя"</returns>
        private string TypeToString(DataTypesExt type)
        {
            switch (type)
            {
                case DataTypesExt.Cdl:
                case DataTypesExt.Cdc:
                case DataTypesExt.Cdl | DataTypesExt.Cdc: return "CD";

                case DataTypesExt.Wm:
                case DataTypesExt.Wm | DataTypesExt.Cdc:
                case DataTypesExt.Wm | DataTypesExt.Cdl:  return "WM";

                case DataTypesExt.Spm :                   return "SPM";

                case DataTypesExt.Mpm: return "MPM";

                case DataTypesExt.MflT11:
                case DataTypesExt.MflT31:
                case DataTypesExt.MflT32:
                case DataTypesExt.MflT1 :
                case DataTypesExt.MflT2 :
                case DataTypesExt.MflT3 : return "MFL";

                case DataTypesExt.MflT22:
                case DataTypesExt.MflT33:
                case DataTypesExt.MflT34:
                case DataTypesExt.TfiT41:
                case DataTypesExt.TfiT4 : return "TFI";

                case DataTypesExt.Cd360:
                case DataTypesExt.Cd360 | DataTypesExt.Cdc | DataTypesExt.Cds | DataTypesExt.Cdh | DataTypesExt.Cdl | DataTypesExt.Cdg | DataTypesExt.Cdf:
                case DataTypesExt.Cd360 | DataTypesExt.Cdc: 
                case DataTypesExt.Cd360 | DataTypesExt.Cdl: 
                case DataTypesExt.Cd360 | DataTypesExt.Cds: 
                case DataTypesExt.Cd360 | DataTypesExt.Cdh: 
                case DataTypesExt.Cd360 | DataTypesExt.Cdg: 
                case DataTypesExt.Cd360 | DataTypesExt.Cdf: return "CD360";

                case DataTypesExt.CDPA:
                case DataTypesExt.CDPA | DataTypesExt.Cdc | DataTypesExt.Cds | DataTypesExt.Cdh | DataTypesExt.Cdl | DataTypesExt.Cdg | DataTypesExt.Cdf:
                case DataTypesExt.CDPA | DataTypesExt.Cdc:
                case DataTypesExt.CDPA | DataTypesExt.Cdl:
                case DataTypesExt.CDPA | DataTypesExt.Cds:
                case DataTypesExt.CDPA | DataTypesExt.Cdh:
                case DataTypesExt.CDPA | DataTypesExt.Cdg:
                case DataTypesExt.CDPA | DataTypesExt.Cdf: return "CDPA";
                case DataTypesExt.Ema : return "EMA";
                default: break;
            }
            return string.Empty;
        }

        /// <summary>
        /// Преобразование Строковые "Тип носителя", "Описание", "Наименование" в enum "Тип носителя"
        /// </summary>
        /// <param name="strType">Тип носителя</param>
        /// <param name="description">Описание</param>
        /// <param name="title">Наименование</param>
        /// <returns>enum "Тип носителя"</returns>
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
                case "spm":
                    type |= DataTypesExt.Spm;
                    break;
                case "mpm":
                    type |= DataTypesExt.Mpm;
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
                case "cds": type |= DataTypesExt.Cd360 | DataTypesExt.Cds; break;
                case "cdh": type |= DataTypesExt.Cd360 | DataTypesExt.Cdh; break;
                case "cdg": type |= DataTypesExt.Cd360 | DataTypesExt.Cdg; break;
                case "cdf": type |= DataTypesExt.Cd360 | DataTypesExt.Cdf; break;
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
