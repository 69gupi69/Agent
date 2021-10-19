using System;
using System.Collections.Generic;
using System.Linq;
using Diascan.Agent.TaskManager;
using Diascan.Agent.Types;
using Diascan.Agent.Types.ModelCalculationDiagData;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.Types;

namespace Diascan.Agent.AnalysisManager
{
    public class AnalysisManager
    {
        private const int DistanceChecking = 1000;

        private double areaFail;
        private double areaKpp;
        private double receptionChamber;
        private double triggerChamber;

        private readonly Action<Session> updateAction;
        private readonly Action<object> loggerInfoAction;

        public AnalysisManager(Action<Session> updateAction, Action<object> loggerInfoAction)
        {
            this.updateAction = updateAction;
            this.loggerInfoAction = loggerInfoAction;
        }
        public AnalysisManager(Action<object> loggerInfoAction)
        {
            this.loggerInfoAction = loggerInfoAction;
        }

        private AnalysisType GetAnalysisType(List<IDiagData> items, double diameter,DataType dataType, enCdmDirectionName dataTypeKey, double sumAreaLdi)
        {
            var sensorsСontiguousAll = new Dictionary<int, List<RowDataAllTypes>>();
            var areaContiguousAll = 0.0d;
            foreach (var item in items)
            {
                areaContiguousAll += CalcAreasСontiguous((DiagData)item, diameter, out var sensorsСontiguous);

                foreach (var sensor in sensorsСontiguous)
                    if (sensorsСontiguousAll.ContainsKey(sensor.Key))
                        sensorsСontiguousAll[sensor.Key] = sensorsСontiguousAll[sensor.Key].Union(sensor.Value).ToList();
                    else
                        sensorsСontiguousAll[sensor.Key] = sensor.Value;
            }
            //  считает тут каждый тип сд360
            var rowDataSorted = FillFieldsTableByPdi(null, items, out var doubleAngle);

            return new AnalysisType()
            {
                AreaFail           = areaFail,
                AreaKpp            = areaKpp,
                NumberSensorsBlock = 0,
                CdmType            = dataTypeKey,
                DataType           = dataType,
                AreaLdi            = sumAreaLdi,
                AreaContiguous     = areaContiguousAll,
                SensorsСontiguous  = sensorsСontiguousAll,
                RowDataSorted      = rowDataSorted.ToList(),
                DoubleAngle        = doubleAngle
            };
        }

        public Dictionary<Calculation, AnalysisCalculation> DoAnalysis(Session session)
        {
            var dictionaryAnalysisCalculation = new Dictionary<Calculation, AnalysisCalculation>();
            foreach (var calculation in session.Calculations)
                CalcAreas(calculation); //  расчет площадей ПДИ по скорости и по датчикам
            foreach (var calculation in session.Calculations)
            {
                var analysisCalculation = new AnalysisCalculation();
                receptionChamber = calculation.DataOutput.ReceptionChamber;
                triggerChamber = calculation.DataOutput.TriggerChamber;

                //  таблицы с ПДИ
                var dataTypes = TaskHelper.GroupDiagDataByType(calculation.DiagDataList);

                foreach (var dataType in dataTypes)
                {
                    areaKpp = areaFail = 0;
                    if (dataType.Key == enCdmDirectionName.None)
                    {
                        foreach (var noneType in dataType.Value)
                        {
                            areaKpp = areaFail = 0;
                            //  смежные датчики: считаю площадь и возвращем лист смежных датчиков  
                            var areaContiguous = CalcAreasСontiguous(noneType, calculation.DataOutput.Diameter,
                                out var sensorsСontiguous);
                            //  начинаем считать все кроме сд360
                            var rowDataSorted = FillFieldsTableByPdi(noneType, null, out var doubleAngle);

                            var analysisType = new AnalysisType()
                            {
                                AreaFail = areaFail,
                                AreaKpp = areaKpp,
                                NumberSensorsBlock = noneType.NumberSensorsBlock,
                                CdmType = enCdmDirectionName.None,
                                DataType = noneType.DataType,
                                AreaLdi = noneType.AreaLdi,
                                AreaContiguous = areaContiguous,
                                SensorsСontiguous = sensorsСontiguous,
                                RowDataSorted = rowDataSorted,
                                DoubleAngle = doubleAngle
                            };
                            analysisCalculation.AnalisysTypeCollection.Add(analysisType);
                        }
                    }
                    else
                    {
                        areaKpp = areaFail = 0;

                        if (dataType.Value.Any(item => item.DataType == DataType.CDPA))
                        {
                            var cdpaDiagDataList = new List<IDiagData>();
                            var items = dataType.Value.Select(item => item as CDpaDiagData);
                            if (items != null)
                                cdpaDiagDataList.AddRange(items);


                            analysisCalculation.AnalisysTypeCollection.Add(GetAnalysisType(cdpaDiagDataList,
                                calculation.DataOutput.Diameter,
                                DataType.CDPA,
                                dataType.Key,
                                dataType.Value.Sum(q => q.AreaLdi)));
                        }
                        else if (dataType.Value.Any(item => item.DataType == DataType.Cd360))
                        {
                            var cdmDiagDataList = new List<IDiagData>();
                            var items = dataType.Value.Select(item => item as CdmDiagData);
                            if (items != null)
                                cdmDiagDataList.AddRange(items);

                            analysisCalculation.AnalisysTypeCollection.Add(GetAnalysisType(cdmDiagDataList,
                                calculation.DataOutput.Diameter,
                                DataType.Cd360,
                                dataType.Key,
                                dataType.Value.Sum(q => q.AreaLdi)));
                        }
                    }
                }

                analysisCalculation.DiagDataMain = GetAreaTypeSpeedInfo(calculation);
                CheckRestart(analysisCalculation, calculation);
                dictionaryAnalysisCalculation.Add(calculation, analysisCalculation);
            }

            foreach (var calculation in session.Calculations)
                loggerInfoAction?.Invoke($"{calculation.DataOutput.WorkItemName}: Проверка перезапуска завершена");
            
            updateAction?.Invoke(session);

            return dictionaryAnalysisCalculation;
        }

        /// <summary>
        /// Подсчет площади смежных датчиков и формирование списка смежных датчиков
        /// </summary>
        /// <param name="diagData"></param>
        /// <param name="diameter"> Диаметр</param>
        /// <param name="sensorsСontiguous"> Списка смежных датчиков</param>
        /// <returns></returns>
        public static double CalcAreasСontiguous(IDiagData diagData, double diameter, out Dictionary<int, List<RowDataAllTypes>> sensorsСontiguous)
        {
            sensorsСontiguous = new Dictionary<int, List<RowDataAllTypes>>();
            var pipeDiameter = diameter / 1000f;
            var sensorArea = pipeDiameter * Math.PI / diagData.SensorCount;
            var areas = 0.0d;

            var diagDatasNearRangeSensors = NearRangeSensors(diagData.HaltingSensors, diagData.HaltingSensors.Count, 2);

            if (diagData.NumberSensorsBlock == 0)
            {
                foreach (var sensors in diagDatasNearRangeSensors)
                    for (var i = 0; i < sensors.Count; i++)
                        if (sensors.Count == diagData.SensorCount)
                            AllSensorСontiguous(i, ref areas, sensorArea, sensors, sensorsСontiguous, diagData.HaltingSensors);
                        else
                            PartialSensorСontiguous(i, ref areas, sensorArea, sensors, sensorsСontiguous, diagData.HaltingSensors);
            }
            else if (diagData.NumberSensorsBlock > 0)
            {
                switch (diagData.DataType)
                {
                    case DataType.TfiT4:
                    case DataType.TfiT41:
                    case DataType.MflT1:
                    case DataType.MflT11:
                    case DataType.MflT2:
                    case DataType.MflT22:
                    case DataType.MflT3:
                    case DataType.MflT31:
                    case DataType.MflT32:
                    case DataType.MflT33:
                    case DataType.MflT34:
                        var diagDatasNearRangeSensorsFiltr = diagDatasNearRangeSensors.Where(q => q.Count >= diagData.NumberSensorsBlock * 2).ToList();
                        CheckInRow(diagDatasNearRangeSensorsFiltr);
                        foreach (var sensors in diagDatasNearRangeSensorsFiltr)
                        {
                            СreatingListSensorBlocks(diagData, sensors, out var haltingSensorsBlocks, out var sensorsBlocks);
                                if (haltingSensorsBlocks.Count >= 2)
                                    for (var i = 0; i < sensorsBlocks.Count; i++)
                                        if (sensors.Count == diagData.SensorCount)
                                            AllSensorСontiguous(i, ref areas, sensorArea, sensorsBlocks, sensorsСontiguous, haltingSensorsBlocks);
                                        else
                                            PartialSensorСontiguous(i, ref areas, sensorArea, sensorsBlocks, sensorsСontiguous, haltingSensorsBlocks);
                        }
                        break;
                }
            }
            return areas;
        }

        private static void CheckInRow(List<List<int>> diagDatasNearRangeSensorsFiltr)
        {
            for (var z = 0; z < diagDatasNearRangeSensorsFiltr.Count; z++)
            {
                var sensors = diagDatasNearRangeSensorsFiltr[z];
                sensors.Sort();
                for (int i = 0, j = 1; j < sensors.Count; i++, j++)
                {
                    if (sensors[j] - sensors[i] != 1)
                    {
                        var bufferList = sensors.GetRange(0, i+1);
                        sensors.RemoveRange(0,i+1);
                        i = 0; j = 1;
                        diagDatasNearRangeSensorsFiltr.Add(bufferList);
                    }
                }
            }
        }

        private static void СreatingListSensorBlocks(IDiagData diagData, List<int> sensors, out Dictionary<int, List<SensorRange>> haltingSensorsBlocks, out List<int> sensorsBlocks)
        {
            haltingSensorsBlocks = new Dictionary<int, List<SensorRange>>();
            sensorsBlocks = new List<int>();

            var blocks = new List<List<int>>();

            sensors.Sort();

            for (var i = 0; i < sensors.Count; i++) // формируем список блоков номеров датчиков
                if (0 == (i + 1) % diagData.NumberSensorsBlock)
                    blocks.Add(sensors.GetRange(i + 1 - diagData.NumberSensorsBlock, diagData.NumberSensorsBlock));

            var index = (int)(sensors[0] / diagData.NumberSensorsBlock);
            foreach (var block in blocks)
            {
                var firstSensorDistances = diagData.HaltingSensors[block[0]];
                foreach (var firstSensorDistance in firstSensorDistances)
                {
                    var buferList = new List<SensorRange>(); // буфер для смежных данных блоке
                    var i = 1;
                    while (i < block.Count)
                    {
                        if (i == 1)
                        {
                            var filterNextSensorDistances = FilterSensorDistances(firstSensorDistance, diagData.HaltingSensors[block[i]]);
                            foreach (var nextSensorDistance in filterNextSensorDistances)
                            {
                                var maxBegin = Math.Max(firstSensorDistance.Begin, nextSensorDistance.Begin);
                                var minEnd = Math.Min(firstSensorDistance.End, nextSensorDistance.End);
                                buferList.Add(new SensorRange { Begin = maxBegin, End = minEnd });
                            }
                        }
                        else
                        {
                            var buferIntermediary = new List<SensorRange>(); // промежуточный буфер для накопления данных в блоке
                            for (var j = 0; j < buferList.Count; j++)
                            {
                                var filterNextSensorDistances = FilterSensorDistances(buferList[j], diagData.HaltingSensors[block[i]]);
                                foreach (var nextSensorDistance in filterNextSensorDistances)
                                {
                                    var maxBegin = Math.Max(buferList[j].Begin, nextSensorDistance.Begin);
                                    var minEnd = Math.Min(buferList[j].End, nextSensorDistance.End);

                                    buferIntermediary.Add(new SensorRange { Begin = maxBegin, End = minEnd });
                                }
                            }
                            if (buferIntermediary.Count == 0)
                                break;

                            buferList.RemoveRange(0, buferList.Count);
                            buferList.AddRange(buferIntermediary);
                        }
                        i++;
                    }
                    if (i >= block.Count)
                        if (haltingSensorsBlocks.ContainsKey(index))
                        {
                            haltingSensorsBlocks[index].AddRange(buferList);
                        }
                        else
                        {
                            haltingSensorsBlocks[index] = buferList;
                            sensorsBlocks.Add(index);
                        }
                }
                index++;
            }
        }

        private static void AllSensorСontiguous(int i, ref double areas, double sensorArea, List<int> sensors, Dictionary<int, List<RowDataAllTypes>> sensorsСontiguous, Dictionary<int, List<SensorRange>> haltingSensors)
        {
            var previousIndex = (i - 1 + sensors.Count) % sensors.Count;
            var nextIndex = (i + 1 + sensors.Count) % sensors.Count;

            var previousSensorDistances = haltingSensors[sensors[previousIndex]];
            var firstSensorDistances = haltingSensors[sensors[i]];
            var nextSensorDistances = haltingSensors[sensors[nextIndex]];

            var inSensorAdd = false;
            var сontiguousAlongDist = new List<RowDataAllTypes>();
            foreach (var firstSensorDistance in firstSensorDistances)
            {
                var filterPreviousSensorDistances = FilterSensorDistances(firstSensorDistance, previousSensorDistances);
                var filterNextSensorDistances = FilterSensorDistances(firstSensorDistance, nextSensorDistances);
                double start = double.NaN, stop = double.NaN;
                if (filterPreviousSensorDistances.Count > 0 && filterNextSensorDistances.Count > 0)
                {
                    foreach (var previousSensorDistance in filterPreviousSensorDistances)
                        foreach (var nextSensorDistance in filterNextSensorDistances)
                        {
                            var maxPrevBegin = Math.Max(firstSensorDistance.Begin, previousSensorDistance.Begin);
                            var minPrevEnd = Math.Min(firstSensorDistance.End, previousSensorDistance.End);
                            var maxNextBegin = Math.Max(firstSensorDistance.Begin, nextSensorDistance.Begin);
                            var minNextEnd = Math.Min(firstSensorDistance.End, nextSensorDistance.End);

                            var maxBegin = Math.Min(maxPrevBegin, maxNextBegin);
                            var minEnd = Math.Max(minPrevEnd, minNextEnd);

                            if (double.IsNaN(start) && double.IsNaN(stop))
                            {
                                start = maxBegin;
                                stop = minEnd;
                            }
                            else
                            {
                                start = Math.Min(start, maxBegin);
                                stop = Math.Max(stop, minEnd);
                            }
                        }
                    if (start >= firstSensorDistance.Begin && stop <= firstSensorDistance.End)
                    {
                        inSensorAdd = true;
                        var area = sensorArea * (stop - start);
                        сontiguousAlongDist.Add(new RowDataAllTypes() { AreaContiguous = area, StartDistance = firstSensorDistance.Begin, EndDistance = firstSensorDistance.End });
                        areas += area;
                    }
                }
                else if (filterPreviousSensorDistances.Count > 0)
                {
                    GetSensorСontiguous(ref inSensorAdd, sensorArea, ref areas, сontiguousAlongDist, new List<SensorRange> { firstSensorDistance }, filterPreviousSensorDistances);
                }
                else if (filterNextSensorDistances.Count > 0)
                {
                    GetSensorСontiguous(ref inSensorAdd, sensorArea, ref areas, сontiguousAlongDist, new List<SensorRange> { firstSensorDistance }, filterNextSensorDistances);
                }
            }
            if (inSensorAdd)
                sensorsСontiguous[sensors[i] + 1] = сontiguousAlongDist;
        }

        private static void PartialSensorСontiguous(int i, ref double areas, double sensorArea, List<int> sensors, Dictionary<int, List<RowDataAllTypes>> sensorsСontiguous, Dictionary<int, List<SensorRange>> haltingSensors)
        {
            var previousIndex = i - 1;
            var nextIndex = i + 1;

            if (nextIndex > sensors.Count - 1 || previousIndex < 0)
                LeftOrRightSensorСontiguous(i, ref areas, sensorArea, sensors, sensorsСontiguous, haltingSensors);
            else
                AllSensorСontiguous(i, ref areas, sensorArea, sensors, sensorsСontiguous, haltingSensors);
        }

        private static void LeftOrRightSensorСontiguous(int i, ref double areas, double sensorArea, List<int> sensors, Dictionary<int, List<RowDataAllTypes>> sensorsСontiguous, Dictionary<int, List<SensorRange>> haltingSensors)
        {
            var previousIndex = i - 1;
            var nextIndex = i + 1;

            var firstSensorDistances = haltingSensors[sensors[i]];
            var nextOrPreviousSensorDistances = haltingSensors[sensors[nextIndex > sensors.Count - 1 ? previousIndex : nextIndex]];

            var inSensorAdd = false;
            var сontiguousAlongDist = new List<RowDataAllTypes>();

            GetSensorСontiguous(ref inSensorAdd, sensorArea, ref areas, сontiguousAlongDist, firstSensorDistances, nextOrPreviousSensorDistances);

            if (inSensorAdd)
                sensorsСontiguous[sensors[i] + 1] = сontiguousAlongDist;
        }

        private static void GetSensorСontiguous(ref bool inSensorAdd, double sensorArea, ref double areas,
            List<RowDataAllTypes> сontiguousAlongDist,
            List<SensorRange> firstSensorDistances,
            List<SensorRange> nextOrPreviousSensorDistances)
        {
            foreach (var firstSensorDistance in firstSensorDistances)
            {
                var filterNextOrPreviousSensorDistances = FilterSensorDistances(firstSensorDistance, nextOrPreviousSensorDistances);
                double start = double.NaN, stop = double.NaN;
                foreach (var nextOrPreviousSensor in filterNextOrPreviousSensorDistances)
                {
                    if (firstSensorDistance.End < nextOrPreviousSensor.Begin || firstSensorDistance.Begin > nextOrPreviousSensor.End)
                        continue;
                    var minEnd = Math.Min(firstSensorDistance.End, nextOrPreviousSensor.End);
                    var maxBegin = Math.Max(firstSensorDistance.Begin, nextOrPreviousSensor.Begin);

                    if (double.IsNaN(start) && double.IsNaN(stop))
                    {
                        start = maxBegin;
                        stop = minEnd;
                    }
                    else
                    {
                        start = Math.Min(start, maxBegin);
                        stop = Math.Max(stop, minEnd);
                    }
                }
                if (start >= firstSensorDistance.Begin && stop <= firstSensorDistance.End)
                {
                    inSensorAdd = true;
                    var area = sensorArea * (stop - start);
                    сontiguousAlongDist.Add(new RowDataAllTypes() { AreaContiguous = area, StartDistance = firstSensorDistance.Begin, EndDistance = firstSensorDistance.End });
                    areas += area;
                }
            }
        }

        private static List<SensorRange> FilterSensorDistances(SensorRange firstSensorDistance, List<SensorRange> sensorDistances)
        {
            var filterSensorDistances = new List<SensorRange>();
            foreach (var sensor in sensorDistances)
            {
                if (firstSensorDistance.End < sensor.Begin || firstSensorDistance.Begin > sensor.End)
                    continue;

                filterSensorDistances.Add(sensor);
            }
            return filterSensorDistances;
        }

        private static List<List<int>> NearRangeSensors(Dictionary<int, List<SensorRange>> haltingSensors, int sensorCount, int nearSensorCount)
        {
            var nearRangeSensors = new List<List<int>>();
            var buff = new List<int>();
            var sensors = haltingSensors.Keys.OrderBy(q => q).ToList();
            var lastSensor = sensorCount - 1;

            for (var i = 0; i < sensors.Count; i++)
            {
                if (sensors[i] == lastSensor && sensors[0] == 0)
                {
                    if (!buff.Contains(sensors[i]))
                        buff.Add(sensors[i]);

                    for (var j = 0; j < sensors.Count; j++)
                    {
                        if (haltingSensors.ContainsKey(sensors[j] + 1))
                        {
                            if (!buff.Contains(sensors[j]))
                                buff.Add(sensors[j]);

                            if (!buff.Contains(sensors[j] + 1))
                                buff.Add(sensors[j + 1]);
                        }
                        else
                        {
                            if (nearRangeSensors.Count > 0)
                            {
                                if (buff.Count - 1 - j > 0 && nearRangeSensors[0][0] == buff[buff.Count - 1 - j])
                                    nearRangeSensors.RemoveAt(0);
                                break;
                            }
                        }
                    }
                }
                if (haltingSensors.ContainsKey(sensors[i] + 1))
                {
                    if (!buff.Contains(sensors[i]))
                        buff.Add(sensors[i]);

                    if (!buff.Contains(sensors[i] + 1))
                        buff.Add(sensors[i + 1]);
                }
                else
                {
                    if (buff.Count >= nearSensorCount)
                    {
                        nearRangeSensors.Add(buff);
                        buff = new List<int>();
                    }
                    buff.Clear();
                }
            }
            return nearRangeSensors;
        }

        private void CalcAreas(Calculation calculation)
        {
            if (calculation.DataOutput == null) return;

            calculation.DataOutput.OverallSpeedAreaLdi = 0;
            calculation.DataOutput.OverallAreaLdi = 0;
            calculation.DataOutput.OverallArea = 0;

            var pipeDiameter = calculation.DataOutput.Diameter / 1000f;

            // Расчёт площади ПДИ по скорости

            foreach (var diagData in calculation.DiagDataList)
            {
                if (diagData.SpeedInfos != null)
                    foreach (var speedInfo in diagData.SpeedInfos)
                    {
                        var speedAreaLdi = pipeDiameter * Math.PI * (speedInfo.Distance.End - speedInfo.Distance.Begin);
                        speedInfo.Area = Math.Round(speedAreaLdi, 3, MidpointRounding.AwayFromZero);
                        calculation.DataOutput.OverallSpeedAreaLdi += speedAreaLdi;
                    }
            }

            calculation.DataOutput.OverallSpeedAreaLdi = Math.Round(calculation.DataOutput.OverallSpeedAreaLdi, 3, MidpointRounding.AwayFromZero);

            // Расчёт площади ПДИ по датчикам
            foreach (var diagData in calculation.DiagDataList)
            {
                //diagData.SensorAreaLdi = new Dictionary<int, double>();
                diagData.AreaLdi = 0;

                calculation.DataOutput.OverallArea += pipeDiameter * Math.PI * diagData.DistanceLength;
                var sensorArea = pipeDiameter * Math.PI / diagData.SensorCount;

                foreach (var sensorData in diagData.HaltingSensors)
                {
                    var sensorDistLdi = 0d;

                    for (var i = 0; i < sensorData.Value.Count; i++)
                    {
                        var sensorDataItem = sensorData.Value[i];
                        sensorDistLdi += sensorDataItem.End - sensorDataItem.Begin;
                        sensorDataItem.Area = sensorArea * (sensorDataItem.End - sensorDataItem.Begin);

                        sensorData.Value[i] = sensorDataItem;
                    }

                    var sensorAreaLdi = sensorArea * sensorDistLdi;

                    //diagData.SensorAreaLdi[sensorData.Key] = sensorAreaLdi;
                    diagData.AreaLdi += sensorAreaLdi;
                }
                diagData.AreaLdi = diagData.AreaLdi;
                calculation.DataOutput.OverallAreaLdi += diagData.AreaLdi;
            }
        }

        private List<SpeedInfos> CreateSpeedInfos(ReferenceInputData dataOutput, IDiagData calculationDiagData)
        {
            var name = GetAreaSpeedInfoName(calculationDiagData);
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

        private Dictionary<enCommonDataType, DiagDataMain> GetAreaTypeSpeedInfo(Calculation calculation)
        {
            var diagDataMain = new Dictionary<enCommonDataType, DiagDataMain>();
            foreach (var diagData in calculation.DiagDataList)
            {
                switch (diagData.DataType)
                {
                    case DataType.Spm:
                        InnerGetAreaTypeSpeedInfo(diagData, diagDataMain, calculation.DataOutput, enCommonDataType.Spm);
                        break;

                    case DataType.Mpm:
                        InnerGetAreaTypeSpeedInfo(diagData, diagDataMain, calculation.DataOutput, enCommonDataType.Mpm);
                        break;

                    case DataType.Wm:
                        InnerGetAreaTypeSpeedInfo(diagData, diagDataMain, calculation.DataOutput, enCommonDataType.Wm);
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
                        InnerGetAreaTypeSpeedInfo(diagData, diagDataMain, calculation.DataOutput, enCommonDataType.Mfl);
                        break;

                    case DataType.Cdl:
                    case DataType.Cdc:
                    case DataType.Cd360:
                    case DataType.CDPA:
                        InnerGetAreaTypeSpeedInfo(diagData, diagDataMain, calculation.DataOutput, enCommonDataType.Cd);
                        break;
                    case DataType.Ema:
                        InnerGetAreaTypeSpeedInfo(diagData, diagDataMain, calculation.DataOutput, enCommonDataType.Ema);
                        break;
                }
            }
            return diagDataMain;
        }

        private string GetAreaSpeedInfoName(IDiagData diagData)
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
                case DataType.CDPA:
                    var CDpaDiagData = (CDpaDiagData)diagData;
                    return $"{CDpaDiagData.DirectionName}({CDpaDiagData.Angle}-{CDpaDiagData.EntryAngle})";
                case DataType.Ema: return "Ema";
                default: return "Не определен";
            }
        }


        private void InnerGetAreaTypeSpeedInfo(IDiagData diagData, Dictionary<enCommonDataType, DiagDataMain> diagDataMain, ReferenceInputData dataOutput, enCommonDataType cDataType)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="diagData"></param>
        /// <param name="DiagDataList"> Cdm(CD360) и CDPA</param>
        /// <param name="doubleAngle"> Двойные углы</param>
        /// <returns></returns>
        private List<RowDataAllTypes> FillFieldsTableByPdi(IDiagData diagData, List<IDiagData> diagDataList, out bool doubleAngle)
        {
            var rowData = new List<RowDataAllTypes>();
            var doubleAngleFlag = false;
            if (diagData != null)
            {
                // все кроме CD360 и CDPA
                rowData = RowDataAllTypes(diagData, false);
            }
            if (diagDataList != null) //  CD360 и CDPA
            {
                if (diagDataList.Any(item => item.DataType == DataType.CDPA))
                {
                    var cdpaDiagDataList = diagDataList.Select(item => item as CDpaDiagData).ToList();
                    rowData = RowDataOnlyCdpa(cdpaDiagDataList, out doubleAngleFlag);
                }
                else if (diagDataList.Any(item => item.DataType == DataType.Cd360))
                {
                    var cdmDiagDataList = diagDataList.Select(item => item as CdmDiagData).ToList();
                    rowData = RowDataOnlyCd360(cdmDiagDataList, out doubleAngleFlag);
                }
            }
            doubleAngle = doubleAngleFlag;
            //  сортировка по "отказам"
            return (from q in rowData orderby q.RangeType == enSensorRangeType.Failure descending select q).ToList();
        }

        private List<RowDataAllTypes> RowDataAllTypes(IDiagData diagData, bool doubleAngleFlag)
        {
            var rowDataAllTypes = new List<RowDataAllTypes>();
            var triggerChamberLocal = triggerChamber + diagData.StartDist;
            var receptionChamberLocal = diagData.StopDist - receptionChamber;

            foreach (var haltingSensor in diagData.HaltingSensors)
            {
                foreach (var sensorRange in haltingSensor.Value)
                {
                    //  1. (кпп)
                    if (sensorRange.Begin <= triggerChamberLocal && sensorRange.End <= triggerChamberLocal ||
                        sensorRange.Begin >= receptionChamberLocal && sensorRange.End >= receptionChamberLocal)
                    {
                        var rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = sensorRange.Begin,
                            EndDistance = sensorRange.End,
                            PipeType = enPipeType.Сhamber,
                            RangeType = sensorRange.RangeType,
                            Area = sensorRange.Area
                        };

                        CommonParams(rowData, diagData, sensorRange, doubleAngleFlag);

                        if (rowData.RangeType == enSensorRangeType.Failure)
                            areaFail = areaFail + sensorRange.Area;

                        rowDataAllTypes.Add(rowData);
                        areaKpp = areaKpp + sensorRange.Area;
                        continue;
                    }
                    //  2.
                    if (sensorRange.Begin < triggerChamberLocal && sensorRange.End < receptionChamberLocal)
                    {
                        // часть до (кпп)
                        var area = sensorRange.Area * (triggerChamberLocal - sensorRange.Begin) /
                               (sensorRange.End - sensorRange.Begin);

                        var rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = sensorRange.Begin,
                            EndDistance = triggerChamberLocal,
                            PipeType = enPipeType.Сhamber,
                            RangeType = sensorRange.RangeType,
                            Area = area
                        };

                        CommonParams(rowData, diagData, sensorRange, doubleAngleFlag);
                        rowDataAllTypes.Add(rowData);
                        areaKpp = areaKpp + area;
                        //часть после (лч)
                        area = sensorRange.Area * (sensorRange.End - triggerChamberLocal) /
                               (sensorRange.End - sensorRange.Begin);

                        rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = triggerChamberLocal,
                            EndDistance = sensorRange.End,
                            PipeType = enPipeType.Linear,
                            RangeType = sensorRange.RangeType,
                            Area = area
                        };

                        CommonParams(rowData, diagData, sensorRange, doubleAngleFlag);
                        rowDataAllTypes.Add(rowData);

                        if (rowData.RangeType == enSensorRangeType.Failure)
                            areaFail = areaFail + sensorRange.Area;

                        continue;
                    }
                    //  3.
                    if (sensorRange.Begin < triggerChamberLocal && sensorRange.End > receptionChamberLocal)
                    {
                        // часть до (кпп)
                        var area = sensorRange.Area * (triggerChamberLocal - sensorRange.Begin) / (sensorRange.End - sensorRange.Begin);
                        var rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = sensorRange.Begin,
                            EndDistance = triggerChamberLocal,
                            PipeType = enPipeType.Сhamber,
                            RangeType = sensorRange.RangeType,
                            Area = area
                        };

                        CommonParams(rowData, diagData, sensorRange, doubleAngleFlag);
                        rowDataAllTypes.Add(rowData);
                        areaKpp = areaKpp + area;

                        //середина (лч)
                        area = sensorRange.Area * (receptionChamberLocal - triggerChamberLocal) / (sensorRange.End - sensorRange.Begin);
                        rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = triggerChamberLocal,
                            EndDistance = receptionChamberLocal,
                            PipeType = enPipeType.Linear,
                            RangeType = sensorRange.RangeType,
                            Area = area
                        };

                        CommonParams(rowData, diagData, sensorRange, doubleAngleFlag);
                        rowDataAllTypes.Add(rowData);

                        //часть после (кпп)
                        area = sensorRange.Area * (sensorRange.End - receptionChamberLocal) / (sensorRange.End - sensorRange.Begin);
                        rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = receptionChamberLocal,
                            EndDistance = sensorRange.End,
                            PipeType = enPipeType.Сhamber,
                            RangeType = sensorRange.RangeType,
                            Area = area
                        };

                        CommonParams(rowData, diagData, sensorRange, doubleAngleFlag);
                        if (rowData.RangeType == enSensorRangeType.Failure)
                            areaFail = areaFail + sensorRange.Area;
                        rowDataAllTypes.Add(rowData);
                        areaKpp = areaKpp + area;

                        continue;
                    }
                    //  4. (лч)
                    if (sensorRange.Begin >= triggerChamberLocal && sensorRange.End <= receptionChamberLocal)
                    {
                        var area = sensorRange.Area;
                        var rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = sensorRange.Begin,
                            EndDistance = sensorRange.End,
                            PipeType = enPipeType.Linear,
                            RangeType = sensorRange.RangeType,
                            Area = area
                        };

                        CommonParams(rowData, diagData, sensorRange, doubleAngleFlag);
                        if (rowData.RangeType == enSensorRangeType.Failure)
                            areaFail = areaFail + sensorRange.Area;
                        rowDataAllTypes.Add(rowData);

                        continue;
                    }
                    //  5.
                    if (sensorRange.Begin > triggerChamberLocal && sensorRange.End > receptionChamberLocal)
                    {
                        // часть до (лч)
                        var area = sensorRange.Area * (receptionChamberLocal - sensorRange.Begin) / (sensorRange.End - sensorRange.Begin);
                        var rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = sensorRange.Begin,
                            EndDistance = receptionChamberLocal,
                            PipeType = enPipeType.Linear,
                            RangeType = sensorRange.RangeType,
                            Area = area
                        };

                        CommonParams(rowData, diagData, sensorRange, doubleAngleFlag);
                        rowDataAllTypes.Add(rowData);

                        //часть после (кпп)
                        area = sensorRange.Area * (sensorRange.End - receptionChamberLocal) / (sensorRange.End - sensorRange.Begin);
                        rowData = new RowDataAllTypes
                        {
                            SensorNumber = haltingSensor.Key + 1,
                            StartDistance = receptionChamberLocal,
                            EndDistance = sensorRange.End,
                            PipeType = enPipeType.Сhamber,
                            RangeType = sensorRange.RangeType,
                            Area = area
                        };

                        CommonParams(rowData, diagData, sensorRange, doubleAngleFlag);
                        if (rowData.RangeType == enSensorRangeType.Failure)
                            areaFail = areaFail + sensorRange.Area;
                        rowDataAllTypes.Add(rowData);
                        areaKpp = areaKpp + area;
                    }
                }
            }
            return rowDataAllTypes;
        }

        private void CommonParams(RowDataAllTypes rowData, IDiagData diagData, SensorRange distRange, bool doubleAngleFlag)
        {
            var percentPdi = (distRange.End - distRange.Begin) / diagData.DistanceLength;
            const double maxPercentPdi = 0.95;
            if (doubleAngleFlag)
            {
                if (diagData is CdmDiagData)
                {
                    rowData.DirectionSensor = ((CdmDiagData)diagData).Angle.ToString();
                    rowData.InputAngle = ((CdmDiagData)diagData).EntryAngle;
                }
                else
                {
                    rowData.DirectionSensor = ((CDpaDiagData)diagData).Angle.ToString();
                    rowData.InputAngle = ((CDpaDiagData)diagData).EntryAngle;
                }
            }

            if (percentPdi >= maxPercentPdi || distRange.RangeType == enSensorRangeType.Failure)
                rowData.RangeType = enSensorRangeType.Failure;
        }


        private List<RowDataAllTypes> RowDataOnlyCdpa(List<CDpaDiagData> cdmDiagData, out bool doubleAngleFlag)
        {
            doubleAngleFlag = false;
            var rowDataAllTypes = new List<RowDataAllTypes>();

            foreach (var diagData in cdmDiagData)
            {
                // добавляем углы
                doubleAngleFlag = true;
                rowDataAllTypes.AddRange(RowDataAllTypes(diagData, true));
            }
            return rowDataAllTypes;
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

        /// <summary>
        /// Поиск площади в линейно части
        /// </summary>
        /// <param name="sensorCount">Количество сенсоров</param>
        /// <param name="dataOutput">Входные данные</param>
        /// <param name="analysisType">Проанализированные данные ПДН по всей дистанции</param>
        /// <returns></returns>
        private double SearchAreaLinearPDI(int ? sensorCount, ReferenceInputData dataOutput, AnalysisType analysisType)
        {
            var areaLinearPDI         = 0.0d;
            var launchChamberLocal    = triggerChamber + dataOutput.DistRange.Begin;
            var receptionChamberLocal = dataOutput.DistRange.End - receptionChamber;
            var pipeDiameter          = dataOutput.Diameter / 1000f;
            var sensorArea            = pipeDiameter * Math.PI / (int)sensorCount;
            foreach (var item in analysisType.SensorsСontiguous)
            {
                foreach (var rowDataAllTypes in item.Value)
                {
                    if (rowDataAllTypes.EndDistance < launchChamberLocal || rowDataAllTypes.StartDistance > receptionChamberLocal)
                        continue;

                    var maxPrevBegin = Math.Max(rowDataAllTypes.StartDistance, launchChamberLocal);
                    var minPrevEnd   = Math.Min(rowDataAllTypes.EndDistance,   receptionChamberLocal);
                    var maxNextBegin = Math.Max(rowDataAllTypes.StartDistance, launchChamberLocal);
                    var minNextEnd   = Math.Min(rowDataAllTypes.EndDistance,   receptionChamberLocal);

                    var start = Math.Min(maxPrevBegin, maxNextBegin);
                    var stop  = Math.Max(minPrevEnd, minNextEnd);

                    areaLinearPDI += sensorArea * (stop - start);
                }
            }
            return areaLinearPDI;
        }


        /// <summary>
        /// Аналез перепуск ВИП
        /// </summary>
        /// <param name="analysisCalculation"></param>
        /// <param name="calculation"></param>
        private void CheckRestart(AnalysisCalculation analysisCalculation, Calculation calculation)
        {
            //  1. Критерий (общий для всех)
            const string criterion = "Суммарная площадь потерянной диагностической информации по смежным датчикам более 5% от общей обследованной площади.";

            var defectoscope = "";
            var dataTypes = TaskHelper.GroupDiagDataByType(calculation.DiagDataList);

            CheckRestartLengthDifference(analysisCalculation, calculation);

            foreach (var dataType in dataTypes)
            {
                if (dataType.Key == enCdmDirectionName.None)
                {
                    foreach (var noneType in dataType.Value)
                    {
                        if (noneType.DataType == DataType.Mpm)
                            defectoscope = "ПРН";

                        // линейная часть ПДИ
                        var areaLinearPDI = SearchAreaLinearPDI(calculation.DiagDataList.Where(t => t.DataType == noneType.DataType)?.FirstOrDefault().SensorCount, calculation.DataOutput, analysisCalculation.AnalisysTypeCollection.Where(q => q.DataType == noneType.DataType && q.CdmType == enCdmDirectionName.None)
                                                                                                                                  ?.FirstOrDefault());
                        // общая площадь сенок труб вдоль всей дистанции
                        var surveyArea = Math.Round(Math.PI * calculation.DataOutput.Diameter / 1000 * ((noneType.StopDist - noneType.StartDist)), 3);

                        // % ПДИ от общей площади
                        var percentLoss2 = (areaLinearPDI / surveyArea) * 100;
                        if (percentLoss2 > 5.0d)
                            analysisCalculation.RestartReport.Add(new RestartCriterion(noneType.DataType.ToString(), criterion));
                    }
                }
                else
                {
                    var dataTypeFirst = dataType.Value.FirstOrDefault();
                    var surveyArea = Math.Round(Math.PI * calculation.DataOutput.Diameter / 1000 * (dataTypeFirst.StopDist - dataTypeFirst.StartDist), 3);

                    // линейная часть ПДИ
                    var areaLinearPDI = SearchAreaLinearPDI(calculation.DiagDataList.Where(q => q.DataType == DataType.Cd360 || q.DataType == DataType.CDPA)
                                                                                    ?.FirstOrDefault().SensorCount, calculation.DataOutput,
                                                            analysisCalculation.AnalisysTypeCollection.Where(q => q.DataType == DataType.Cd360 || q.DataType == DataType.CDPA && q.CdmType == dataType.Key)
                                                                                                      ?.FirstOrDefault());

                    var percentLoss = (areaLinearPDI / surveyArea) * 100;
                    if (!(percentLoss > 5)) continue;
                    analysisCalculation.RestartReport.Add(new RestartCriterion(dataType.Key.ToString(), criterion));
                }
            }
            //TODO PRODUCT BACKLOG ITEM 35644
            // 3. Для ВИП с ультразвуковыми системами(WM, CD) – наличие потерь по 2 - м и более смежным сбойным датчикам протяженностью от 1 - го км.
            // 4. Для ВИП с магнитными системами(MFL, TFI) - наличие потерь по 2 - м и более смежным блокам датчиков протяженностью от 1 - го км.
            //  WM
            var wmDiagDataList = calculation.DiagDataList.Where(q => q.DataType == DataType.Wm);
            NearSensors(calculation.DataOutput, wmDiagDataList, analysisCalculation.RestartReport, 2);
            //  CD
            var cdDiagDataList = calculation.DiagDataList.Where(q => q.DataType == DataType.Cdc || q.DataType == DataType.Cdl || q.DataType == DataType.Cd360 || q.DataType == DataType.CDPA);
            NearSensors(calculation.DataOutput, cdDiagDataList, analysisCalculation.RestartReport, 2);
            //  MFL / TFI
            var mflDiagDataList = calculation.DiagDataList.Where(q => q.DataType == DataType.MflT1 || q.DataType == DataType.MflT11 ||
                                                                      q.DataType == DataType.MflT2 || q.DataType == DataType.MflT22 ||
                                                                      q.DataType == DataType.MflT3 || q.DataType == DataType.MflT31 ||
                                                                      q.DataType == DataType.MflT32 || q.DataType == DataType.MflT33 ||
                                                                      q.DataType == DataType.MflT34 || q.DataType == DataType.TfiT4 || q.DataType == DataType.TfiT41);


            NearSensorsMfl(calculation.DataOutput, mflDiagDataList, analysisCalculation.RestartReport, 2);
            //

            if (calculation.DiagDataList.Any(q => q.DataType != DataType.Mpm))
            {
                if (calculation.Carriers.Count == 0)
                    defectoscope = "";//calculation.RestartReport.Add(new RestartCriterion("", "Идентификаторы не определены."));
                else
                    defectoscope = calculation.Carriers.FirstOrDefault().Defectoscope;
            }
            ////TODO PRODUCT BACKLOG ITEM 34293
            ////  2. Для ВИП ДКП количиество смежных сбойных датчиков 2 и более протяженностью более 1 км
            //if (defectoscope.Contains("ДКП") || defectoscope.Contains("ДВУ"))
            //{
            //    //  WM
            //    var wmDiagDataList = calculation.DiagDataList.Where(q => q.DataType == DataType.Wm);
            //    NearSensors(wmDiagDataList, analysisCalculation.RestartReport, 2);
            //    //  CD
            //    var cdDiagDataList = calculation.DiagDataList.Where(q => q.DataType == DataType.Cdc || q.DataType == DataType.Cdl || q.DataType == DataType.Cd360);
            //    NearSensors(cdDiagDataList, analysisCalculation.RestartReport, 2);
            //}

            ////  4 и 5. Для ВИП с ультрозвуковыми системами - наличие потерь по 2 и более смежными датчиками протяженностью от 1 км
            ////         Для ВИП с магнитными системами - наличие потерь по 2-м и более смежыми блоками датчиков протяженностью от 1 км
            //else if (defectoscope.Contains("ДКК") || defectoscope.Contains("ДКУ") || defectoscope.Contains("УСК.03") ||
            //         defectoscope.Contains("УСК") || defectoscope.Contains("МСК") || defectoscope.Contains("ДМК") ||
            //         defectoscope.Contains("ДМУ") || defectoscope.Contains("ДКМ"))
            //{
            //    //  WM
            //    var wmDiagDataList = calculation.DiagDataList.Where(q => q.DataType == DataType.Wm);
            //    NearSensors(wmDiagDataList, analysisCalculation.RestartReport, 2);
            //    //  CD
            //    var cdDiagDataList = calculation.DiagDataList.Where(q => q.DataType == DataType.Cdc || q.DataType == DataType.Cdl || q.DataType == DataType.Cd360);
            //    NearSensors(cdDiagDataList, analysisCalculation.RestartReport, 2);
            //    //  MFL / TFI
            //    var mflDiagDataList = calculation.DiagDataList.Where(q => q.DataType == DataType.MflT1 || q.DataType == DataType.MflT11 ||
            //                                                              q.DataType == DataType.MflT2 || q.DataType == DataType.MflT22 ||
            //                                                              q.DataType == DataType.MflT3 || q.DataType == DataType.MflT31 ||
            //                                                              q.DataType == DataType.MflT32 || q.DataType == DataType.MflT33 ||
            //                                                              q.DataType == DataType.MflT34 || q.DataType == DataType.TfiT4 || q.DataType == DataType.TfiT41);


            //    NearSensorsMfl(mflDiagDataList, analysisCalculation.RestartReport, 2);
            //}


            //  3. Для ВИП УСК.04 количество смежных сбойных датчиков 2 и более протяженностью от 1 км //TODO задвоение Критерия для WM данных PRODUCT BACKLOG ITEM 37698 
            //if (defectoscope.Contains("УСК.04"))
            //{
            //    NearSensors(calculation.DataOutput, calculation.DiagDataList, analysisCalculation.RestartReport, 2);
            //}
            //  6. Для ВИП ПРН/ОПТ - отсутствие информации от 2 и более смежных каналов протяженностью от 1 км
            if (defectoscope.Contains("ПРН") || defectoscope.Contains("ОПТ"))
            {
                NearSensors(calculation.DataOutput, calculation.DiagDataList, analysisCalculation.RestartReport, 2);
            }

            //  7.  Для ВИП 40-ДМА - отсутствие информации от 2 и более смежных датчиков протяженностью от 1-го км
            // является потерей диагностической информации и требует перепуска ВИП.
            else if (defectoscope.Contains("ДМА"))
            {
                NearSensors(calculation.DataOutput, calculation.DiagDataList, analysisCalculation.RestartReport, 2);
            }

            calculation.IsNeedRestart = analysisCalculation.RestartReport.Count != 0;
            calculation.State |= enCalculationStateTypes.Analysis;
        }

        public void NearSensorsMfl(ReferenceInputData dataOutput, IEnumerable<IDiagData> diagDataList, List<RestartCriterion> restartCriterion, int nearSensorBlocks)
        {
            foreach (var diagData in diagDataList)
            {
                var nearSensorCount = diagData.NumberSensorsBlock * nearSensorBlocks;
                var check = CheckDataTypeCheck(dataOutput, diagData.HaltingSensors, diagData.SensorCount, DistanceChecking, nearSensorCount);
                if (check)
                {
                    var criterion =
                        $"Наличие смежных сбойных блоков датчиков {nearSensorBlocks} и более протяженностью от {DistanceChecking / 1000} - го км";
                    if (diagData is CdmDiagData data)
                        restartCriterion.Add(new RestartCriterion($"{data.DataType} " +
                                                                  $"{data.DirectionName}" +
                                                                  $"({data.Angle}, " +
                                                                  $"{data.EntryAngle})",
                            criterion));
                    else
                        restartCriterion.Add(new RestartCriterion($"{diagData.DataType}", criterion));
                }
            }
        }

        public void NearSensors(ReferenceInputData dataOutput, IEnumerable<IDiagData> diagDataList, List<RestartCriterion> restartCriterion, int nearSensorCount)
        {
            foreach (var diagData in diagDataList)
            {
                //nearSensorCount = nearSensorCount == 0 ? diagData.NumberSensorsBlock : nearSensorCount;
                var check = CheckDataTypeCheck(dataOutput, diagData.HaltingSensors, diagData.SensorCount, DistanceChecking, nearSensorCount);
                if (check)
                {
                    var criterion = $"Количество смежных сбойных датчиков {nearSensorCount} и более протяженностью от {DistanceChecking / 1000} - го км";
                    if (diagData is CdmDiagData dataCdm)
                        restartCriterion.Add(new RestartCriterion($"{dataCdm.DataType} "     +
                                                                         $"{dataCdm.DirectionName}" +
                                                                         $"({dataCdm.Angle}, "      +
                                                                         $"{dataCdm.EntryAngle})",
                                                                         criterion));
                    else if (diagData is CDpaDiagData dataCDpa)
                    {
                        restartCriterion.Add(new RestartCriterion($"{dataCDpa.DataType} "     +
                                                                         $"{dataCDpa.DirectionName}" +
                                                                         $"({dataCDpa.Angle}, "      +
                                                                         $"{dataCDpa.EntryAngle})",
                                                                         criterion));
                    }
                    else
                        restartCriterion.Add(new RestartCriterion($"{diagData.DataType}", criterion));
                }
            }
        }

        private bool CheckDataTypeCheck(ReferenceInputData dataOutput, Dictionary<int, List<SensorRange>> haltingSensors, int sensorCount, double distance, int nearSensorCount)
        {
            var launchChamberLocal = triggerChamber + dataOutput.DistRange.Begin;
            var receptionChamberLocal = dataOutput.DistRange.End - receptionChamber;
            var nearRangeSensors = NearRangeSensors(haltingSensors, sensorCount, nearSensorCount);

            foreach (var sensors in nearRangeSensors)
            {
                for (var i = 0; i < sensors.Count; i++)
                {

                    var firstSensorDistances = new Dictionary<SensorRange, List<int>>();
                    foreach (var sensor in haltingSensors[sensors[i]])
                    {

                        if (sensor.End - sensor.Begin < distance) continue;
                        firstSensorDistances.Add(sensor, new List<int>() { sensors[i] });
                    }

                    for (var j = i + 1; j < sensors.Count; j++)
                    {
                        var nextSensorDistances = haltingSensors[sensors[j]];
                        var buff = new Dictionary<SensorRange, List<int>>();
                        foreach (var firstSensorDistance in firstSensorDistances)
                        {
                            foreach (var nextSensorDistance in nextSensorDistances)
                            {
                                if (nextSensorDistance.End - nextSensorDistance.Begin < distance) continue;
                                if (firstSensorDistance.Key.End < nextSensorDistance.Begin ||
                                    firstSensorDistance.Key.Begin > nextSensorDistance.End) continue;
                                var min = Math.Min(firstSensorDistance.Key.End, nextSensorDistance.End);
                                var max = Math.Max(firstSensorDistance.Key.Begin, nextSensorDistance.Begin);

                                if (min < launchChamberLocal || max > receptionChamberLocal) continue;

                                if (min - max < distance) continue;
                                var range = new SensorRange() { Begin = max, End = min };
                                if (firstSensorDistances.ContainsKey(range))
                                {
                                    firstSensorDistances[range].Add(sensors[j]);
                                    buff.Add(range, firstSensorDistances[range]);
                                }
                                else
                                {
                                    buff.Add(range, new List<int>() { sensors[j] });
                                    buff[range].AddRange(firstSensorDistance.Value);
                                }
                            }
                        }
                        if (buff.Count == 0)
                            break;
                        firstSensorDistances = buff;
                    }
                    if (firstSensorDistances.Any(q => q.Value.Count >= nearSensorCount))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Критерий перепуска по «длина участка по ТЗ (м)» и «протяжённость по ВИП (м)» 
        /// </summary>
        private void CheckRestartLengthDifference(AnalysisCalculation analysisCalculation, Calculation calculation)
        {
            const string criterionGateValve = "Длина участка по техническому заданию больше протяжённости по ВИП.\nНекорректная работа одометрической системы ВИП.";
            const string criterion = "Длина участка по техническому заданию больше протяжённости по ВИП.";
            const string dataType = "На основании\nтехнического задания";

            var calcDataOutput = calculation.DataOutput;
            var distLength = calcDataOutput.DistRange.End - calcDataOutput.DistRange.Begin;

            // разница длины (%)
            var lengthDifferencePercent = (calcDataOutput.InspectionDirNameSectLengTechTask[calculation.DataLocation.InspectionDirName] - distLength) * 100 / distLength;
            // разница длины (м)
            var lengthDifference = calcDataOutput.InspectionDirNameSectLengTechTask[calculation.DataLocation.InspectionDirName] - distLength;

            if (calcDataOutput.InspectionDirNameSectLengTechTask[calculation.DataLocation.InspectionDirName] <= 1000d && !calcDataOutput.GateValve)
            {
                if (lengthDifferencePercent > 0.5d && lengthDifference > 500d)
                    analysisCalculation.RestartReport.Add(new RestartCriterion(dataType, criterion));
            }
            else if (lengthDifferencePercent > 0.5d && lengthDifference > 500 && calcDataOutput.GateValve)
            {
                analysisCalculation.RestartReport.Add(new RestartCriterion(dataType, criterionGateValve));
            }
            else if (lengthDifferencePercent > 0.5d && lengthDifference > 500 && !calcDataOutput.GateValve)
            {
                analysisCalculation.RestartReport.Add(new RestartCriterion(dataType, criterion));
            }
        }
    }
}

