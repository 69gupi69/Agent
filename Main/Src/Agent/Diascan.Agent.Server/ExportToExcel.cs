using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aspose.Cells;
using Diascan.Agent.CalcDiagDataLossTask;
using Diascan.Agent.Manager.Properties;
using Diascan.Agent.ModelDB;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.DataProviders.NAV;
using DiCore.Lib.NDT.Types;

namespace Diascan.Agent.Manager
{
    public class RowDataAllTypes
    {
        public int SensorNumber { get; set; }   //  Номер датчика
        public string DirectionSensor { get; set; } //  Направление датчика
        public float InputAngle { get; set; }   //  Угол ввода
        public double StartDistance { get; set; }   //  Дистанция начала ПДИ
        public double EndDistance { get; set; } //  Дистанция конца ПДИ
        public double Area { get; set; }    //  Площадь ПДИ
        public bool RefusePdiType { get; set; }    //  (Отказ)/(ВГС\загрязнение\повреждение)
        public bool AreaType { get; set; }    //  (КПП СОД)/(Линейная часть)
    }

    public class SpeedInfos
    {
        public double Begin { get; set; }
        public double End { get; set; }
        public double Speed { get; set; }
        public double Area { get; set; }
        public bool AreaType { get; set; }
    }

    public class ExportToExcel
    {
        private Workbook workBook;
        private int worksheetNumber;
        private Dictionary<string, double> dictionaryAreasFail;
        private double areaFail;
        private double areaKpp;
        private const int MaxRowSheet = 65000;  //  максимальное число строк в одном листе
        private double receptionChamber;
        private double triggerChamber;

        public byte[] CreateExcelFile(Calculation calculation, out bool result)
        {
            result = false;
            dictionaryAreasFail = new Dictionary<string, double>();
            CalcDiagDataLossTask.DiagDataLossTask.CalcAreas(calculation);
            receptionChamber = calculation.DataOutput.ReceptionChamber;
            triggerChamber = calculation.DataOutput.TriggerChamber;
            
            workBook = new Workbook();
            worksheetNumber = 0;
            //  таблицы с ПДИ
            var dataTypes = new ControllerHelper().GroupDiagDataByType(calculation.DiagDataList);

            foreach (var dataType in dataTypes)
            {
                areaKpp = areaFail = 0;
                if (dataType.Key == enCdmDirectionName.None)
                {
                    foreach (var noneType in dataType.Value)
                    {
                        areaKpp = areaFail = 0;
                        workBook.Worksheets.Add("ПДИ по датчикам " + noneType.DataType.ToString().ToUpper());
                        worksheetNumber++;
                        //  сформировать шапку таблицы
                        CreateTableHeaderCalculationPDI(calculation.DataOutput);
                        //  начинаем считать все кроме сд360
                        var rowDataSorted = FillFieldsTableByPdi(noneType, null, out var doubleAngle);
                        dictionaryAreasFail.Add(noneType.DataType.ToString(), areaFail);
                        //  отрисовка заголовка таблицы и самой таблицы датчиков
                        CreateTableBySensors(noneType.DataType.ToString(), noneType.AreaLdi, rowDataSorted, doubleAngle);
                    }
                }
                else
                {
                    areaKpp = areaFail = 0;
                    workBook.Worksheets.Add("ПДИ по датчикам " + dataType.Key.ToString().ToUpper());
                    worksheetNumber++;
                    //  сформировать шапку таблицы
                    CreateTableHeaderCalculationPDI(calculation.DataOutput);
                    //  считает тут каждый тип сд360
                    var rowDataSorted = FillFieldsTableByPdi(null, dataType.Value.Select(item => (CdmDiagData)item).ToList(), out var doubleAngle);
                    dictionaryAreasFail.Add(dataType.Key.ToString(), areaFail);
                    //  отрисовка заголовка таблицы и самой таблицы датчиков
                    CreateTableBySensors(dataType.Key.ToString(), dataType.Value.Sum(q => q.AreaLdi), rowDataSorted.ToList(), doubleAngle);
                }
            }

            //  первый лист
            //  "шапка" таблицы
            CreateHeaderSummaryTable(0);
            //  отрисовка и заполние полей таблицы
            var rowIndex = CreateRowSummaryTable(dataTypes, 0);
            CreateCommonLinesSummaryTable(calculation, rowIndex);

            //  таблица перезапуска ВИП
            RestartVip(calculation.RestartReport, 0, rowIndex);

            //  ПДИ по скорости
            workBook.Worksheets.Add();
            worksheetNumber++;
            workBook.Worksheets[worksheetNumber].Name = "ПДИ по скорости";
            CreateTableBySpeed(calculation);


            //  ПДИ рамки
            if(calculation.CdChange)
            {
                workBook.Worksheets.Add();
                worksheetNumber++;
                workBook.Worksheets[worksheetNumber].Name = "Определение типа CD";
                CreateHeaderTableByFrames(calculation.DataOutput);
                CreateTableByFrames(calculation);
            }

            //  Навигация
            if (calculation.NavigationInfo.NavigationState.HasFlag(NavigationStateTypes.CalcNavigation))
            {
                workBook.Worksheets.Add();
                worksheetNumber++;
                workBook.Worksheets[worksheetNumber].Name = "Контроль качества НД";
                CreateHeaderNavTable();
                CreateBodyNavTable(calculation);
            }

            result = true;
            return workBook.SaveToStream().ToArray();
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
                TableDesign(worksheetNumber, 1, 5, headerRowIndex, columnIndex, Boldface(11));
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

                TableDesign(worksheetNumber, 2, 7, tableRowIndex - 2, 0, DifferentFontStyle(11));

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
                TableDesign(worksheetNumber, i, 7, tableRowIndex - i, 0, SimpleStyle());
            }
        }

        private void CreateTableHeaderCalculationPDI( ReferenceInputData dataOutput )
        {
            var workSheet = workBook.Worksheets[worksheetNumber];
            var row = 2;
            var column = 1;
            workSheet.Cells[0, 0].Value = "№ п/п";
            workSheet.Cells[0, 1].Value = "Результаты автоматического расчета потерь диагностической информации по участку \n" + dataOutput.Route.Value;
            workSheet.Cells.SetRowHeight(0, 40);
            workSheet.Cells.Merge(0, 1, 1, 4);
            for (var i = 0; i < 5; i++)
            {
                workSheet.Cells[1, i].Value = i + 1;
            }
            for (var i = 0; i < 7; i++)
            {
                workSheet.Cells[i + 2, 0].Value = i + 1;
            }

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

            TableDesign(worksheetNumber, 2, 5, 0, 0, DifferentFontBorderType(11));
            TableDesign(worksheetNumber, 7, 1, 2, 0, BaseDoubleCellBorderType(11));

            TableDesign(worksheetNumber, 7, 1, row, column, BaseDoubleCellBorderType(11));
            TableDesign(worksheetNumber, 7, 1, row, column + 1, DifferentFontBorderType(11));
            TableDesign(worksheetNumber, 7, 1, row, column + 2, BaseDoubleCellBorderType(11));
            TableDesign(worksheetNumber, 7, 1, row, column + 3, DifferentFontBorderType(11));
        }

        private int CreateRowSummaryTable(Dictionary<enCdmDirectionName, List<DiagData>> dataTypes, int worksheetNumber)
        {
            var workSheet = workBook.Worksheets[worksheetNumber];
            var rowIndex = 0;
            var firstRowIndex = 2;
            foreach (var dataType in dataTypes)
            {
                if (dataType.Key == enCdmDirectionName.None)
                {
                    foreach (var noneType in dataType.Value)
                    {
                        var areaVip = dictionaryAreasFail[noneType.DataType.ToString()];

                        //  № п\п
                        workSheet.Cells[rowIndex + firstRowIndex, 0].Value = rowIndex + 1;
                        //Вид диагностических данных
                        workSheet.Cells[rowIndex + firstRowIndex, 4].Value = noneType.DataType.ToString().ToUpper();
                        //  Суммарная площадь ПДИ, м2
                        workSheet.Cells[rowIndex + firstRowIndex, 6].Value = Math.Round(noneType.AreaLdi, 3);
                        //  Площадь ПДИ по причине отказа ВИП, м2
                        workSheet.Cells[rowIndex + firstRowIndex, 7].Value = Math.Round(areaVip, 3);
                        //  Площадь ПДИ по причинам ВГС\загрязнение\повреждение, м2
                        workSheet.Cells[rowIndex + firstRowIndex, 8].Value = Math.Round(noneType.AreaLdi - areaVip, 3);
                        rowIndex++;
                    }
                }
                else
                {
                    var areaSum = dataType.Value.Sum(q => q.AreaLdi);
                    var areaVip = dictionaryAreasFail[dataType.Key.ToString()];

                    //  № п\п
                    workSheet.Cells[rowIndex + firstRowIndex, 0].Value = rowIndex + 1;
                    //Вид диагностических данных
                    workSheet.Cells[rowIndex + firstRowIndex, 4].Value = dataType.Key.ToString();
                    //  Суммарная площадь ПДИ, м2
                    workSheet.Cells[rowIndex + firstRowIndex, 6].Value = Math.Round(areaSum, 3);
                    //  Площадь ПДИ по причине отказа ВИП, м2
                    workSheet.Cells[rowIndex + firstRowIndex, 7].Value = Math.Round(areaVip, 3);
                    //  Площадь ПДИ по причинам ВГС\загрязнение\повреждение, м2
                    workSheet.Cells[rowIndex + firstRowIndex, 8].Value = Math.Round(areaSum - areaVip, 3);
                    rowIndex++;
                }
            }
            return rowIndex;
        }

        private void CreateCommonLinesSummaryTable(Calculation calculation, int rowIndex)
        {
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
        }

        private List<SpeedInfos> CreateSpeedInfos( ReferenceInputData dataOutput, DiagData calculationDiagData )
        {
            var result = new List<SpeedInfos>();

            var triggerChamberLocal = triggerChamber + dataOutput.DistRange.Begin;
            var receptionChamberLocal = dataOutput.DistRange.End - receptionChamber;

            foreach (var speedInfo in calculationDiagData.SpeedInfos)
            {
                //  1. (кпп)
                if (speedInfo.Distance.Begin <= triggerChamberLocal &&
                    speedInfo.Distance.End <= triggerChamberLocal ||
                    speedInfo.Distance.Begin >= receptionChamberLocal && 
                    speedInfo.Distance.End >= receptionChamberLocal)
                {
                    result.Add(SpeedInfosAreaTypeKPP(speedInfo.Distance.Begin, speedInfo.Distance.End, speedInfo));
                    continue;
                }

                //  2.
                if (speedInfo.Distance.Begin < triggerChamberLocal && 
                    speedInfo.Distance.End < receptionChamberLocal)
                {
                    // часть до (кпп)
                    result.Add(SpeedInfosAreaTypeKPP(speedInfo.Distance.Begin, triggerChamberLocal, speedInfo));
                    //часть после (лч)
                    result.Add(SpeedInfosAreaTypeLP(triggerChamberLocal, speedInfo.Distance.End, speedInfo));
                    continue;
                }

                //  3.
                if (speedInfo.Distance.Begin < triggerChamberLocal && 
                    speedInfo.Distance.End > receptionChamberLocal)
                {
                    // часть до (кпп)
                    result.Add(SpeedInfosAreaTypeKPP(speedInfo.Distance.Begin, triggerChamberLocal, speedInfo));
                    //середина (лч)
                    result.Add(SpeedInfosAreaTypeLP(triggerChamberLocal, receptionChamberLocal, speedInfo));
                    //часть после (кпп)
                    result.Add(SpeedInfosAreaTypeKPP(receptionChamberLocal, speedInfo.Distance.End, speedInfo));
                    continue;
                }

                //  4. (лч)
                if (speedInfo.Distance.Begin >= triggerChamberLocal && 
                    speedInfo.Distance.End <= receptionChamberLocal)
                {
                    result.Add(SpeedInfosAreaTypeLP(speedInfo.Distance.Begin, speedInfo.Distance.End, speedInfo));
                    continue;
                }

                //  5.
                if (speedInfo.Distance.Begin > triggerChamberLocal && 
                    speedInfo.Distance.End > receptionChamberLocal)
                {
                    // часть до (лч)
                    result.Add(SpeedInfosAreaTypeLP(speedInfo.Distance.Begin, receptionChamberLocal, speedInfo));
                    //часть после (кпп)
                    result.Add(SpeedInfosAreaTypeKPP(receptionChamberLocal, speedInfo.Distance.End, speedInfo));
                }
            }
            return result;
        }

        private SpeedInfos SpeedInfosAreaTypeKPP(double begin, double end, OverSpeedInfo speedInfo)
        {
            return new SpeedInfos
            {
                Begin = begin,
                End = end,
                Speed = speedInfo.Speed,
                Area = speedInfo.Area,
                AreaType = true
            };
        }

        private SpeedInfos SpeedInfosAreaTypeLP(double begin, double end, OverSpeedInfo speedInfo)
        {
            return new SpeedInfos
            {
                Begin = begin,
                End = end,
                Speed = speedInfo.Speed,
                Area = speedInfo.Area,
                AreaType = false
            };
        }

        private Dictionary<DiagDataTypeMain, DiagDataMain> GetAreaTypeSpeedInfo( ReferenceInputData dataOutput, List<DiagData> diagDataList)
        {
            var diagDataMain = new Dictionary<DiagDataTypeMain, DiagDataMain>();
            foreach (var diagData in diagDataList )
            {
                switch( diagData.DataType )
                {
                    case DataType.Spm:   if ( !diagDataMain.ContainsKey( DiagDataTypeMain.Spm ) )
                                            diagDataMain.Add( DiagDataTypeMain.Spm, new DiagDataMain(){PassportSpeedDiapason = diagData.PassportSpeedDiapason, NameTypeData = "SPM", SpeedInfos = new List<SpeedInfos>() } );
                                         diagDataMain[DiagDataTypeMain.Spm].SpeedInfos.AddRange( CreateSpeedInfos( dataOutput, diagData )  );
                                         foreach ( var overSpeedInfo in diagData.SpeedInfos )
                                             diagDataMain[DiagDataTypeMain.Spm].Area += overSpeedInfo.Area;
                                         break;
                    
                    case DataType.Mpm:   if ( !diagDataMain.ContainsKey( DiagDataTypeMain.Mpm ) )
                                            diagDataMain.Add( DiagDataTypeMain.Mpm, new DiagDataMain(){PassportSpeedDiapason = diagData.PassportSpeedDiapason, NameTypeData = "MPM", SpeedInfos = new List<SpeedInfos>() } );
                                         diagDataMain[DiagDataTypeMain.Mpm].SpeedInfos.AddRange( CreateSpeedInfos( dataOutput, diagData )  );
                                         foreach ( var overSpeedInfo in diagData.SpeedInfos )
                                             diagDataMain[DiagDataTypeMain.Mpm].Area += overSpeedInfo.Area;
                                         break;
                    
                    case DataType.Wm:    if ( !diagDataMain.ContainsKey( DiagDataTypeMain.Wm ) )
                                            diagDataMain.Add( DiagDataTypeMain.Wm, new DiagDataMain(){PassportSpeedDiapason = diagData.PassportSpeedDiapason, NameTypeData = "WM", SpeedInfos = new List<SpeedInfos>() } );
                                         diagDataMain[DiagDataTypeMain.Wm].SpeedInfos.AddRange( CreateSpeedInfos( dataOutput, diagData )  );
                                         foreach( var overSpeedInfo in diagData.SpeedInfos )
                                             diagDataMain[DiagDataTypeMain.Wm].Area += overSpeedInfo.Area;
                                         break;

                    case DataType.MflT1:
                    case DataType.MflT3:
                    case DataType.TfiT4: if ( !diagDataMain.ContainsKey( DiagDataTypeMain.Mfl ) )
                                            diagDataMain.Add( DiagDataTypeMain.Mfl, new DiagDataMain(){PassportSpeedDiapason = diagData.PassportSpeedDiapason, NameTypeData = "Mfl", SpeedInfos = new List<SpeedInfos>() } );
                                         diagDataMain[DiagDataTypeMain.Mfl].SpeedInfos.AddRange( CreateSpeedInfos( dataOutput, diagData )  );
                                         foreach( var overSpeedInfo in diagData.SpeedInfos )
                                             diagDataMain[DiagDataTypeMain.Mfl].Area += overSpeedInfo.Area;
                                         break;

                    case DataType.Cdl:
                    case DataType.Cdc:
                    case DataType.Cd360: if ( !diagDataMain.ContainsKey( DiagDataTypeMain.Cd ) )
                                            diagDataMain.Add( DiagDataTypeMain.Cd, new DiagDataMain(){PassportSpeedDiapason = diagData.PassportSpeedDiapason, NameTypeData = "CD", SpeedInfos = new List<SpeedInfos>() } );
                                         diagDataMain[DiagDataTypeMain.Cd].SpeedInfos.AddRange( CreateSpeedInfos( dataOutput, diagData )  );
                                         foreach( var overSpeedInfo in diagData.SpeedInfos )
                                             diagDataMain[DiagDataTypeMain.Cd].Area += overSpeedInfo.Area;
                                         break;
                }
            }
            return diagDataMain;
        }

        private void CreateTableBySpeed(Calculation calculation)
        {
            var workSheet = workBook.Worksheets[worksheetNumber];

            if ( calculation.Helper == null || calculation.Helper.Carriers == null && calculation.DiagDataList.Any( q => q.DataType != DataType.Mpm || q.DataType != DataType.Spm ) )

            {
                workSheet.Cells[0, 0].Value = "Идентификаторы носителей не определены!";
                workSheet.Cells.Merge(0, 0, 1, 10);
                TableDesign(worksheetNumber, 1, 3, 0, 0, DifferentFontStyle(12));
                return;
            }

            var speedInfos   = new List<SpeedInfos>();
            var areaKppSpeed = 0d;
            var countSheet   = 0; // количество листов
            var index        = 9; // отступ
            var rowCount     = 0; // обще число данных с speedInfo
            var rowDataIndex = 0; // количество заполненных строк

            foreach(var calculationDiagData in calculation.DiagDataList)
            {
                if (calculationDiagData.DataType == DataType.Nav || calculationDiagData.DataType == DataType.NavSetup) continue;
                speedInfos   = CreateSpeedInfos(calculation.DataOutput, calculationDiagData);
                if (speedInfos.Count == 0)
                    areaKppSpeed = 0;
                else
                    areaKppSpeed += calculation.DataOutput.OverallSpeedAreaLdi * speedInfos.Where(q => q.AreaType).Sum(q => q.End - q.Begin) / speedInfos.Sum(q => q.End - q.Begin);
                rowCount += speedInfos.Count;
            }

            countSheet = (int)Math.Ceiling((double)rowCount / MaxRowSheet );

            //  Шапка таблицы
            CreateTableHeaderCalculationPDI(calculation.DataOutput);
            workSheet.Cells[6, 4].Value = double.IsNaN(areaKppSpeed) ? 0 : Math.Round(areaKppSpeed, 3);
            workSheet.Cells[7, 4].Value = double.IsNaN(areaKppSpeed) ? 0 : Math.Round(calculation.DataOutput.OverallSpeedAreaLdi - areaKppSpeed, 3);
            workSheet.Cells[8, 4].Value = calculation.DataOutput.OverallSpeedAreaLdi;

            workSheet.Cells[2, 5].Value = "Диапазоны допустимых скоростей: ";


            var diagDataMain = GetAreaTypeSpeedInfo(calculation.DataOutput, calculation.DiagDataList);

            var row = 2;
            foreach (var iten in diagDataMain)
            {
                workSheet.Cells[row, 5].Value = $@"для ДД {iten.Value.NameTypeData}, м/с: ";
                workSheet.Cells[row, 6].Value = $@" 0,2 - {iten.Value.PassportSpeedDiapason.Begin} ";

                workSheet.Cells[row + 3, 5].Value = $@"Площадь ПДИ по ДД {iten.Value.NameTypeData}, м²: ";
                workSheet.Cells[row + 3, 6].Value = $@" {iten.Value.Area} ";

                row++;
            }

            workSheet.Cells[1, 5].Value = "6";
            workSheet.Cells[1, 6].Value = "7";
            workSheet.Cells.SetRowHeight(0, 40);
            workSheet.Cells.Merge(0, 1, 1, 6);
            TableDesign(worksheetNumber, 2, 6, 0, 1, DifferentFontBorderType(11));
            TableDesign(worksheetNumber, 7, 1, 2, 5, BaseDoubleCellBorderType(11));
            TableDesign(worksheetNumber, 7, 1, 2, 6, DifferentFontBorderType(11));

            if (speedInfos.Count == 0)
            {
                workSheet.Cells[9, 0].Value = "Потери не обнаружены";
                workSheet.Cells.Merge(9, 0, 1, 4);
                TableDesign(worksheetNumber, 1, 4, 9, 0, DifferentFontStyle());
                return;
            }

            for (var i = 0; i < countSheet; i++)
            {
                //  шапка столбцов
                workSheet.Cells[index - 1, 0].Value = "Дистанция начала ПДИ, м";
                workSheet.Cells[index - 1, 1].Value = "Дистанция конца ПДИ, м";
                workSheet.Cells[index - 1, 2].Value = "Фактическая скорость ВИП, м/с";
                workSheet.Cells[index - 1, 3].Value = "Площадь ПДИ, м\u00B2";
                workSheet.Cells[index - 1, 4].Value = "Тип участка";
                workSheet.Cells[index - 1, 5].Value = "Тип данных";

                workSheet.Cells[index - 2, 0].Value = "Потери данных по причине не соответствия скорости ВИП паспортному диапазону";
                workSheet.Cells.Merge(index - 2, 0, 1, 6);

                TableDesign(worksheetNumber, 1, 6, index - 2, 0, DifferentFontStyle(16));
                TableDesign(worksheetNumber, 1, 6, index - 1, 0, DifferentFontStyle(11));
                var j = 0; // количество заполненных строк на листе
                foreach( var iten in diagDataMain )
                {
                    foreach ( var speedInfo in iten.Value.SpeedInfos )
                    {
                            var begin = Math.Round( speedInfo.Begin, 3 );
                            var end   = Math.Round( speedInfo.End, 3 );
                            var area  = ( end - begin ) * Math.PI * calculation.DataOutput.Diameter / 1000;
                            workSheet.Cells[index + j, 0].Value = begin;
                            workSheet.Cells[index + j, 1].Value = end;
                            workSheet.Cells[index + j, 2].Value = Math.Round( speedInfo.Speed, 3 );
                            workSheet.Cells[index + j, 3].Value = Math.Round( area, 3 );
                            workSheet.Cells[index + j, 4].Value = speedInfo.AreaType ? "КПП СОД" : "Линейная часть";
                            workSheet.Cells[index + j, 5].Value = iten.Value.NameTypeData;
                            if ( rowDataIndex + 1 == rowCount )
                            {
                                var lastRow = rowDataIndex - MaxRowSheet * i + 1;
                                TableDesign(worksheetNumber, lastRow, 6, index);
                                return;
                            }

                        if( j > MaxRowSheet )
                            break; // при привышени колисчтва строк выходим из цылка

                        rowDataIndex++;
                        j++;

                    }
                    if ( j > MaxRowSheet )
                        break; // при привышени колисчтва строк выходим из цылка
                }

                TableDesign(worksheetNumber, MaxRowSheet, 6, index);

                workBook.Worksheets.Add($"ПДИ по скорости_{i + 1}");
                worksheetNumber++;
                workSheet = workBook.Worksheets[worksheetNumber];
                index = 2;
            }
        }

        private void CreateHeaderTableByFrames( ReferenceInputData referenceInputData )
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
            workSheet.Cells[firstRowIndex + 1, 3].Value = Math.Round( referenceInputData.DistRange.End - referenceInputData.DistRange.Begin);
            workSheet.Cells[firstRowIndex + 3, 2].Value = "Дефектоскоп №: ";
            workSheet.Cells[firstRowIndex + 3, 3].Value = referenceInputData.FlawDetector;
            workSheet.Cells[firstRowIndex + 4, 2].Value = "Ответственный за пропуск: ";
            workSheet.Cells[firstRowIndex + 4, 3].Value = referenceInputData.ResponsibleWorkItem;

            TableDesign(worksheetNumber, 5, 1, 0, 0, BaseStyle());
            TableDesign(worksheetNumber, 5, 1, 0, 1, DifferentFontStyle());
            TableDesign(worksheetNumber, 5, 1, 0, 2, BaseStyle());
            TableDesign(worksheetNumber, 5, 1, 0, 3, DifferentFontStyle());
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
            TableDesign(worksheetNumber, 1, 4, firstRowIndex, 0, DifferentFontStyle(16));

            workSheet.Cells[firstRowIndex + 1, 0].Value = tableHeader;
            workSheet.Cells.Merge(firstRowIndex + 1, 0, 1, 4);
            TableDesign(worksheetNumber, 1, 4, firstRowIndex + 1, 0, BaseStyle(11));

            if (calculation.Frames.Count != 0)
            {
                workSheet.Cells[firstRowIndex + 2, 0].Value = @"№ п/п";
                workSheet.Cells[firstRowIndex + 2, 1].Value = "Дистанция начала, м";
                workSheet.Cells[firstRowIndex + 2, 2].Value = "Дистанция конца, м";
                TableDesign(worksheetNumber, 1, 3, firstRowIndex + 2, 0, DifferentFontStyle(11));

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

                TableDesign(worksheetNumber, rowOffset - firstRowIndex - 3, 3, firstRowIndex + 3, 0, BaseStyle());
            }
            workSheet.AutoFitRows();
            workSheet.AutoFitColumns();
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
            workSheet.Cells[firstRowIndex, 8].Value = "Площадь ПДИ по причинам"+ "\n" + @"ВГС\загрязнение\повреждение," +  "м\u00B2";
            workSheet.Cells[firstRowIndex, 9].Value = "Площадь ПДИ по \nпричине несоответствия \nскорости, м\u00B2";

            for (var i = 0; i < 10; i++)
            {
                workSheet.Cells[firstRowIndex + 1, i].Value = i + 1;
            }

            TableDesign(worksheetNumber, 2, 10, firstRowIndex, 0, DifferentFontBorderType(11));
            workSheet.AutoFitRows();
            workSheet.AutoFitColumns();
        }

        private void CreateTableBySensors(string dataType, double areaLdi, List<RowDataAllTypes> rowData, bool doubleAngleFlag)
        {
            var startRow = 9;

            var shiftColumns = 0;
            var workSheet = workBook.Worksheets[worksheetNumber];
            var count = Math.Ceiling((double)rowData.Count / MaxRowSheet);  //  количество листов с ПДИ для одного типа данных
            var index = 11;
            var rowDataIndex = 0;

            workSheet.Cells[6, 4].Value = Math.Round(areaKpp, 3);
            workSheet.Cells[7, 4].Value = Math.Round(areaLdi - areaKpp, 3);
            workSheet.Cells[8, 4].Value = Math.Round(areaLdi, 3);

            if (rowData.Count == 0)
            {
                workSheet.Cells[startRow, 0].Value = "Потери не обнаружены";
                workSheet.Cells.Merge(startRow, 0, 1, 4);
                TableDesign(worksheetNumber, 1, 4, startRow, 0, DifferentFontStyle());
                return;
            }

            //var column = 0;
            //заполнение таблицы данными с учетом максимального значения ячеек
            for (var i = 0; i < count; i++)
            {
                workSheet.Cells[index - 1, 0].Value = "№ п/п";
                workSheet.Cells[index - 1, 1].Value = "№ датчика";
                if (doubleAngleFlag)
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
                for (var j = 0; j < 7 + shiftColumns; j++)
                    workSheet.Cells[index, j].Value = j + 1;

                workSheet.Cells[index - 2, 0].Value = $"ТИП ДАТЧИКОВ ({dataType.ToUpper()})";
                workSheet.Cells.Merge(index - 2, 0, 1, 7 + shiftColumns);

                TableDesign(worksheetNumber, 1, 7 + shiftColumns, index - 2, 0, DifferentFontBorderType(16));
                TableDesign(worksheetNumber, 2, 7 + shiftColumns, index - 1, 0, DifferentFontBorderType(11));
                index++;
                for (var j = 0; j < MaxRowSheet; j++)
                {
                    workSheet.Cells[index + j, 0].Value = j + 1;
                    workSheet.Cells[index + j, 1].Value = rowData[rowDataIndex].SensorNumber;
                    if (doubleAngleFlag)
                    {
                        workSheet.Cells[index + j, 2].Value = rowData[rowDataIndex].DirectionSensor;
                        workSheet.Cells[index + j, 3].Value = Math.Round(rowData[rowDataIndex].InputAngle, 1);
                        shiftColumns = 2;
                    }
                    workSheet.Cells[index + j, 2 + shiftColumns].Value = Math.Round(rowData[rowDataIndex].StartDistance, 3);
                    workSheet.Cells[index + j, 3 + shiftColumns].Value = Math.Round(rowData[rowDataIndex].EndDistance, 3);
                    workSheet.Cells[index + j, 4 + shiftColumns].Value = Math.Round(rowData[rowDataIndex].Area, 3);
                    workSheet.Cells[index + j, 5 + shiftColumns].Value = rowData[rowDataIndex].RefusePdiType ? "Отказ" : @"ВГС\загрязнение\повреждение";
                    workSheet.Cells[index + j, 6 + shiftColumns].Value = rowData[rowDataIndex].AreaType ? "КПП СОД" : "Линейная часть";

                    if (rowData.Count == rowDataIndex + 1)
                    {
                        var lastRow = rowDataIndex - MaxRowSheet * i + index + 1;
                        var totaiRow = lastRow - index;
                        TableDesign(worksheetNumber, totaiRow, 7 + shiftColumns, index);
                        return;
                    }
                    rowDataIndex++;
                }

                TableDesign(worksheetNumber, MaxRowSheet, 7 + shiftColumns, index);
                workBook.Worksheets.Add($"ПДИ по датчикам {dataType.ToUpper()}_{i + 1}");
                worksheetNumber++;
                workSheet = workBook.Worksheets[worksheetNumber];
                index = 2;
            }
        }

        private List<RowDataAllTypes> FillFieldsTableByPdi(DiagData diagData, List<CdmDiagData> cdmDiagData, out bool doubleAngle)
        {
            var rowData = new List<RowDataAllTypes>();
            var doubleAngleFlag = false;
            if (diagData != null)
            {
                //  все кроме сд360
                rowData = RowDataAllTypes(diagData, false);
            }
            if (cdmDiagData != null)
            {
                //  сд360
                rowData = RowDataOnlyCd360(cdmDiagData, out doubleAngleFlag);
            }
            doubleAngle = doubleAngleFlag;
            //  сортировка по "отказам"
            return (from q in rowData orderby q.RefusePdiType descending select q).ToList();
        }

        private List<RowDataAllTypes> RowDataAllTypes(DiagData diagData, bool doubleAngleFlag)
        {
            var rowDataAllTypes = new List<RowDataAllTypes>();
            var triggerChamberLocal = triggerChamber + diagData.StartDist;
            var receptionChamberLocal = diagData.EndDist - receptionChamber;

            foreach (var haltingSensor in diagData.HaltingSensors)
            {
                foreach (var distRange in haltingSensor.Value)
                {
                    //  1. (кпп)
                    if (distRange.Begin <= triggerChamberLocal && distRange.End <= triggerChamberLocal ||
                        distRange.Begin >= receptionChamberLocal && distRange.End >= receptionChamberLocal)
                    {
                        var area = distRange.Area;
                        var rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = distRange.Begin,
                            EndDistance = distRange.End,
                            AreaType = true,
                            Area = area
                        };
                        CommonParams(rowData, diagData, distRange, doubleAngleFlag);
                        if (rowData.RefusePdiType) areaFail = areaFail + distRange.Area;
                        rowDataAllTypes.Add(rowData);
                        areaKpp = areaKpp + area;
                        continue;
                    }
                    //  2.
                    if (distRange.Begin < triggerChamberLocal && distRange.End < receptionChamberLocal)
                    {
                        // часть до (кпп)
                        var area = distRange.Area * (triggerChamberLocal - distRange.Begin) /
                               (distRange.End - distRange.Begin);
                        var rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = distRange.Begin,
                            EndDistance = triggerChamberLocal,
                            AreaType = true,
                            Area = area
                        };
                        CommonParams(rowData, diagData, distRange, doubleAngleFlag);
                        rowDataAllTypes.Add(rowData);
                        areaKpp = areaKpp + area;
                        //часть после (лч)
                        area = distRange.Area * (distRange.End - triggerChamberLocal) / 
                               (distRange.End - distRange.Begin);
                        rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = triggerChamberLocal,
                            EndDistance = distRange.End,
                            AreaType = false,
                            Area = area
                        };
                        CommonParams(rowData, diagData, distRange, doubleAngleFlag);
                        rowDataAllTypes.Add(rowData);
                        if (rowData.RefusePdiType) areaFail = areaFail + distRange.Area;
                        continue;
                    }
                    //  3.
                    if (distRange.Begin < triggerChamberLocal && distRange.End > receptionChamberLocal)
                    {
                        // часть до (кпп)
                        var area = distRange.Area * (triggerChamberLocal - distRange.Begin) / (distRange.End - distRange.Begin);
                        var rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = distRange.Begin,
                            EndDistance = triggerChamberLocal,
                            AreaType = true,
                            Area = area
                        };
                        CommonParams(rowData, diagData, distRange, doubleAngleFlag);
                        rowDataAllTypes.Add(rowData);
                        areaKpp = areaKpp + area;
                        //середина (лч)
                        area = distRange.Area * (receptionChamberLocal - triggerChamberLocal) / (distRange.End - distRange.Begin);
                        rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = triggerChamberLocal,
                            EndDistance = receptionChamberLocal,
                            AreaType = false,
                            Area = area
                        };
                        CommonParams(rowData, diagData, distRange, doubleAngleFlag);
                        rowDataAllTypes.Add(rowData);
                        //часть после (кпп)
                        area = distRange.Area * (distRange.End - receptionChamberLocal) / (distRange.End - distRange.Begin);
                        rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = receptionChamberLocal,
                            EndDistance = distRange.End,
                            AreaType = true,
                            Area = area
                        };
                        CommonParams(rowData, diagData, distRange, doubleAngleFlag);
                        if (rowData.RefusePdiType) areaFail = areaFail + distRange.Area;
                        rowDataAllTypes.Add(rowData);
                        areaKpp = areaKpp + area;
                        continue;
                    }
                    //  4. (лч)
                    if (distRange.Begin >= triggerChamberLocal && distRange.End <= receptionChamberLocal)
                    {
                        var area = distRange.Area;
                        var rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = distRange.Begin,
                            EndDistance = distRange.End,
                            AreaType = false,
                            Area = area
                        };
                        CommonParams(rowData, diagData, distRange, doubleAngleFlag);
                        if (rowData.RefusePdiType) areaFail = areaFail + distRange.Area;
                        rowDataAllTypes.Add(rowData);
                        continue;
                    }
                    //  5.
                    if (distRange.Begin > triggerChamberLocal && distRange.End > receptionChamberLocal)
                    {
                        // часть до (лч)
                        var area = distRange.Area * (receptionChamberLocal - distRange.Begin) / (distRange.End - distRange.Begin);
                        var rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = distRange.Begin,
                            EndDistance = receptionChamberLocal,
                            AreaType = false,
                            Area = area
                        };
                        CommonParams(rowData, diagData, distRange, doubleAngleFlag);
                        rowDataAllTypes.Add(rowData);
                        //часть после (кпп)
                        area = distRange.Area * (distRange.End - receptionChamberLocal) / (distRange.End - distRange.Begin);
                        rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = receptionChamberLocal,
                            EndDistance = distRange.End,
                            AreaType = true,
                            Area = area
                        };
                        CommonParams(rowData, diagData, distRange, doubleAngleFlag);
                        if (rowData.RefusePdiType) areaFail = areaFail + distRange.Area;
                        rowDataAllTypes.Add(rowData);
                        areaKpp = areaKpp + area;
                    }
                }
            }
            return rowDataAllTypes;
        }

        private void CommonParams(RowDataAllTypes rowData, DiagData diagData, ModelDB.Range<double> distRange, bool doubleAngleFlag)
        {
            var percentPdi = (distRange.End - distRange.Begin) / diagData.DistanceLength;
            const double maxPercentPdi = 0.95;
            if (doubleAngleFlag)
            {
                rowData.DirectionSensor = ((CdmDiagData)diagData).Angle.ToString();
                rowData.InputAngle = ((CdmDiagData)diagData).EntryAngle;
            }

            if (percentPdi >= maxPercentPdi)
                rowData.RefusePdiType = true;
        }

        private List<RowDataAllTypes> RowDataOnlyCd360(List<CdmDiagData> cdmDiagData, out bool doubleAngleFlag)
        {
            doubleAngleFlag = false;
            var rowDataAllTypes = new List<RowDataAllTypes>();

            foreach (var diagData in cdmDiagData)
            {
                if (!MathHelper.TestFloatEquals(diagData.Angle, diagData.Id))
                {
                    //  двойные углы сд360
                    doubleAngleFlag = true;
                    rowDataAllTypes.AddRange(RowDataAllTypes(diagData, true));
                }
                else
                {
                    //  сд360 без двойных углов
                    rowDataAllTypes.AddRange(RowDataAllTypes(diagData, false));
                }
            }
            return rowDataAllTypes;
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

        private Style Boldface(int size = 10)
        {
            var style = new Style();
            style.Font.Name = "Franklin Gothic Book";
            style.Font.Size = size;
            style.VerticalAlignment = TextAlignmentType.Center;
            style.HorizontalAlignment = TextAlignmentType.Left;
            style.Font.IsBold = true;
            style.IsTextWrapped = true;
            return style;
        }

        private Style SimpleStyle(int size = 10)
        {
            var style = BaseStyle(size);
            style.VerticalAlignment = TextAlignmentType.Left;
            style.HorizontalAlignment = TextAlignmentType.Left;
            return style;
        }

        private Style DifferentFontStyle(int size = 10)
        {
            var style = BaseStyle(size);
            style.Font.IsBold = true;
            style.IsTextWrapped = true;
            return style;
        }

        private Style BaseDoubleCellBorderType(int size = 10)
        {
            var style = BaseStyle(size);
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Double;
            style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Double;
            style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Double;
            style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Double;
            return style;
        }

        private Style DifferentFontBorderType(int size = 10)
        {
            var style = DifferentFontStyle(size);
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Double;
            style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Double;
            style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Double;
            style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Double;
            return style;
        }

        private void TableDesign(int worksheetNumber, int totalRow, int totalColumn, int firstRow = 0, int firstColumn = 0, Style style = null)
        {
            if (style == null)
                style = BaseStyle();
            workBook.Worksheets[worksheetNumber].AutoFitColumns(0, totalColumn);
            var range = workBook.Worksheets[worksheetNumber].Cells.CreateRange(firstRow, firstColumn, totalRow, totalColumn);
            range.ApplyStyle(style, new StyleFlag { All = true });
        }
        
        //////////////////////////////////////////////////////////////////////////
        //  Методы для навигационных данных
        //////////////////////////////////////////////////////////////////////////
        
        private void CreateHeaderNavTable()
        {
            var workSheet = workBook.Worksheets[worksheetNumber];
            
            workSheet.Cells[0, 0].Value = "Этапы \nконтроля";
            workSheet.Cells[0, 1].Value = "Период фиксирования значения \nданных для расчёта";
            workSheet.Cells[0, 2].Value = "Расчитаные значения данных";
            workSheet.Cells[0, 3].Value = "Контроль значений данных";
            workSheet.Cells[0, 4].Value = "Расшифровка";
            workSheet.Cells[0, 5].Value = "Примечание";

            TableDesign(worksheetNumber, 1, 6, 0, 0, DifferentFontStyle(11));
        }
        
        private void CreateBodyNavTable(Calculation calculation)
        {
            var rowOffset = 1;

            switch (calculation.NavigationInfo.NavType)
            {
                case enNavType.Adis:
                    CreateBodyNavAdisTable(calculation, ref rowOffset);
                    break;
                case enNavType.Bep:
                    CreateBodyNavBepTable(calculation, ref rowOffset);
                    break;
                case enNavType.Bins:
                    CreateBodyNavBinsTable(calculation, ref rowOffset);
                    break;
            }

            var workSheet = workBook.Worksheets[worksheetNumber];

            TableDesign(worksheetNumber, rowOffset + 1, 6, 0, 0, DifferentFontStyle());
            TableDesign(worksheetNumber, rowOffset, 4, 1, 1, SimpleStyle());

            workSheet.AutoFitRows();
            workSheet.AutoFitColumns();
        }

        private void CreateBodyNavAdisTable(Calculation calculation, ref int rowOffset)
        {
            var workSheet = workBook.Worksheets[worksheetNumber];
            var navCalcParams = ConstNavCalcParams.GetAllParams().FirstOrDefault(item => item.NavType == calculation.NavigationInfo.NavType);
            var navInfo = calculation.NavigationInfo;

            if (navCalcParams == null) return;
            
            CreateBodyNavSquareDeviation(workSheet, ref rowOffset, navInfo, navCalcParams);
            rowOffset++;

            CreateBodyNavTableItems(workSheet, ref rowOffset, navInfo, navCalcParams);
        }

        private void CreateBodyNavBepTable(Calculation calculation, ref int rowOffset)
        {
            var workSheet = workBook.Worksheets[worksheetNumber];
            var navCalcParams = ConstNavCalcParams.GetAllParams().FirstOrDefault(item => item.NavType == calculation.NavigationInfo.NavType);
            var navInfo = calculation.NavigationInfo;

            rowOffset = 1;

            if (navCalcParams == null) return;
            
            CreateBodyNavSquareDeviation(workSheet, ref rowOffset, navInfo, navCalcParams);
            rowOffset++;

            CreateBodyNavTableItems(workSheet, ref rowOffset, navInfo, navCalcParams);
        }

        private void CreateBodyNavBinsTable(Calculation calculation, ref int rowOffset)
        {
            var workSheet = workBook.Worksheets[worksheetNumber];
            var navCalcParams = ConstNavCalcParams.GetAllParams().FirstOrDefault(item => item.NavType == calculation.NavigationInfo.NavType);
            var navInfo = calculation.NavigationInfo;

            if (navCalcParams == null) return;
            
            CreateBodyNavSquareDeviation(workSheet, ref rowOffset, navInfo, navCalcParams);
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 2;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavLastMinuteSetupPhase;
            GravAccel(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.GravitationalAcceleration, 3));
            GravAccel(workSheet.Cells[rowOffset, 3], navCalcParams.GravAccel);
            GravAccelDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.GravAccel)
                ? Resources.NormalAccelerometer
                : Resources.WrongAccelerometer;
            rowOffset++;
            
            workSheet.Cells[rowOffset, 0].Value = 3;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavLastMinuteSetupPhase;
            EarthAngularSpeedRotation(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.EarthAngularSpeedRotation, 3));
            EarthAngularSpeedRotation(workSheet.Cells[rowOffset, 3], navCalcParams.EarthAngularSpeedRotation);
            EarthAngularSpeedRotationDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value =
                navInfo.NavCalcState.HasFlag(NavCalcStateTypes.EarthAngularSpeedRotation)
                    ? Resources.NormalGyroscope
                    : Resources.WrongGyroscope;
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 4;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavLastMinuteSetupPhase;
            DiffLatitude(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.DifferenceLatitudes, 3));
            DiffLatitude(workSheet.Cells[rowOffset, 3], navCalcParams.DifferenceLatitudes);
            DiffLatitudeDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.DiffLatitude)
                ? Resources.NormalGyroscopeAndOrAccelerometer
                : Resources.WrongGyroscopeAndOrAccelerometer;
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 5;
            workSheet.Cells.Merge(rowOffset, 0, 2, 1);
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            workSheet.Cells.Merge(rowOffset, 1, 2, 1); 
            AccelSum(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AccelSum, 3));
            AccelSum(workSheet.Cells[rowOffset, 3], navCalcParams.AccelSum);
            AccelSumDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.AccelSum)
                ? Resources.NormalAccelerometer
                : Resources.WrongAccelerometer; 
            rowOffset++;

            AccelMax(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AccelMax, 3));
            AccelMax(workSheet.Cells[rowOffset, 3], navCalcParams.AccelMax);
            AccelMaxDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.AccelMax)
                ? Resources.NormalAccelerometer
                : Resources.WrongAccelerometer;
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 6;
            workSheet.Cells.Merge(rowOffset, 0, 2, 1);
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            workSheet.Cells.Merge(rowOffset, 1, 2, 1);
            AngularSpeedSum(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AngularSpeedSum, 3));
            AngularSpeedSum(workSheet.Cells[rowOffset, 3], navCalcParams.AngularSpeedSum);
            AngularSpeedSumDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.AngularSpeedSum)
                ? Resources.NormalGyroscope
                : Resources.WrongGyroscope;
            rowOffset++;

            AngularSpeedMax(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AngularSpeedMax, 3));
            AngularSpeedMax(workSheet.Cells[rowOffset, 3], navCalcParams.AngularSpeedMax);
            AngularSpeedMaxDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.AngularSpeedMax)
                ? Resources.NormalGyroscope
                : Resources.WrongGyroscope;
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 7;
            workSheet.Cells.Merge(rowOffset, 0, 4, 1);
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            workSheet.Cells.Merge(rowOffset, 1, 4, 1); 
            AverageRollAngle(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AverageRollAngle, 3));
            AverageRollAngle(workSheet.Cells[rowOffset, 3], navCalcParams.AverageRollAngle);
            AverageRollAngleDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.AverageRollPitchAngle)
                ? Resources.NormalGyroscopeOrAccelerometer
                : Resources.WrongGyroscopeOrAccelerometer;
            workSheet.Cells.Merge(rowOffset, 5, 2, 1);
            rowOffset++;
            
            AveragePitchAngle(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AveragePitchAngle, 3));
            AveragePitchAngle(workSheet.Cells[rowOffset, 3], navCalcParams.AveragePitchAngle);
            AveragePitchAngleDesc(workSheet.Cells[rowOffset, 4]);
            rowOffset++;
            
            MaxRollAngle(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.MaxRollAngle, 3));
            MaxRollAngle(workSheet.Cells[rowOffset, 3], navCalcParams.MaxRollAngle);
            MaxRollAngleDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.MaxRollPitchAngle)
                ? Resources.NormalGyroscopeOrAccelerometer
                : Resources.WrongGyroscopeOrAccelerometer;
            workSheet.Cells.Merge(rowOffset, 5, 2, 1);
            rowOffset++;

            MaxPitchAngle(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.MaxPitchAngle, 3));
            MaxPitchAngle(workSheet.Cells[rowOffset, 3], navCalcParams.MaxPitchAngle);
            MaxPitchAngleDesc(workSheet.Cells[rowOffset, 4]);
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 8;
            workSheet.Cells.Merge(rowOffset, 0, 4, 1);
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            PitchAngleAtMovement(workSheet.Cells[rowOffset, 2], navInfo.PitchAngleAtMovement);
            PitchAngleAtMovement(workSheet.Cells[rowOffset, 3], navCalcParams.PitchAngleAtMovement ); // угол тангажа > 4 градусов
            PitchAngleAtMovementDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.PitchAngleAtMovement) ? GasEnvironment : string.Empty;
            workSheet.Cells.Merge(rowOffset, 5, 4, 1);
            rowOffset++;

            PitchAngleSectionPhaseDesc(workSheet.Cells[rowOffset, 1], 6);
            PitchAngleSection(workSheet.Cells[rowOffset, 2], 6, navInfo.PitchAngleSection6);
            PitchAngleSectionDesc(workSheet.Cells[rowOffset, 4], 6);
            rowOffset++;

            PitchAngleSectionPhaseDesc(workSheet.Cells[rowOffset, 1], 3);
            PitchAngleSection(workSheet.Cells[rowOffset, 2], 3, navInfo.PitchAngleSection3);
            PitchAngleSectionDesc(workSheet.Cells[rowOffset, 4], 3);
            rowOffset++;

            PitchAngleSectionPhaseDesc(workSheet.Cells[rowOffset, 1], 2);
            PitchAngleSection(workSheet.Cells[rowOffset, 2], 2, navInfo.PitchAngleSection2);
            PitchAngleSectionDesc(workSheet.Cells[rowOffset, 4], 2);
        }

        private void CreateBodyNavSquareDeviation(Worksheet workSheet, ref int rowOffset, NavigationInfo navInfo, NavCalcParams navCalcParams)
        {
            workSheet.Cells[rowOffset, 0].Value = 1;
            workSheet.Cells.Merge(rowOffset, 0, 7, 1);
            workSheet.Cells[rowOffset, 1].Value = Resources.SetupPhaseLast600Sec;
            workSheet.Cells.Merge(rowOffset, 1, 7, 1);
            LinearSpeed(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.LinearSpeed, 3));
            LinearSpeed(workSheet.Cells[rowOffset, 3], navCalcParams.LinearSpeed);
            AccelAngularSpeedDesc(workSheet.Cells[rowOffset, 4], navInfo.NavType);
            workSheet.Cells.Merge(rowOffset, 4, 7, 1);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.StandardDeviations)
                ? Resources.NormalGyroscopeAndOrAccelerometer
                : Resources.WrongGyroscopeAndOrAccelerometer;
            if(navInfo.NavCalcState.HasFlag(NavCalcStateTypes.WrongNavSetupTime))
                workSheet.Cells[rowOffset, 5].Value = Resources.WrongGyroscopeAndOrAccelerometer + " (Δt<600c)";
            workSheet.Cells.Merge(rowOffset, 5, 7, 1);
            rowOffset++;

            SquareDeviationAccelX(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.SquareDeviationAccelX, 3));
            SquareDeviationAccelX(workSheet.Cells[rowOffset, 3], navCalcParams.SquareDeviationAccelX);
            rowOffset++;

            SquareDeviationAccelY(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.SquareDeviationAccelY, 3));
            SquareDeviationAccelY(workSheet.Cells[rowOffset, 3], navCalcParams.SquareDeviationAccelY);
            rowOffset++;

            SquareDeviationAccelZ(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.SquareDeviationAccelZ, 3));
            SquareDeviationAccelZ(workSheet.Cells[rowOffset, 3], navCalcParams.SquareDeviationAccelZ);
            rowOffset++;

            SquareDeviationAngularSpeedZ(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.SquareDeviationAngularSpeedZ, 3));
            SquareDeviationAngularSpeedZ(workSheet.Cells[rowOffset, 3], navCalcParams.SquareDeviationAngularSpeedZ);
            rowOffset++;

            SquareDeviationAngularSpeedY(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.SquareDeviationAngularSpeedY, 3));
            SquareDeviationAngularSpeedY(workSheet.Cells[rowOffset, 3], navCalcParams.SquareDeviationAngularSpeedY);
            rowOffset++;

            if (navCalcParams.NavType == enNavType.Bep) return;

            SquareDeviationAngularSpeedX(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.SquareDeviationAngularSpeedX, 3));
            SquareDeviationAngularSpeedX(workSheet.Cells[rowOffset, 3], navCalcParams.SquareDeviationAngularSpeedX);
        }

        private void CreateBodyNavTableItems(Worksheet workSheet, ref int rowOffset, NavigationInfo navInfo, NavCalcParams navCalcParams)
        {
            workSheet.Cells[rowOffset, 0].Value = 2;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavSetupPhase;
            GravAccel(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.GravitationalAcceleration, 3));
            GravAccel(workSheet.Cells[rowOffset, 3], navCalcParams.GravAccel);
            GravAccelDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.GravAccel)
                ? Resources.NormalAccelerometer
                : Resources.WrongAccelerometer;
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 3;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavSetupPhase;
            EarthAngularSpeedRotation(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.EarthAngularSpeedRotation, 3));
            EarthAngularSpeedRotation(workSheet.Cells[rowOffset, 3], navCalcParams.EarthAngularSpeedRotation, false);
            EarthAngularSpeedRotationDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.EarthAngularSpeedRotation)
                    ? Resources.NormalGyroscope
                    : Resources.WrongGyroscope;
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 4;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            AccelSum(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AccelSum, 3));
            AccelSum(workSheet.Cells[rowOffset, 3], navCalcParams.AccelSum);
            AccelSumDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.AccelSum)
                ? Resources.NormalAccelerometer
                : Resources.WrongAccelerometer;
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 5;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            AccelMax(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AccelMax, 3));
            AccelMax(workSheet.Cells[rowOffset, 3], navCalcParams.AccelMax);
            AccelMaxDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.AccelMax)
                ? Resources.NormalAccelerometer
                : Resources.WrongAccelerometer;
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 6;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            AngularSpeedSum(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AngularSpeedSum, 3));
            AngularSpeedSum(workSheet.Cells[rowOffset, 3], navCalcParams.AngularSpeedSum);
            AngularSpeedSumDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.AngularSpeedSum)
                ? Resources.NormalGyroscope
                : Resources.WrongGyroscope;
            rowOffset++;

            workSheet.Cells[rowOffset, 0].Value = 7;
            workSheet.Cells[rowOffset, 1].Value = Resources.NavWorkPhase;
            AngularSpeedMax(workSheet.Cells[rowOffset, 2], Math.Round(navInfo.AngularSpeedMax, 3));
            AngularSpeedMax(workSheet.Cells[rowOffset, 3], navCalcParams.AngularSpeedMax);
            AngularSpeedMaxDesc(workSheet.Cells[rowOffset, 4]);
            workSheet.Cells[rowOffset, 5].Value = navInfo.NavCalcState.HasFlag(NavCalcStateTypes.AngularSpeedMax)
                ? Resources.NormalGyroscope
                : Resources.WrongGyroscope;
        }

        private const string CriterionDone = "Критерий выполнен";
        private const string CriterionNotDone = "Критерий не выполнен";
        private const string AdvancedAnalysis = "НД не могут быть обработаны штатным образом, необходим расширенный анализ НД";
        private const string NotGasEnvironment = "ВИП двигался не в ГС";
        private const string GasEnvironment = "Данный режим движения возможен в случае пропуска ВИП по"+
                                              "\nгазу, газо-воздушной и богатой газом среде, обладающей"+
                                              "\nзначительной сжимаемостью (ГС) Кроме того, выполнение данных"+
                                              "\nкритериев может происходить  на особенностях  трубопровода"+
                                              "\nтипа косой стык, тройник, вантуз, камера пуска или приема, даже"+
                                              "\nесли ВИП не двигался в ГС";

        private void LinearSpeed<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"vj={param.Value}+{param.Threshold} м/с" : $"vj= {item}";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        private void SquareDeviationAccelX<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"S[axj]<{param.Value} м/с²" : $"S[axj]= {item}";
            cell.Characters(3, 2).Font.IsSubscript = true;
        }
        private void SquareDeviationAccelY<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"S[ayj]<{param.Value} м/с²" : $"S[ayj]= {item}";
            cell.Characters(3, 2).Font.IsSubscript = true;
        }
        private void SquareDeviationAccelZ<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"S[azj]<{param.Value} м/с²" : $"S[azj]= {item}";
            cell.Characters(3, 2).Font.IsSubscript = true;
        }
        private void SquareDeviationAngularSpeedX<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"S[ωxj]<{param.Value} \u00B0/с" : $"S[ωxj]= {item}";
            cell.Characters(3, 2).Font.IsSubscript = true;
        }
        private void SquareDeviationAngularSpeedY<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"S[ωyj]<{param.Value} \u00B0/с" : $"S[ωyj]= {item}";
            cell.Characters(3, 2).Font.IsSubscript = true;
        }
        private void SquareDeviationAngularSpeedZ<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"S[ωzj]<{param.Value} \u00B0/с" : $"S[ωzj]= {item}";
            cell.Characters(3, 2).Font.IsSubscript = true;
        }

        private void GravAccel<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"g={param.Value}±{param.Threshold} м/с²" : $"g= {item}";
        }
        private void EarthAngularSpeedRotation<T>(Cell cell, T item, bool isEqualOperation = true) // <-
        {
            cell.Value = item is NavCalcParamItem param ? 
                $"ωₒ= {param.Value}±{param.Threshold} \u00B0/с" : 
                isEqualOperation ? 
                    $"ωₒ= {item}" : 
                    $"ωₒ< {item}";
        }
        private void DiffLatitude<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"|ψ-ψₒ|<{param.Value}\u00B0" : $"|ψ-ψₒ|= {item}";
        }
        private void AccelSum<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"aс={param.Value}±{param.Threshold} м/с²" : $"aс= {item}";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        private void AccelMax<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"am < {param.Value} м/с²" : $"am= {item}";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        private void AngularSpeedSum<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"ωс < {param.Value} \u00B0/с" : $"ωс= {item}";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        private void AngularSpeedMax<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"ωm < {param.Value} \u00B0/с" : $"ωm= {item}";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        private void AverageRollAngle<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"γс < {param.Value}\u00B0" : $"γс= {item}";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        private void AveragePitchAngle<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"Θс < {param.Value}\u00B0" : $"Θс= {item}";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        private void MaxRollAngle<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"γm < {param.Value}\u00B0" : $"γm= {item}";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        private void MaxPitchAngle<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"Θm < {param.Value}\u00B0" : $"Θm= {item}";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        private void PitchAngleAtMovement<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"ΔΘj > {param.Value}\u00B0" : $"ΔΘj = {item}"; // угол тангажа > 4 градусов
            cell.Characters(2, 1).Font.IsSubscript = true;
        }
        private void PitchAngleSection(Cell cell, int sectionLen, int value)
        {
            cell.Value = $"C{sectionLen} = {value}";
        }
        private void AverageRollAngleDesc(Cell cell)
        {
            cell.Value = "где γс - среднее значение расхождения по углу крена в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        private void AveragePitchAngleDesc(Cell cell)
        {
            cell.Value = "где Θс - среднее значение расхождения по углу тангажа в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        private void MaxRollAngleDesc(Cell cell)
        {
            cell.Value = "где γс - максимальное значение расхождения по углу крена в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        private void MaxPitchAngleDesc(Cell cell)
        {
            cell.Value = "где Θс - максимальное значение расхождения по углу тангажа в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        private void PitchAngleAtMovementDesc(Cell cell)
        {
            cell.Value = "где ΔΘ - колличество расхождений по углу тангажа, > 4° градусов в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }

        private void PitchAngleSectionPhaseDesc(Cell cell, int sectionLen)
        {
            cell.Value = $"наличие непрерывных участков данных протяженностью от {sectionLen} м";
        }

        private void AccelAngularSpeedDesc(Cell cell, enNavType navType)
        {
            if (navType == enNavType.Bep)
            {
                cell.Value = "где vj - линейная скорость ВИП" +
                             "\nj - элементы НД, используемые при обработке(относящиеся к участку выставки)" +
                             "\nωj – угловая скорость вращения ВИП" +
                             "\nΔωj - изменение угла гироскопа" +
                             "\naj – проекция линейного ускорения" +
                             "\nS{∙}-операция вычисления среднеквадратичного отклонения";
                cell.Characters(5, 1).Font.IsSubscript = true;
                cell.Characters(108, 1).Font.IsSubscript = true;
                cell.Characters(144, 1).Font.IsSubscript = true;
                cell.Characters(174, 1).Font.IsSubscript = true;
            }
            else
            {
                cell.Value = "где vj - линейная скорость ВИП" +
                             "\nj - элементы НД, используемые при обработке(относящиеся к участку выставки)" +
                             "\nωj = 200Δωj / 3600 – угловая скорость" +
                             "\nΔωj - изменение угла гироскопа" +
                             "\naj – проекция линейного ускорения" +
                             "\nS{∙}-операция вычисления среднеквадратичного отклонения";
                cell.Characters(5, 1).Font.IsSubscript = true;
                cell.Characters(108, 1).Font.IsSubscript = true;
                cell.Characters(147, 1).Font.IsSubscript = true;
                cell.Characters(177, 1).Font.IsSubscript = true;
            }
        }
        private void GravAccelDesc(Cell cell)
        {
            cell.Value = @"где g - значение ускорения свободного падения на участке выставки в течение последней минуты перед началом движения";
        }
        private void EarthAngularSpeedRotationDesc(Cell cell)
        {
            cell.Value = @"где ωₒ - значения угловой скорости вращения Земли на участке выставки в течение последней минуты перед началом движения";
        }
        private void DiffLatitudeDesc(Cell cell)
        {
            cell.Value = @"где ψ -значения широты на участке выставки в течение последней минуты перед началом движения, ψₒ - истинная широта камеры пуска ВИП";
        }
        private void AccelSumDesc(Cell cell)
        {
            cell.Value = "где aс - значение обобщенного ускорения в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        private void AccelMaxDesc(Cell cell)
        {
            cell.Value = "где am - значение обобщенного ускорения в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        private void AngularSpeedSumDesc(Cell cell)
        {
            cell.Value = "где ωс - значение обобщенной угловой скорости в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        private void AngularSpeedMaxDesc(Cell cell)
        {
            cell.Value = "где ωm -  значение обобщенной угловой скорости в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        private void PitchAngleSectionDesc(Cell cell, int sectionLen)
        {
            cell.Value = $"С{sectionLen} - количество непрерывных участков данных протяженностью от {sectionLen} м";
        }
    }

    [Flags]
    public enum DiagDataTypeMain
    {
        None = 0,
        Spm = 1,
        Mpm = 2,
        Wm = 4,
        Mfl = 8,
        Cd = 16384,
    }

    public class DiagDataMain
    {
        public bool AreaType { get; set; }
        public double Area { get; set; }
        public string NameTypeData { get; set; }
        public List<SpeedInfos> SpeedInfos { get; set; }
        public ModelDB.Range<double> PassportSpeedDiapason { get; set; } // Паспортный диапазон скорости ВИП
    }

}
