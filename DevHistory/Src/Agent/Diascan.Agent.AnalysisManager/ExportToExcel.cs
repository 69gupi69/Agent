using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Aspose.Cells;
using Diascan.Agent.AnalysisManager.Properties;
using Diascan.Agent.CalcDiagDataLossTask;
using Diascan.Agent.TaskManager;
using Diascan.Agent.Types;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.DataProviders.NAV;

namespace Diascan.Agent.AnalysisManager
{
    public class ExportToExcel
    {
        private double triggerChamber;
        private double receptionChamber;
        private Workbook workBook;
        private int worksheetNumber;
        private const int MaxRowSheet = 65000;  //  максимальное число строк в одном листе
        private Action<object> loggerInfoAction;

        public ExportToExcel(Action<object> loggerInfoAction)
        {
            var license = new Aspose.Cells.License();
            this.loggerInfoAction = loggerInfoAction;
            license.SetLicense(new System.IO.MemoryStream(Resources.Aspose_Total));
        }

        public byte[] CreateExcelFile(AnalysisCalculation analysisCalculation, Calculation calculation, out bool result)
        {
            result = false;
            workBook = new Workbook();
            worksheetNumber = 0;
            receptionChamber = calculation.DataOutput.ReceptionChamber;
            triggerChamber = calculation.DataOutput.TriggerChamber;
            //  таблицы с ПДИ
            foreach (var analysisType in analysisCalculation.AnalisysTypeCollection)
            {
                workBook.Worksheets.Add(analysisType.CdmType == enCdmDirectionName.None
                    ? $"ПДИ по датчикам {analysisType.DataType.ToString().ToUpper()}"
                    : $"ПДИ по датчикам {analysisType.CdmType.ToString().ToUpper()}");
                worksheetNumber++;
                CreateTableHeaderCalculationPdi(calculation.DataOutput, analysisType.DataType);
                //  отрисовка заголовка таблицы и самой таблицы датчиков
                loggerInfoAction?.Invoke($"отрисовка заголовка таблицы и самой таблицы датчиков {analysisType.DataType.ToString().ToUpper()}");
                CreateTableBySensors(analysisType);
            }
            //  первый лист
            //  "шапка" таблицы
            loggerInfoAction?.Invoke($"первый лист \n \"шапка\" таблицы");
            CreateHeaderSummaryTable(0);
            //  отрисовка и заполние полей таблицы
            loggerInfoAction?.Invoke($"отрисовка и заполние полей таблицы");
            var rowIndex = CreateRowSummaryTable(analysisCalculation, 0);
            CreateCommonLinesSummaryTable(calculation, rowIndex);
            //  таблица перезапуска ВИП
            loggerInfoAction?.Invoke($"таблица перезапуска ВИП");
            RestartVip(analysisCalculation.RestartReport, 0, rowIndex);
            //  ПДИ по скорости
            loggerInfoAction?.Invoke($"ПДИ по скорости");
            workBook.Worksheets.Add();
            worksheetNumber++;
            workBook.Worksheets[worksheetNumber].Name = "ПДИ по скорости";
            CreateTableBySpeed(analysisCalculation, calculation);

            //  ПДИ рамки
            loggerInfoAction?.Invoke($"ПДИ рамки");
            if (calculation.CdChange)
            {
                workBook.Worksheets.Add();
                worksheetNumber++;
                workBook.Worksheets[worksheetNumber].Name = "Определение типа CD";
                CreateHeaderTableByFrames(calculation.DataOutput);
                CreateTableByFrames(calculation);
            }

            //  Навигация
            loggerInfoAction?.Invoke($"Навигация");
            if (calculation.NavigationInfo.State.HasFlag(NavigationStateTypes.CalcNavigation))
            {
                workBook.Worksheets.Add();
                worksheetNumber++;
                workBook.Worksheets[worksheetNumber].Name = "Контроль качества НД";
                CreateHeaderNavTable();
                CreateBodyNavTable(calculation);
            }

            result = true;
            loggerInfoAction?.Invoke($"Файл Excel {result}");
            return workBook.SaveToStream().ToArray();
        }

        private void CreateHeaderSummaryTable(int worksheetNumber)
        {
            var workSheet = workBook.Worksheets[worksheetNumber];
            workSheet.Name = "Сводная";
            var firstRowIndex = 0;

            //  "Шапка" таблицы
            workSheet.Cells[firstRowIndex, 0].Value = @"№ п\п";
            workSheet.Cells[firstRowIndex, 1].Value = "Организация";
            workSheet.Cells[firstRowIndex, 2].Value = "Трубопровод";
            workSheet.Cells[firstRowIndex, 3].Value = "Участок";
            workSheet.Cells[firstRowIndex, 4].Value = "Тип \nдиагностических \nданных";
            workSheet.Cells[firstRowIndex, 5].Value = "Площадь \nобследования, м\u00B2";
            workSheet.Cells[firstRowIndex, 6].Value = "Суммарная \nплощадь ПДИ, м\u00B2";
            workSheet.Cells[firstRowIndex, 7].Value = "Площадь ПДИ по \nпричине отказа ВИП, м\u00B2";
            workSheet.Cells[firstRowIndex, 8].Value = "Площадь ПДИ по причинам" + "\n" + @"ВГС\загрязнение\повреждение," + "м\u00B2";
            workSheet.Cells[firstRowIndex, 9].Value = "Площадь ПДИ по \nпричине несоответствия \nскорости, м\u00B2";

            for (var i = 0; i < 10; i++)
            {
                workSheet.Cells[firstRowIndex + 1, i].Value = i + 1;
            }

            TableDesign(worksheetNumber, 2, 10, firstRowIndex, 0, Styles.DifferentFontBorderType(11));
        }

        private void CreateCommonLinesSummaryTable(Calculation calculation, int rowIndex)
        {
            if (rowIndex == 0) return;

            var workSheet = workBook.Worksheets[0];
            var firstRowIndex = 2;
            //  Организация
            workSheet.Cells.Merge(firstRowIndex, 1, rowIndex, 1);
            workSheet.Cells[firstRowIndex, 1].Value = calculation.DataOutput.Contractor.Value;
            //  Трубопровод
            workSheet.Cells.Merge(firstRowIndex, 2, rowIndex, 1);
            workSheet.Cells[firstRowIndex, 2].Value = calculation.DataOutput.PipeLine.Value;
            //  Участок
            workSheet.Cells.Merge(firstRowIndex, 3, rowIndex, 1);
            workSheet.Cells[firstRowIndex, 3].Value = calculation.DataOutput.Route.Value;
            //  Площадь обследования, м2
            workSheet.Cells.Merge(firstRowIndex, 5, rowIndex, 1);
            workSheet.Cells[firstRowIndex, 5].Value = Math.Round(Math.PI * calculation.DataOutput.Diameter / 1000 * (calculation.DataOutput.DistRange.End - calculation.DataOutput.DistRange.Begin), 3); //calculation.DataOutput.OverallArea;
            //  Площадь ПДИ по причине несоответствия скорости, м2
            workSheet.Cells.Merge(firstRowIndex, 9, rowIndex, 1);
            workSheet.Cells[firstRowIndex, 9].Value = calculation.DataOutput.OverallSpeedAreaLdi;

            TableDesign(0, rowIndex, 10, firstRowIndex);
            workSheet.AutoFitColumns();
        }

        private int CreateRowSummaryTable(AnalysisCalculation analysisCalculation, int worksheetNumber)
        {
            var workSheet = workBook.Worksheets[worksheetNumber];
            var rowIndex = 0;
            var firstRowIndex = 2;
            foreach (var analysisType in analysisCalculation.AnalisysTypeCollection)
            {
                //  № п\п
                workSheet.Cells[rowIndex + firstRowIndex, 0].Value = rowIndex + 1;
                //Вид диагностических данных
                workSheet.Cells[rowIndex + firstRowIndex, 4].Value = analysisType.DataType.ToString().ToUpper();
                //  Суммарная площадь ПДИ, м2
                workSheet.Cells[rowIndex + firstRowIndex, 6].Value = Math.Round(analysisType.AreaLdi, 3);
                //  Площадь ПДИ по причине отказа ВИП, м2
                workSheet.Cells[rowIndex + firstRowIndex, 7].Value = Math.Round(analysisType.AreaFail, 3);
                //  Площадь ПДИ по причинам ВГС\загрязнение\повреждение, м2
                workSheet.Cells[rowIndex + firstRowIndex, 8].Value = Math.Round(analysisType.AreaLdi - analysisType.AreaFail, 3);
                rowIndex++;
            }

            workSheet.AutoFitColumns();
            return rowIndex;
        }

        private void CreateTableHeaderCalculationPdi(ReferenceInputData dataOutput, DataType dataType = DataType.None)
        {
            var workSheet = workBook.Worksheets[worksheetNumber];
            workSheet.Cells[0, 0].Value = "№ п/п";
            workSheet.Cells[0, 1].Value = "Результаты автоматического расчета потерь диагностической информации по участку \n" + dataOutput.Route.Value;
            workSheet.Cells.SetRowHeight(0, 40);
            workSheet.Cells.Merge(0, 1, 1, 4);
            for (var i = 0; i < 5; i++)
            {
                workSheet.Cells[1, i].Value = i + 1;
            }
            for (var i = 0; i < 8; i++)
            {
                workSheet.Cells[i + 2, 0].Value = i + 1;
            }

            var row = 2;
            var column = 1;

            workSheet.Cells[row, column].Value = "Название прогона: ";
            workSheet.Cells[row, column + 1].Value = dataOutput.WorkItemName;
            workSheet.Cells[row + 1, column].Value = "Диаметр, мм: ";
            workSheet.Cells[row + 1, column + 1].Value = dataOutput.Diameter;
            workSheet.Cells[row + 2, column].Value = "Организация: ";
            workSheet.Cells[row + 2, column + 1].Value = dataOutput.Contractor.Value;
            workSheet.Cells[row + 3, column].Value = "Трубопровод: ";
            workSheet.Cells[row + 3, column + 1].Value = dataOutput.PipeLine.Value;
            workSheet.Cells[row + 4, column].Value = "Участок: ";
            workSheet.Cells[row + 4, column + 1].Value = dataOutput.Route.Value;
            workSheet.Cells[row + 5, column].Value = "Дефектоскоп №: ";
            workSheet.Cells[row + 5, column + 1].Value = dataOutput.FlawDetector;
            workSheet.Cells[row + 6, column].Value = "Ответственный за пропуск: ";
            workSheet.Cells[row + 6, column + 1].Value = dataOutput.ResponsibleWorkItem;

            var distLenght = dataOutput.DistRange.End - dataOutput.DistRange.Begin;

            workSheet.Cells[row, column + 2].Value = "Дата пропуска: ";
            workSheet.Cells[row, column + 3].Value = dataOutput.DateWorkItem;
            workSheet.Cells[row + 1, column + 2].Value = "Протяженность по ВИП, м: ";
            workSheet.Cells[row + 1, column + 3].Value = Math.Round(distLenght, 3);
            workSheet.Cells[row + 3, column + 2].Value = "Площадь обследования, м\u00B2: ";
            workSheet.Cells[row + 3, column + 3].Value = Math.Round(Math.PI * dataOutput.Diameter / 1000 * distLenght, 3);
            workSheet.Cells[row + 4, column + 2].Value = "Площадь ПДИ на КПП СОД, м\u00B2: ";
            workSheet.Cells[row + 4, column + 3].Value = "";
            workSheet.Cells[row + 5, column + 2].Value = "ПДИ на линейной части, м\u00B2: ";
            workSheet.Cells[row + 5, column + 3].Value = "";
            workSheet.Cells[row + 6, column + 2].Value = "Суммарная площадь ПДИ, м\u00B2: ";
            workSheet.Cells[row + 6, column + 3].Value = "";

            if (IsDataTypeBlocksorСontiguous(dataType.ToString()))
                workSheet.Cells[row + 7, column + 2].Value = "Суммарная площадь ПДИ по блокам смежныx датчиков, м\u00B2: ";
            else
                workSheet.Cells[row + 7, column + 2].Value = "Суммарная площадь ПДИ по смежным датчикам, м\u00B2: ";

            workSheet.Cells[row + 7, column + 3].Value = "";

            workSheet.Cells.SetColumnWidth(0, 6);
            workSheet.AutoFitColumns(1, 3);
            workSheet.Cells.SetColumnWidth(4, 20);


            TableDesign(worksheetNumber, 2, 5, 0, 0, Styles.DifferentFontBorderType(12));
            TableDesign(worksheetNumber, 8, 1, 2, 0, Styles.BaseDoubleCellBorderType(12));

            TableDesign(worksheetNumber, 8, 1, row, column, Styles.BaseDoubleCellBorderType(12));
            TableDesign(worksheetNumber, 8, 1, row, column + 1, Styles.DifferentFontBorderType(12));
            TableDesign(worksheetNumber, 8, 1, row, column + 2, Styles.BaseDoubleCellBorderType(12));
            TableDesign(worksheetNumber, 8, 1, row, column + 3, Styles.DifferentFontBorderType(12));
        }

        private void CreateTableBySpeed(AnalysisCalculation analysisCalculation, Calculation calculation)
        {
            var workSheet = workBook.Worksheets[worksheetNumber];

            var speedInfos = new List<SpeedInfos>();
            var areaKppSpeed = 0d;
            var countSheet = 0; // количество листов
            var index = 9; // отступ
            var rowCount = 0; // обще число данных с speedInfo
            var rowDataIndex = 0; // количество заполненных строк

            foreach (var diagData in calculation.DiagDataList)
            {
                if (diagData.DataType == DataType.Nav || diagData.DataType == DataType.NavSetup) continue;
                speedInfos = CreateSpeedInfos(calculation.DataOutput, diagData);
                if (speedInfos.Count == 0)
                    areaKppSpeed = 0;
                else
                    areaKppSpeed += calculation.DataOutput.OverallSpeedAreaLdi * speedInfos.Where(q => q.AreaType).Sum(q => q.End - q.Begin) / speedInfos.Sum(q => q.End - q.Begin);
                rowCount += speedInfos.Count;
            }

            countSheet = (int)Math.Ceiling((double)rowCount / MaxRowSheet);

            //  Шапка таблицы
            CreateTableHeaderCalculationPdi(calculation.DataOutput);
            workSheet.Cells[6, 4].Value = double.IsNaN(areaKppSpeed) ? 0 : Math.Round(areaKppSpeed, 3);
            workSheet.Cells[7, 4].Value = double.IsNaN(areaKppSpeed) ? 0 : Math.Round(calculation.DataOutput.OverallSpeedAreaLdi - areaKppSpeed, 3);
            workSheet.Cells[8, 4].Value = calculation.DataOutput.OverallSpeedAreaLdi;

            workSheet.Cells[2, 5].Value = "Диапазоны допустимых скоростей: ";

            var row = 2;
            foreach (var item in analysisCalculation.DiagDataMain)
            {
                workSheet.Cells[row, 5].Value = $@"для ДД {item.Value.NameTypeData}, м/с: ";
                if (item.Value.PassportSpeedDiapason.Begin == 0)
                    workSheet.Cells[row, 6].Value = @"";
                else
                    workSheet.Cells[row, 6].Value = $@" 0,2 - {item.Value.PassportSpeedDiapason.Begin} ";

                workSheet.Cells[row + 3, 5].Value = $@"Площадь ПДИ по ДД {item.Value.NameTypeData}, м²: ";
                workSheet.Cells[row + 3, 6].Value = $@" {item.Value.Area} ";

                row++;
            }

            workSheet.Cells[1, 5].Value = "6";
            workSheet.Cells[1, 6].Value = "7";
            workSheet.Cells.SetRowHeight(0, 40);
            workSheet.Cells.Merge(0, 1, 1, 6);
            TableDesign(worksheetNumber, 2, 6, 0, 1, Styles.DifferentFontBorderType(11));
            TableDesign(worksheetNumber, 7, 1, 2, 5, Styles.BaseDoubleCellBorderType(11));
            TableDesign(worksheetNumber, 7, 1, 2, 6, Styles.DifferentFontBorderType(11));

            workSheet.Cells.SetColumnWidth(5, 14);
            workSheet.Cells.SetColumnWidth(6, 20);

            if (speedInfos.Count == 0)
            {
                workSheet.Cells[9, 0].Value = "Потери не обнаружены";
                workSheet.Cells.Merge(9, 0, 1, 4);
                TableDesign(worksheetNumber, 1, 4, 9, 0, Styles.DifferentFontStyle());
                return;
            }

            for (var i = 0; i < countSheet; i++)
            {
                //  шапка столбцов
                workSheet.Cells[index - 1, 0].Value = "№ п/п";
                workSheet.Cells[index - 1, 1].Value = "Дистанция начала ПДИ, м";
                workSheet.Cells[index - 1, 2].Value = "Дистанция конца ПДИ, м";
                workSheet.Cells[index - 1, 3].Value = "Фактическая скорость ВИП, м/с";
                workSheet.Cells[index - 1, 4].Value = "Площадь ПДИ, м\u00B2";
                workSheet.Cells[index - 1, 5].Value = "Тип участка";
                workSheet.Cells[index - 1, 6].Value = "Тип данных";

                workSheet.Cells[index - 2, 0].Value = "Потери данных по причине не соответствия скорости ВИП паспортному диапазону";
                workSheet.Cells.Merge(index - 2, 0, 1, 7);

                TableDesign(worksheetNumber, 1, 7, index - 2, 0, Styles.DifferentFontStyle(16));
                TableDesign(worksheetNumber, 1, 7, index - 1, 0, Styles.DifferentFontStyle(11));
                var j = 0; // количество заполненных строк на листе
                foreach (var item in analysisCalculation.DiagDataMain)
                {
                    foreach (var speedInfo in item.Value.SpeedInfos)
                    {
                        var begin = Math.Round(speedInfo.Begin, 3);
                        var end = Math.Round(speedInfo.End, 3);
                        var area = (end - begin) * Math.PI * calculation.DataOutput.Diameter / 1000;
                        workSheet.Cells[index + j, 0].Value = j + 1;
                        workSheet.Cells[index + j, 1].Value = begin;
                        workSheet.Cells[index + j, 2].Value = end;
                        workSheet.Cells[index + j, 3].Value = Math.Round(speedInfo.Speed, 3);
                        workSheet.Cells[index + j, 4].Value = Math.Round(area, 3);
                        workSheet.Cells[index + j, 5].Value = speedInfo.AreaType ? "КПП СОД" : "Линейная часть";
                        workSheet.Cells[index + j, 6].Value = speedInfo.Name;//item.Value.NameTypeData;
                        if (rowDataIndex + 1 == rowCount)
                        {
                            var lastRow = rowDataIndex - MaxRowSheet * i + 1;
                            TableDesign(worksheetNumber, lastRow, 7, index);
                            return;
                        }

                        if (j > MaxRowSheet)
                            break; // при привышени колисчтва строк выходим из цылка

                        rowDataIndex++;
                        j++;

                    }
                    if (j > MaxRowSheet)
                        break; // при привышени колисчтва строк выходим из цылка
                }

                TableDesign(worksheetNumber, MaxRowSheet, 7, index);

                workBook.Worksheets.Add($"ПДИ по скорости_{i + 1}");
                worksheetNumber++;
                workSheet = workBook.Worksheets[worksheetNumber];
                index = 2;
            }
        }

        private List<SpeedInfos> CreateSpeedInfos(ReferenceInputData dataOutput, DiagData calculationDiagData)
        {
            var name = GetAreaSpeedInfoName(calculationDiagData);
            var result = new List<SpeedInfos>();

            var triggerChamberLocal = triggerChamber + dataOutput.DistRange.Begin;
            var receptionChamberLocal = dataOutput.DistRange.End - receptionChamber; ;

            foreach (var speedInfo in calculationDiagData.SpeedInfos)
            {
                //  1. (кпп)
                if (speedInfo.Distance.Begin <= triggerChamberLocal &&
                    speedInfo.Distance.End <= triggerChamberLocal ||
                    speedInfo.Distance.Begin >= receptionChamberLocal &&
                    speedInfo.Distance.End >= receptionChamberLocal)
                {
                    result.Add(SpeedInfosAreaTypeKPP(name, speedInfo.Distance.Begin, speedInfo.Distance.End, speedInfo));
                    continue;
                }

                //  2.
                if (speedInfo.Distance.Begin < triggerChamberLocal &&
                    speedInfo.Distance.End < receptionChamberLocal)
                {
                    // часть до (кпп)
                    result.Add(SpeedInfosAreaTypeKPP(name, speedInfo.Distance.Begin, triggerChamberLocal, speedInfo));
                    //часть после (лч)
                    result.Add(SpeedInfosAreaTypeLP(name, triggerChamberLocal, speedInfo.Distance.End, speedInfo));
                    continue;
                }

                //  3.
                if (speedInfo.Distance.Begin < triggerChamberLocal &&
                    speedInfo.Distance.End > receptionChamberLocal)
                {
                    // часть до (кпп)
                    result.Add(SpeedInfosAreaTypeKPP(name, speedInfo.Distance.Begin, triggerChamberLocal, speedInfo));
                    //середина (лч)
                    result.Add(SpeedInfosAreaTypeLP(name, triggerChamberLocal, receptionChamberLocal, speedInfo));
                    //часть после (кпп)
                    result.Add(SpeedInfosAreaTypeKPP(name, receptionChamberLocal, speedInfo.Distance.End, speedInfo));
                    continue;
                }

                //  4. (лч)
                if (speedInfo.Distance.Begin >= triggerChamberLocal &&
                    speedInfo.Distance.End <= receptionChamberLocal)
                {
                    result.Add(SpeedInfosAreaTypeLP(name, speedInfo.Distance.Begin, speedInfo.Distance.End, speedInfo));
                    continue;
                }

                //  5.
                if (speedInfo.Distance.Begin > triggerChamberLocal &&
                    speedInfo.Distance.End > receptionChamberLocal)
                {
                    // часть до (лч)
                    result.Add(SpeedInfosAreaTypeLP(name, speedInfo.Distance.Begin, receptionChamberLocal, speedInfo));
                    //часть после (кпп)
                    result.Add(SpeedInfosAreaTypeKPP(name, receptionChamberLocal, speedInfo.Distance.End, speedInfo));
                }
            }
            return result;
        }

        private string GetAreaSpeedInfoName(DiagData diagData)
        {
            switch (diagData.DataType)
            {
                case DataType.Spm: return "Spm";
                case DataType.Mpm: return "Mpm";
                case DataType.Wm: return "Wm";
                case DataType.MflT1: return "MflT1";
                case DataType.MflT11: return "MflT11";
                case DataType.MflT3: return "MflT3";
                case DataType.MflT31: return "MflT31";
                case DataType.MflT32: return "MflT32";
                case DataType.MflT33: return "MflT33";
                case DataType.MflT34: return "MflT34";
                case DataType.TfiT4: return "TfiT4";
                case DataType.TfiT41: return "TfiT41";
                case DataType.Cdl: return "Cdl";
                case DataType.Cdc: return "Cdc";
                case DataType.Cd360:
                    var cd360DiagData = (CdmDiagData)diagData;
                    return $"{cd360DiagData.DirectionName}({cd360DiagData.Angle}-{cd360DiagData.EntryAngle})";
                case DataType.Ema: return "Ema";
                default: return "Не определен";
            }
        }

        private SpeedInfos SpeedInfosAreaTypeKPP(string name, double begin, double end, OverSpeedInfo speedInfo)
        {
            return new SpeedInfos
            {
                Name = name,
                Begin = begin,
                End = end,
                Speed = speedInfo.Speed,
                Area = speedInfo.Area,
                AreaType = true
            };
        }

        private SpeedInfos SpeedInfosAreaTypeLP(string name, double begin, double end, OverSpeedInfo speedInfo)
        {
            return new SpeedInfos
            {
                Name = name,
                Begin = begin,
                End = end,
                Speed = speedInfo.Speed,
                Area = speedInfo.Area,
                AreaType = false
            };
        }

        private void InnerGetAreaTypeSpeedInfo(DiagData diagData, Dictionary<enCommonDataType, DiagDataMain> diagDataMain, ReferenceInputData dataOutput, enCommonDataType cDataType)
        {
            if (!diagDataMain.ContainsKey(cDataType))
                diagDataMain.Add(cDataType, new DiagDataMain()
                {
                    PassportSpeedDiapason = diagData.PassportSpeedDiapason,
                    NameTypeData = cDataType.ToString(),
                    SpeedInfos = new List<SpeedInfos>()
                });

            diagDataMain[cDataType].SpeedInfos.AddRange(CreateSpeedInfos(dataOutput, diagData));

            foreach (var overSpeedInfo in diagData.SpeedInfos)
                diagDataMain[cDataType].Area += overSpeedInfo.Area;
        }

        private void CreateTableByFrames(Calculation calculation)
        {
            var workSheet = workBook.Worksheets[worksheetNumber];
            var firstRowIndex = 5;

            var framesType = calculation.FramesTypeCds ? "CDS" : "CDL";
            var tableHeader = calculation.Frames.Count > 0
                ? "Список рамок, соответствующих критериям поперечных сварных швов"
                : "Рамки, соответствующих критериям поперечных сварных швов, отсутствуют";

            workSheet.Cells[firstRowIndex, 0].Value = $"В результате выполненых расчетов ДД соответствует типу {framesType}";
            workSheet.Cells.Merge(firstRowIndex, 0, 1, 4);
            TableDesign(worksheetNumber, 1, 4, firstRowIndex, 0, Styles.DifferentFontStyle(16));

            workSheet.Cells[firstRowIndex + 1, 0].Value = tableHeader;
            workSheet.Cells.Merge(firstRowIndex + 1, 0, 1, 4);
            TableDesign(worksheetNumber, 1, 4, firstRowIndex + 1, 0, Styles.BaseStyle(11));

            if (calculation.Frames.Count != 0)
            {
                workSheet.Cells[firstRowIndex + 2, 0].Value = @"№ п/п";
                workSheet.Cells[firstRowIndex + 2, 1].Value = "Дистанция начала, м";
                workSheet.Cells[firstRowIndex + 2, 2].Value = "Дистанция конца, м";
                TableDesign(worksheetNumber, 1, 3, firstRowIndex + 2, 0, Styles.DifferentFontStyle(11));

                var rowOffset = firstRowIndex + 3;
                var rowIndex = 0;
                foreach (var frame in calculation.Frames)
                {
                    workSheet.Cells[rowOffset, 0].Value = $"{rowIndex + 1}";
                    workSheet.Cells[rowOffset, 1].Value = Math.Round(frame.Left, 2);
                    workSheet.Cells[rowOffset, 2].Value = Math.Round(frame.Right, 2);
                    rowOffset++;
                    rowIndex++;
                }

                TableDesign(worksheetNumber, rowOffset - firstRowIndex - 3, 3, firstRowIndex + 3, 0, Styles.BaseStyle());
            }
            workSheet.AutoFitColumns(0, 2);
            workSheet.Cells.SetColumnWidth(3, 30);
        }

        private void CreateHeaderTableByFrames(ReferenceInputData referenceInputData)
        {
            var workSheet = workBook.Worksheets[worksheetNumber];
            var firstRowIndex = 0;

            workSheet.Cells[firstRowIndex, 0].Value = "Название прогона: ";
            workSheet.Cells[firstRowIndex, 1].Value = referenceInputData.WorkItemName;
            workSheet.Cells[firstRowIndex + 1, 0].Value = "Диаметр, мм: ";
            workSheet.Cells[firstRowIndex + 1, 1].Value = referenceInputData.Diameter;
            workSheet.Cells[firstRowIndex + 2, 0].Value = "Организация: ";
            workSheet.Cells[firstRowIndex + 2, 1].Value = referenceInputData.Contractor.Value;
            workSheet.Cells[firstRowIndex + 3, 0].Value = "Трубопровод: ";
            workSheet.Cells[firstRowIndex + 3, 1].Value = referenceInputData.PipeLine.Value;
            workSheet.Cells[firstRowIndex + 4, 0].Value = "Участок: ";
            workSheet.Cells[firstRowIndex + 4, 1].Value = referenceInputData.Route.Value;

            workSheet.Cells[firstRowIndex, 2].Value = "Дата пропуска: ";
            workSheet.Cells[firstRowIndex, 3].Value = referenceInputData.DateWorkItem;
            workSheet.Cells[firstRowIndex + 1, 2].Value = "Протяженность по ВИП, м: ";
            workSheet.Cells[firstRowIndex + 1, 3].Value = Math.Round(referenceInputData.DistRange.End - referenceInputData.DistRange.Begin);
            workSheet.Cells[firstRowIndex + 3, 2].Value = "Дефектоскоп №: ";
            workSheet.Cells[firstRowIndex + 3, 3].Value = referenceInputData.FlawDetector;
            workSheet.Cells[firstRowIndex + 4, 2].Value = "Ответственный за пропуск: ";
            workSheet.Cells[firstRowIndex + 4, 3].Value = referenceInputData.ResponsibleWorkItem;

            TableDesign(worksheetNumber, 5, 1, 0, 0, Styles.BaseStyle());
            TableDesign(worksheetNumber, 5, 1, 0, 1, Styles.DifferentFontStyle());
            TableDesign(worksheetNumber, 5, 1, 0, 2, Styles.BaseStyle());
            TableDesign(worksheetNumber, 5, 1, 0, 3, Styles.DifferentFontStyle());
        }

        private Dictionary<enCommonDataType, DiagDataMain> GetAreaTypeSpeedInfo(ReferenceInputData dataOutput, List<DiagData> diagDataList)
        {
            var diagDataMain = new Dictionary<enCommonDataType, DiagDataMain>();
            foreach (var diagData in diagDataList)
            {
                switch (diagData.DataType)
                {
                    case DataType.Spm:
                        InnerGetAreaTypeSpeedInfo(diagData, diagDataMain, dataOutput, enCommonDataType.Spm);
                        break;

                    case DataType.Mpm:
                        InnerGetAreaTypeSpeedInfo(diagData, diagDataMain, dataOutput, enCommonDataType.Mpm);
                        break;

                    case DataType.Wm:
                        InnerGetAreaTypeSpeedInfo(diagData, diagDataMain, dataOutput, enCommonDataType.Wm);
                        break;

                    case DataType.MflT1:
                    case DataType.MflT11:
                    case DataType.MflT3:
                    case DataType.MflT31:
                    case DataType.MflT32:
                    case DataType.MflT33:
                    case DataType.MflT34:
                    case DataType.TfiT4:
                    case DataType.TfiT41:
                        InnerGetAreaTypeSpeedInfo(diagData, diagDataMain, dataOutput, enCommonDataType.Mfl);
                        break;

                    case DataType.Cdl:
                    case DataType.Cdc:
                    case DataType.Cd360:
                        InnerGetAreaTypeSpeedInfo(diagData, diagDataMain, dataOutput, enCommonDataType.Cd);
                        break;
                    case DataType.Ema:
                        InnerGetAreaTypeSpeedInfo(diagData, diagDataMain, dataOutput, enCommonDataType.Ema);
                        break;
                }
            }
            return diagDataMain;
        }

        private void CreateTableBySensors(AnalysisType analysisType)
        {
            var startRow = 9;
            var shiftColumns = 0;
            var workSheet = workBook.Worksheets[worksheetNumber];
            var count = Math.Ceiling((double)analysisType.RowDataSorted.Count / MaxRowSheet);  //  количество листов с ПДИ для одного типа данных
            var index = 12;
            var rowDataIndex = 0;
            var areaContiguous = 0.0d;

            workSheet.Cells[6, 4].Value = Math.Round(analysisType.AreaKpp, 3);
            workSheet.Cells[7, 4].Value = Math.Round(analysisType.AreaLdi - analysisType.AreaKpp, 3);
            workSheet.Cells[8, 4].Value = Math.Round(analysisType.AreaLdi, 3);
            workSheet.Cells[9, 4].Value = Math.Round(analysisType.NumberSensorsBlock == 0? analysisType.AreaContiguous : analysisType.AreaContiguous * analysisType.NumberSensorsBlock, 3);

            var s = workSheet.Cells[9, 4].GetStyle();
            //s.BackgroundColor = Color.ForestGreen;
            s.ForegroundColor = Color.ForestGreen;
            workSheet.Cells[9, 4].SetStyle(s);

            if (analysisType.RowDataSorted.Count == 0)
            {
                workSheet.Cells[startRow, 0].Value = "Потери не обнаружены";
                workSheet.Cells.Merge(startRow, 0, 1, 4);
                TableDesign(worksheetNumber, 1, 4, startRow, 0, Styles.DifferentFontStyle());
                return;
            }

            var isDataTypeBlocksorСontiguous = IsDataTypeBlocksorСontiguous(analysisType.DataType.ToString());
            //var column = 0;
            //заполнение таблицы данными с учетом максимального значения ячеек
            for (var i = 0; i < count; i++)
            {
                workSheet.Cells[index - 1, 0].Value = "№ п/п";
                workSheet.Cells[index - 1, 1].Value = "№ датчика";
                if (analysisType.DoubleAngle)
                {
                    workSheet.Cells[index - 1, 2].Value = "Направление датчика";
                    workSheet.Cells[index - 1, 3].Value = "Угол ввода";
                    shiftColumns = 2;
                }
                workSheet.Cells[index - 1, 2 + shiftColumns].Value = "Дистанция начала ПДИ, м";
                workSheet.Cells[index - 1, 3 + shiftColumns].Value = "Дистанция конца ПДИ, м";
                workSheet.Cells[index - 1, 4 + shiftColumns].Value = "Площадь ПДИ, м\u00B2";
                workSheet.Cells[index - 1, 5 + shiftColumns].Value = "Тип ПДИ";
                workSheet.Cells[index - 1, 6 + shiftColumns].Value = "Тип участка";

                if (isDataTypeBlocksorСontiguous)
                    workSheet.Cells[index - 1, 7 + shiftColumns].Value = "Площадь по блокам смежных датчиков м\u00B2";
                else
                    workSheet.Cells[index - 1, 7 + shiftColumns].Value = "Площадь по смежным, датчикам м\u00B2";

                for (var j = 0; j < 8 + shiftColumns; j++)
                    workSheet.Cells[index, j].Value = j + 1;

                workSheet.Cells[index - 2, 0].Value = $"ТИП ДАТЧИКОВ ({analysisType.DataType.ToString().ToUpper()})";
                workSheet.Cells.Merge(index - 2, 0, 1, 8 + shiftColumns);

                TableDesign(worksheetNumber, 1, 8 + shiftColumns, index - 2, 0, Styles.DifferentFontBorderType(16));
                TableDesign(worksheetNumber, 2, 8 + shiftColumns, index - 1, 0, Styles.DifferentFontBorderType(11));
                index++;
                // TODO(plzSomeone): rewrite this crap
                //  костыль! проверка для ставых MFL или Tfi т.к. нет инфы о количестве датчиков в блоке
                if (analysisType.NumberSensorsBlock == 0 && isDataTypeBlocksorСontiguous) return;
                ///////////////////////////////////////////////
                for (var j = 0; j + index < MaxRowSheet; j++)
                {
                    workSheet.Cells[index + j, 0].Value = j + 1;
                    workSheet.Cells[index + j, 1].Value = analysisType.RowDataSorted[rowDataIndex].SensorNumber;

                    if (analysisType.DoubleAngle)
                    {
                        workSheet.Cells[index + j, 2].Value = analysisType.RowDataSorted[rowDataIndex].DirectionSensor;
                        workSheet.Cells[index + j, 3].Value = Math.Round(analysisType.RowDataSorted[rowDataIndex].InputAngle, 1);
                        shiftColumns = 2;
                    }
                    workSheet.Cells[index + j, 2 + shiftColumns].Value = Math.Round(analysisType.RowDataSorted[rowDataIndex].StartDistance, 3);
                    workSheet.Cells[index + j, 3 + shiftColumns].Value = Math.Round(analysisType.RowDataSorted[rowDataIndex].EndDistance, 3);
                    workSheet.Cells[index + j, 4 + shiftColumns].Value = Math.Round(analysisType.RowDataSorted[rowDataIndex].Area, 3);
                    workSheet.Cells[index + j, 5 + shiftColumns].Value = SensorRangeTypeToString(analysisType.RowDataSorted[rowDataIndex].RangeType);
                    workSheet.Cells[index + j, 6 + shiftColumns].Value = PipeTypeToString(analysisType.RowDataSorted[rowDataIndex].PipeType);
                    if (isDataTypeBlocksorСontiguous && analysisType.SensorsСontiguous.Count > 0) // TODO(plzSomeone): rewrite this crap
                    {
                        var id = (int)Math.Ceiling((double)analysisType.RowDataSorted[rowDataIndex].SensorNumber / analysisType.NumberSensorsBlock);
                        if (analysisType.SensorsСontiguous.ContainsKey(id))
                        {
                            var value = 0.0d;
                            foreach (var distanceСontiguous in analysisType.SensorsСontiguous[id])
                            {
                                if (distanceСontiguous.StartDistance >= analysisType.RowDataSorted[rowDataIndex].StartDistance &&
                                    distanceСontiguous.StartDistance <= analysisType.RowDataSorted[rowDataIndex].EndDistance &&
                                    distanceСontiguous.EndDistance >= analysisType.RowDataSorted[rowDataIndex].StartDistance &&
                                    distanceСontiguous.EndDistance <= analysisType.RowDataSorted[rowDataIndex].EndDistance)
                                {
                                    areaContiguous += Math.Round(distanceСontiguous.AreaContiguous ?? double.NaN, 6);
                                    value += Math.Round(distanceСontiguous.AreaContiguous ?? double.NaN, 6);
                                    workSheet.Cells[index + j, 7 + shiftColumns].Value = value;

                                    s = workSheet.Cells[index + j, 1].GetStyle();
                                    s.ForegroundColor = Color.ForestGreen;
                                    workSheet.Cells[index + j, 1].SetStyle(s);

                                    s = workSheet.Cells[index + j, 7 + shiftColumns].GetStyle();
                                    s.ForegroundColor = Color.ForestGreen;
                                    workSheet.Cells[index + j, 7 + shiftColumns].SetStyle(s);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (analysisType.SensorsСontiguous.ContainsKey(analysisType.RowDataSorted[rowDataIndex].SensorNumber))
                        {
                            var value = 0.0d;
                            foreach (var distanceСontiguous in analysisType.SensorsСontiguous[analysisType.RowDataSorted[rowDataIndex].SensorNumber])
                            {
                                if (distanceСontiguous.StartDistance >= analysisType.RowDataSorted[rowDataIndex].StartDistance &&
                                    distanceСontiguous.StartDistance <= analysisType.RowDataSorted[rowDataIndex].EndDistance &&
                                    distanceСontiguous.EndDistance >= analysisType.RowDataSorted[rowDataIndex].StartDistance &&
                                    distanceСontiguous.EndDistance <= analysisType.RowDataSorted[rowDataIndex].EndDistance)
                                {
                                    areaContiguous += Math.Round(distanceСontiguous.AreaContiguous ?? double.NaN, 6);
                                    value += Math.Round(distanceСontiguous.AreaContiguous ?? double.NaN, 6);
                                    workSheet.Cells[index + j, 7 + shiftColumns].Value = value;

                                    s = workSheet.Cells[index + j, 1].GetStyle();
                                    s.ForegroundColor = Color.ForestGreen;
                                    workSheet.Cells[index + j, 1].SetStyle(s);

                                    s = workSheet.Cells[index + j, 7 + shiftColumns].GetStyle();
                                    s.ForegroundColor = Color.ForestGreen;
                                    workSheet.Cells[index + j, 7 + shiftColumns].SetStyle(s);
                                }
                            }
                        }
                    }

                    if (analysisType.RowDataSorted.Count == rowDataIndex + 1)
                    {
                        var lastRow = rowDataIndex - MaxRowSheet * i + index + 1;
                        var totaiRow = lastRow - index;
                        TableDesign(worksheetNumber, totaiRow, 8 + shiftColumns, index);
                        workSheet.AutoFitColumns(5, 8);
                        return;
                    }
                    rowDataIndex++;
                }

                TableDesign(worksheetNumber, MaxRowSheet, 8 + shiftColumns, index);
                workBook.Worksheets.Add( analysisType.CdmType == enCdmDirectionName.None                         ?
                                         $"ПДИ по датчикам {analysisType.DataType.ToString().ToUpper()}_{i + 1}" :
                                         $"ПДИ по датчикам {analysisType.CdmType.ToString().ToUpper()}_{i + 1}"   );
                workSheet.AutoFitColumns(5, 8);
                worksheetNumber++;
                workSheet = workBook.Worksheets[worksheetNumber];
                index = 2;
            }
        }


        private void TableDesign(int workSheetNumber, int totalRow, int totalColumn, int firstRow = 0, int firstColumn = 0, Style style = null)
        {
            if (style == null)
                style = Styles.BaseStyle();
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

        private void RestartVip(List<RestartCriterion> restartReport, int worksheetNumber, int rowIndex)
        {
            var workSheet = workBook.Worksheets[worksheetNumber];
            var headerRowIndex = rowIndex + 3;
            var tableRowIndex = rowIndex + 5;
            var columnIndex = 1;
            if (restartReport.Count == 0)
            {
                workSheet.Cells[headerRowIndex, columnIndex].Value = "Перепуск ВИП не требуется. Отсутствуют количественные критерии перепуска.";
                workSheet.Cells.Merge(headerRowIndex, columnIndex, 1, 5);
                TableDesign(worksheetNumber, 1, 5, headerRowIndex, columnIndex, Styles.Boldface(11));
            }
            else
            {
                workSheet.Cells[headerRowIndex, columnIndex].Value = "Необходим перепуск ВИП. Присутствуют количественные критерии перепуска:";
                workSheet.Cells.Merge(headerRowIndex, columnIndex, 1, 5);
                //  шапка таблицы
                workSheet.Cells[tableRowIndex, 0].Value = @"№ п\п";
                workSheet.Cells[tableRowIndex, 1].Value = "Критерий";
                workSheet.Cells.Merge(tableRowIndex, columnIndex, 1, 5);
                workSheet.Cells[tableRowIndex, 6].Value = "Тип ДД";
                tableRowIndex++;
                //  нумерация столбцов
                workSheet.Cells[tableRowIndex, 0].Value = "1";
                workSheet.Cells[tableRowIndex, 1].Value = "2";
                workSheet.Cells.Merge(tableRowIndex, columnIndex, 1, 5);
                workSheet.Cells[tableRowIndex, 6].Value = "3";
                tableRowIndex++;

                TableDesign(worksheetNumber, 2, 7, tableRowIndex - 2, 0, Styles.DifferentFontStyle(11));

                int i;
                for (i = 0; i < restartReport.Count; i++)
                {
                    var a = restartReport[i];
                    workSheet.Cells[tableRowIndex, 0].Value = i + 1;
                    workSheet.Cells[tableRowIndex, 1].Value = restartReport[i].Criterion;
                    workSheet.Cells.Merge(tableRowIndex, columnIndex, 1, 5);
                    workSheet.Cells[tableRowIndex, 6].Value = restartReport[i].DataType;
                    tableRowIndex++;
                }
                TableDesign(worksheetNumber, i, 7, tableRowIndex - i, 0, Styles.SimpleStyle());
            }
        }

        private bool IsDataTypeBlocksorСontiguous(string dataType)
        {
            return dataType == DataType.TfiT4.ToString() ||
                   dataType == DataType.TfiT41.ToString() ||
                   dataType == DataType.MflT1.ToString() ||
                   dataType == DataType.MflT11.ToString() ||
                   dataType == DataType.MflT2.ToString() ||
                   dataType == DataType.MflT22.ToString() ||
                   dataType == DataType.MflT3.ToString() ||
                   dataType == DataType.MflT31.ToString() ||
                   dataType == DataType.MflT32.ToString() ||
                   dataType == DataType.MflT33.ToString() ||
                   dataType == DataType.MflT34.ToString();
        }

        private string SensorRangeTypeToString(enSensorRangeType type)
        {
            switch (type)
            {
                case enSensorRangeType.Failure:
                    return "Отказ";
                case enSensorRangeType.Other:
                    return @"ВГС\загрязнение\повреждение";
                default: return string.Empty;
            }
        }

        private string PipeTypeToString(enPipeType type)
        {
            switch (type)
            {
                case enPipeType.Linear:
                    return "Линейная часть";
                case enPipeType.Сhamber:
                    return "КПП СОД";
                default: return string.Empty;
            }
        }

        private void CreateHeaderNavTable()
        {
            var workSheet = workBook.Worksheets[worksheetNumber];

            workSheet.Cells[0, 0].Value = "Этапы \nконтроля";
            workSheet.Cells[0, 1].Value = "Период фиксирования значения \nданных для расчёта";
            workSheet.Cells[0, 2].Value = "Рассчитанные значения данных";
            workSheet.Cells[0, 3].Value = "Контроль значений данных";
            workSheet.Cells[0, 4].Value = "Расшифровка";
            workSheet.Cells[0, 5].Value = "Примечание";

            TableDesign(worksheetNumber, 1, 6, 0, 0, Styles.DifferentFontBorderType(11));
        }

        private void CreateBodyNavTable(Calculation calculation)
        {
            var rowOffset = 1;

            switch (calculation.NavigationInfo.NavType)
            {
                case enNavType.Bep:
                    CreateBodyNavBepTable(calculation, ref rowOffset);
                    break;
                case enNavType.Adis:
                    CreateBodyNavAdisTable(calculation, ref rowOffset);
                    break;
                case enNavType.Bins:
                    CreateBodyNavBinsTable(calculation, ref rowOffset);
                    break;
            }

            var workSheet = workBook.Worksheets[worksheetNumber];

            TableDesign(worksheetNumber, rowOffset, 5, 1, 1, Styles.SimpleStyle());
            TableDesign(worksheetNumber, rowOffset, 1, 1, 0, Styles.DifferentFontBorderType(11));

            workSheet.AutoFitColumns();
        }

        private void CreateBodyNavBepTable(Calculation calculation, ref int rowOffset)
        {
            var workSheet = workBook.Worksheets[worksheetNumber];
            var navCalcParams = ConstNavCalcParams.GetAllParams().FirstOrDefault(item => item.NavType == calculation.NavigationInfo.NavType);
            var navInfo = calculation.NavigationInfo;
            rowOffset = 1;
            if (navCalcParams == null) return;
            CreateBodyNavTableItems(workSheet, ref rowOffset, navInfo, navCalcParams);
        }

        private void CreateBodyNavAdisTable(Calculation calculation, ref int rowOffset)
        {
            var workSheet = workBook.Worksheets[worksheetNumber];
            var navCalcParams = ConstNavCalcParams.GetAllParams().FirstOrDefault(item => item.NavType == calculation.NavigationInfo.NavType);
            var navInfo = calculation.NavigationInfo;
            rowOffset = 1;
            if (navCalcParams == null) return;
            CreateBodyNavTableItems(workSheet, ref rowOffset, navInfo, navCalcParams);
        }

        private void CreateBodyNavBinsTable(Calculation calculation, ref int rowOffset)
        {
            var workSheet = workBook.Worksheets[worksheetNumber];
            var navCalcParams = ConstNavCalcParams.GetAllParams().FirstOrDefault(item => item.NavType == calculation.NavigationInfo.NavType);
            var navInfo = calculation.NavigationInfo;
            if (navCalcParams == null) return;

            CreateBodyNavSquareDeviationBins(workSheet, ref rowOffset, navInfo, navCalcParams);
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 2;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavLastMinuteSetupPhase;
            NavCellsFormatting.GravAccel(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.GravitationalAcceleration, 3));
            NavCellsFormatting.GravAccel(workSheet.Cells[rowOffset, 3], navCalcParams.GravAccel);
            NavCellsFormatting.GravAccelDesc(workSheet.Cells[rowOffset, 4]);
            NavCalcStatus(navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.GravAccel), workSheet.Cells[rowOffset, 5], Resources.NormalAccelerometer, Resources.WrongAccelerometer);

            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 3;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavLastMinuteSetupPhase;
            NavCellsFormatting.EarthAngularSpeedRotation(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.EarthAngularSpeedRotation, 3));
            NavCellsFormatting.EarthAngularSpeedRotation(workSheet.Cells[rowOffset, 3], navCalcParams.EarthAngularSpeedRotation);
            NavCellsFormatting.EarthAngularSpeedRotationDesc(workSheet.Cells[rowOffset, 4]);
            NavCalcStatus(navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.EarthAngularSpeedRotation),
                workSheet.Cells[rowOffset, 5], Resources.NormalGyroscope, Resources.WrongGyroscope);
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 4;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavLastMinuteSetupPhase;
            NavCellsFormatting.DiffLatitude(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.DifferenceLatitudes, 3));
            NavCellsFormatting.DiffLatitude(workSheet.Cells[rowOffset, 3], navCalcParams.DifferenceLatitudes);
            NavCellsFormatting.DiffLatitudeDesc(workSheet.Cells[rowOffset, 4]);
            NavCalcStatus(navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.DiffLatitude),
                workSheet.Cells[rowOffset, 5], Resources.NormalGyroscope, Resources.WrongGyroscope);
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 5;
            workSheet.Cells.Merge(rowOffset, 0, 2, 1);
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            workSheet.Cells.Merge(rowOffset, 1, 2, 1);
            NavCellsFormatting.AccelSum(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AccelSum, 3));
            NavCellsFormatting.AccelSum(workSheet.Cells[rowOffset, 3], navCalcParams.AccelSum);
            NavCellsFormatting.AccelSumDesc(workSheet.Cells[rowOffset, 4]);
            NavCalcStatus(navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.AccelSum), workSheet.Cells[rowOffset, 5], Resources.NormalAccelerometer, Resources.WrongAccelerometer);
            rowOffset++;

            NavCellsFormatting.AccelMax(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AccelMax, 3));
            NavCellsFormatting.AccelMax(workSheet.Cells[rowOffset, 3], navCalcParams.AccelMax);
            NavCellsFormatting.AccelMaxDesc(workSheet.Cells[rowOffset, 4]);
            NavCalcStatus(navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.AccelMax), workSheet.Cells[rowOffset, 5], Resources.NormalAccelerometer, Resources.WrongAccelerometer);
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 6;
            workSheet.Cells.Merge(rowOffset, 0, 2, 1);
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            workSheet.Cells.Merge(rowOffset, 1, 2, 1);
            NavCellsFormatting.AngularSpeedSum(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AngularSpeedSum, 3));
            NavCellsFormatting.AngularSpeedSum(workSheet.Cells[rowOffset, 3], navCalcParams.AngularSpeedSum);
            NavCellsFormatting.AngularSpeedSumDesc(workSheet.Cells[rowOffset, 4]);
            NavCalcStatus(navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.AngularSpeedSum),
                workSheet.Cells[rowOffset, 5], Resources.NormalGyroscope, Resources.WrongGyroscope);
            rowOffset++;

            NavCellsFormatting.AngularSpeedMax(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AngularSpeedMax, 3));
            NavCellsFormatting.AngularSpeedMax(workSheet.Cells[rowOffset, 3], navCalcParams.AngularSpeedMax);
            NavCellsFormatting.AngularSpeedMaxDesc(workSheet.Cells[rowOffset, 4]);
            NavCalcStatus(navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.AngularSpeedMax),
                workSheet.Cells[rowOffset, 5], Resources.NormalGyroscope, Resources.WrongGyroscope);
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 7;
            workSheet.Cells.Merge(rowOffset, 0, 3, 1);
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            workSheet.Cells.Merge(rowOffset, 1, 3, 1);
            NavCellsFormatting.AverageRollAngle(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AverageRollAngle, 3));
            NavCellsFormatting.AverageRollAngle(workSheet.Cells[rowOffset, 3], navCalcParams.AverageRollAngle);
            NavCellsFormatting.AverageRollAngleDesc(workSheet.Cells[rowOffset, 4]);
            NavCalcStatus(navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.AverageRollPitchAngle),
                workSheet.Cells[rowOffset, 5], Resources.NormalGyroscope, Resources.WrongGyroscope);
            workSheet.Cells.Merge(rowOffset, 5, 3, 1);
            rowOffset++;

            NavCellsFormatting.AveragePitchAngle(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AveragePitchAngle, 3));
            NavCellsFormatting.AveragePitchAngle(workSheet.Cells[rowOffset, 3], navCalcParams.AveragePitchAngle);
            NavCellsFormatting.AveragePitchAngleDesc(workSheet.Cells[rowOffset, 4]);
            rowOffset++;

            //  Убраны из отчета по требованию 35004(Качество НД, внести изменения в критерии для БИНС)
            //MaxRollAngle(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.MaxRollAngle, 3));
            //MaxRollAngle(workSheet.Cells[rowOffset, 3], navCalcParams.MaxRollAngle);
            //MaxRollAngleDesc(workSheet.Cells[rowOffset, 4]);
            //NavCalcStatus(navInfo.NavCalcState.HasFlag(NavCalcStateTypes.MaxRollPitchAngle),
            //    workSheet.Cells[rowOffset, 5], Resources.NormalGyroscope, Resources.WrongGyroscope);
            //workSheet.Cells.Merge(rowOffset, 5, 2, 1);
            //rowOffset++;

            NavCellsFormatting.MaxPitchAngle(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.MaxPitchAngle, 3));
            NavCellsFormatting.MaxPitchAngle(workSheet.Cells[rowOffset, 3], navCalcParams.MaxPitchAngle);
            NavCellsFormatting.MaxPitchAngleDesc(workSheet.Cells[rowOffset, 4]);
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 8;
            workSheet.Cells.Merge(rowOffset, 0, 4, 1);
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            NavCellsFormatting.PitchAngleAtMovement(workSheet.Cells[rowOffset, 2], navInfo.PitchAngleAtMovement);
            NavCellsFormatting.PitchAngleAtMovement(workSheet.Cells[rowOffset, 3], navCalcParams.PitchAngleAtMovement); // угол тангажа > 4 градусов
            NavCellsFormatting.PitchAngleAtMovementDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.PitchAngleAtMovement) ? NavCellsFormatting.GasEnvironment : string.Empty;
            workSheet.Cells.Merge(rowOffset, 5, 4, 1);
            rowOffset++;

            NavCellsFormatting.PitchAngleSectionPhaseDesc(workSheet.Cells[rowOffset, 1], 6);
            NavCellsFormatting.PitchAngleSection(workSheet.Cells[rowOffset, 2], 6, navInfo.PitchAngleSection6);
            NavCellsFormatting.PitchAverageValue(workSheet.Cells[rowOffset, 3], navCalcParams.PitchAngle6);
            NavCellsFormatting.PitchAngleSectionDesc(workSheet.Cells[rowOffset, 4], 6);
            rowOffset++;

            NavCellsFormatting.PitchAngleSectionPhaseDesc(workSheet.Cells[rowOffset, 1], 3);
            NavCellsFormatting.PitchAngleSection(workSheet.Cells[rowOffset, 2], 3, navInfo.PitchAngleSection3);
            NavCellsFormatting.PitchAverageValue(workSheet.Cells[rowOffset, 3], navCalcParams.PitchAngle3);
            NavCellsFormatting.PitchAngleSectionDesc(workSheet.Cells[rowOffset, 4], 3);
            rowOffset++;

            NavCellsFormatting.PitchAngleSectionPhaseDesc(workSheet.Cells[rowOffset, 1], 2);
            NavCellsFormatting.PitchAngleSection(workSheet.Cells[rowOffset, 2], 2, navInfo.PitchAngleSection2);
            NavCellsFormatting.PitchAverageValue(workSheet.Cells[rowOffset, 3], navCalcParams.PitchAngle2);
            NavCellsFormatting.PitchAngleSectionDesc(workSheet.Cells[rowOffset, 4], 2);
        }

        private void CreateBodyNavTableItems(Worksheet workSheet, ref int rowOffset, NavigationInfo navInfo, NavCalcParams navCalcParams)
        {
            var index = rowOffset;
            workSheet.Cells[rowOffset, 0].Value = index;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavSetupPhase;
            NavCellsFormatting.GravAccel(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.GravitationalAcceleration, 3));
            NavCellsFormatting.GravAccel(workSheet.Cells[rowOffset, 3], navCalcParams.GravAccel);
            NavCellsFormatting.GravAccelDesc(workSheet.Cells[rowOffset, 4]);
            NavCalcStatus(navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.GravAccel), workSheet.Cells[rowOffset, 5], Resources.NormalAccelerometer, Resources.WrongAccelerometer);
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = index + 1;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavSetupPhase;
            NavCellsFormatting.EarthAngularSpeedRotation(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.EarthAngularSpeedRotation, 3));
            NavCellsFormatting.EarthAngularSpeedRotation(workSheet.Cells[rowOffset, 3], navCalcParams.EarthAngularSpeedRotation, false);
            NavCellsFormatting.EarthAngularSpeedRotationDesc(workSheet.Cells[rowOffset, 4]);
            NavCalcStatus(navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.EarthAngularSpeedRotation),
                workSheet.Cells[rowOffset, 5], Resources.NormalGyroscope, Resources.WrongGyroscope);
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = index + 2;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            NavCellsFormatting.AccelSum(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AccelSum, 3));
            NavCellsFormatting.AccelSum(workSheet.Cells[rowOffset, 3], navCalcParams.AccelSum);
            NavCellsFormatting.AccelSumDesc(workSheet.Cells[rowOffset, 4]);
            NavCalcStatus(navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.AccelSum), workSheet.Cells[rowOffset, 5], Resources.NormalAccelerometer, Resources.WrongAccelerometer);
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = index + 3;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            NavCellsFormatting.AccelMax(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AccelMax, 3));
            NavCellsFormatting.AccelMax(workSheet.Cells[rowOffset, 3], navCalcParams.AccelMax);
            NavCellsFormatting.AccelMaxDesc(workSheet.Cells[rowOffset, 4]);
            NavCalcStatus(navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.AccelMax), workSheet.Cells[rowOffset, 5], Resources.NormalAccelerometer, Resources.WrongAccelerometer);
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = index + 4;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            NavCellsFormatting.AngularSpeedSum(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AngularSpeedSum, 3));
            NavCellsFormatting.AngularSpeedSum(workSheet.Cells[rowOffset, 3], navCalcParams.AngularSpeedSum);
            NavCellsFormatting.AngularSpeedSumDesc(workSheet.Cells[rowOffset, 4]);
            NavCalcStatus(navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.AngularSpeedSum),
                workSheet.Cells[rowOffset, 5], Resources.NormalGyroscope, Resources.WrongGyroscope);
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = index + 5;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            NavCellsFormatting.AngularSpeedMax(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AngularSpeedMax, 3));
            NavCellsFormatting.AngularSpeedMax(workSheet.Cells[rowOffset, 3], navCalcParams.AngularSpeedMax);
            NavCellsFormatting.AngularSpeedMaxDesc(workSheet.Cells[rowOffset, 4]);
            NavCalcStatus(navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.AngularSpeedMax),
                workSheet.Cells[rowOffset, 5], Resources.NormalGyroscope, Resources.WrongGyroscope);
        }

        private void CreateBodyNavSquareDeviationBins(Worksheet workSheet, ref int rowOffset, NavigationInfo navInfo, NavCalcParams navCalcParams)
        {
            workSheet.Cells[rowOffset, 0].Value = 1;
            workSheet.Cells[rowOffset, 1].Value = Resources.SetupPhaseLast600Sec;
            NavCellsFormatting.PhaseLast600Sec(workSheet.Cells[rowOffset, 2], navInfo.NavSetupTime);
            NavCellsFormatting.PhaseLast600Sec(workSheet.Cells[rowOffset, 3], navCalcParams.Last600Sec);
            NavCellsFormatting.SetupNavTime(workSheet.Cells[rowOffset, 4]);
            NavCalcStatus(!navInfo.NavCalcState.HasFlag(enNavCalcStateTypes.WrongNavSetupTime),
                workSheet.Cells[rowOffset, 5], Resources.NormalGyroscopeAndOrAccelerometer, Resources.WrongGyroscopeAndOrAccelerometer + " (Δt<600c)");
        }

        private void NavCalcStatus(bool state, Cell cell, string normal, string wrong)
        {
            if (state)
            {
                cell.Value = normal;
            }
            else
            {
                cell.Value = wrong;
                var style = cell.GetStyle();
                style.ForegroundColor = System.Drawing.Color.Red;
                style.Pattern = BackgroundType.Solid;
                cell.SetStyle(style);
            }
        }
    }
}
