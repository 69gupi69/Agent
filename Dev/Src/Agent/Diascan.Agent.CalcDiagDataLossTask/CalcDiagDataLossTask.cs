using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.DiagnosticData;
using DiCore.Lib.NDT.CoordinateProvider;
using DiCore.Lib.NDT.DataProviders.CDL;
using DiCore.Lib.NDT.DataProviders.EMA;
using DiCore.Lib.NDT.DataProviders.NAV;
using DiCore.Lib.NDT.DataProviders.NAV.NavSetup;
using DiCore.Lib.NDT.Types;
using CdlChecking;
using Diascan.Agent.Types;
using Diascan.Agent.Types.ModelCalculationDiagData;
using DiCore.Lib.NDT.DataProviders.CDPA;
using DiCore.Lib.NDT.DataProviders.WM.WM32;

namespace Diascan.Agent.CalcDiagDataLossTask
{
    public class DiagDataLossTask
    {
        private int countRangeNav;

        //private MatrixUploader mx;
        
        public bool Calc(Action<Session> updateAction, Action<object> loggerInfo, Calculation calculation, Session session, IDiagData diagData, CancellationTokenSource cts)
        {
            //mx = new MatrixUploader(@"D:\Agent_MpmNotchesExport.csv");
            //mx.Enable = false;

            if (calculation.DataLocation.FullPath == null) return false;
            if (diagData.State) return true;
            if (diagData.DataType == DataType.Nav)
            {
                diagData.State = true;
                return true;
            }

            using (var diagnosticData = new DiagnosticData())
            {
                try
                {
                    if (!diagnosticData.Open(calculation.DataLocation)) return false;
                }
                catch (Exception e)
                {
                    loggerInfo?.Invoke(e.Message);
                }

                var distRange = diagnosticData.GetDistanceRange(diagData.DataType);
                diagData.StartDist = distRange.Begin;
                diagData.StopDist = distRange.End;
                diagData.DistanceLength = distRange.End - distRange.Begin;
                diagData.SensorCount = diagnosticData.GetSensorCount(diagData.DataType);

                if (diagData.DataType == DataType.Cd360)
                    diagData.SensorCount /= diagnosticData.GetCdmDirections().Count;
                if (diagData.DataType == DataType.CDPA)
                    diagData.SensorCount /= diagnosticData.GetCDpaDirections().Count;

                diagData.MaxDistance = Math.Round(distRange.End, 3, MidpointRounding.AwayFromZero);

                var dist = Math.Max(Math.Round(distRange.Begin, 3, MidpointRounding.AwayFromZero), diagData.ProcessedDist);

                var progress = (int) (dist / 100);

                var diagParams = ConstDiagDataParams.GetAllParams().FirstOrDefault(item => item.DataType.HasFlag(diagData.DataType));
                if (diagParams == null) return false;

                var distEnd = dist + diagParams.Distance;

                while (distEnd < diagData.MaxDistance - diagParams.IgnoreAreasCount * diagParams.Distance)
                {
                    if (!Directory.Exists(calculation.DataLocation.BaseDirectory)) return false;
                    if (cts.IsCancellationRequested) return false;

                    switch (diagData.DataType)
                    {
                        case DataType.Cdl:
                            FindCdlHaltingSensors((DiagData)diagData, diagnosticData, diagParams, dist, distEnd);
                            break;
                        case DataType.Cd360:
                            FindCdmHaltingSensors((CdmDiagData)diagData, diagnosticData, diagParams, dist, distEnd);
                            break;
                        case DataType.CDPA:
                            FindCDpaHaltingSensors((CDpaDiagData)diagData, diagnosticData, diagParams, loggerInfo, dist, distEnd);
                            break;
                        case DataType.Mpm:
                            FindMpmHaltingSensors((MpmDiagData)diagData, diagnosticData, diagParams, dist, distEnd);
                            break;
                        case DataType.Wm:
                            FindWmHaltingSensors((DiagData)diagData, diagnosticData, diagParams, dist, distEnd);
                            break;
                        case DataType.TfiT4:
                        case DataType.TfiT41:
                            FindTfiHaltingSensors((DiagData)diagData, diagnosticData, diagParams, dist, distEnd);
                            break;
                        case DataType.MflT1:
                        case DataType.MflT11:
                        case DataType.MflT3:
                        case DataType.MflT31:
                        case DataType.MflT32:
                        case DataType.MflT33:
                        case DataType.MflT34:
                            FindMflHaltingSensors((DiagData)diagData, diagnosticData, diagParams, dist, distEnd);
                            break;
                        case DataType.Ema:
                            FindEmaHaltingSensors((EmaDiagData)diagData, diagnosticData, diagParams, dist, distEnd);
                            break;
                    }

                    dist = distEnd;
                    distEnd += diagParams.Distance;

                    if ((int) (dist / 100f) > progress)
                    {
                        diagData.ProcessedDist = dist;
                        updateAction?.Invoke(session);
                        progress++;
                    }
                }
                diagData.ProcessedDist = diagData.MaxDistance;
            }

            //mx.Upload();

            updateAction?.Invoke(session);
            diagData.State = true;
            return true;
        }

        private void SaveDataSpeedInfo(SpeedInfo[] speedInfos)
        {

            var pathSaveDataSpeedInfo = $@"D:\{DateTime.Now:dd.MM.yyyy}_{DateTime.Now.ToString("T").Replace(":", ".")}_SpeedInfo.csv";

            if (string.IsNullOrEmpty(pathSaveDataSpeedInfo)) return;

            var res = new StringBuilder();

            res.AppendLine($@" Distance; Speed; Time;");

            foreach (var speedInfo in speedInfos)
                res.AppendLine($@" {speedInfo.Distance}; {speedInfo.Speed}; {speedInfo.Time};");

            do
            {
                try
                {
                    System.IO.File.WriteAllText(pathSaveDataSpeedInfo, res.ToString().Replace(".", ","), Encoding.UTF8);
                    MessageBox.Show(null, "Скорость выгружина", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    break;
                }
                catch
                {
                    MessageBox.Show(null, "Произошла ошибка записи в файл!\nВыбрать другой путь для сохранения?",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    break;
                }
            } while (true);
        }

        public void CalcSpeedLdi(Calculation calculation)
        {
            if (calculation.DataLocation.FullPath == null) return;

            using (var dataProvider = new DiagnosticData())
            {
                if (!dataProvider.Open(calculation.DataLocation)) return;

                calculation.DataOutput.DistRange =
                    new Range<double>(dataProvider.CoordinateDataProvider.MinDistance, dataProvider.CoordinateDataProvider.MaxDistance);

                if (!(calculation.DataOutput.Diameter > 0))
                    calculation.DataOutput.Diameter = dataProvider.CoordinateDataProvider.CalcParameters.PipeDiameterMm;

                var minRangeLength = calculation.DataOutput.Diameter / 2000;

                var scanStart = dataProvider.CoordinateDataProvider.MinScan;

                var buildInterval = /*(int)(1 / dataProvider.CoordinateDataProvider.CalcParameters.OdoFactor);*/ (int)(calculation.DataOutput.Diameter * 0.5);
                var scanCount = (dataProvider.CoordinateDataProvider.MaxScan - dataProvider.CoordinateDataProvider.MinScan) / buildInterval;
                var speedInfos = dataProvider.CoordinateDataProvider.GetSpeedInfo(scanStart, scanCount, buildInterval);

                // SaveDataSpeedInfo(speedInfos);

                var range = new Range<double>();
                var speedLdiCount = 0;
                var totalLdiSpeed = 0f;

                foreach (var calculationDiagData in calculation.DiagDataList)
                {
                    if( Math.Abs(calculationDiagData.PassportSpeedDiapason.End - calculationDiagData.PassportSpeedDiapason.Begin) <= 0 )
                        continue;
                    var lowerSpeed = calculationDiagData.PassportSpeedDiapason.End;
                    var upperSpeed = calculationDiagData.PassportSpeedDiapason.Begin;

                    for (var i = 0; i < speedInfos.Length; i++)
                    {
                        var speedInfo = speedInfos[i];

                        if ((speedInfo.Speed > upperSpeed || speedInfo.Speed < lowerSpeed) && speedInfos.Length - 1 > i)
                        {
                            speedLdiCount++;

                            if (MathHelper.TestFloatEquals(range.Begin, 0))
                            {
                                range.Begin = Math.Round(speedInfo.Distance, 3, MidpointRounding.AwayFromZero);
                                totalLdiSpeed += (float)Math.Round(speedInfo.Speed, 3, MidpointRounding.AwayFromZero);
                            }
                            else
                            {
                                range.End = Math.Round(speedInfo.Distance, 3, MidpointRounding.AwayFromZero);
                                totalLdiSpeed += (float)Math.Round(speedInfo.Speed, 3, MidpointRounding.AwayFromZero);
                            }
                        }
                        else if (!MathHelper.TestFloatEquals(range.Begin, 0))
                        {
                            if (MathHelper.TestFloatEquals(range.End, 0))
                                range.End = Math.Round(speedInfo.Distance, 3, MidpointRounding.AwayFromZero);

                            if (range.End - range.Begin > minRangeLength)
                            {
                                var overallSpeedInfo = new OverSpeedInfo()
                                {
                                    Distance = range,
                                    Speed = (float)Math.Round(totalLdiSpeed / (float)speedLdiCount, 3,
                                        MidpointRounding.AwayFromZero)
                                };

                                CombineSpeedLdiDistance(calculationDiagData.SpeedInfos, overallSpeedInfo, calculationDiagData.PassportSpeedDiapason);
                            }
                            range.Begin = range.End = totalLdiSpeed = speedLdiCount = 0;
                        }
                    }
                }
            }
        }

        private void CombineSpeedLdiDistance(List<OverSpeedInfo> calculationSpeedInfos, OverSpeedInfo speedInfo, Range<double> speedThrRange)
        {
            if ( calculationSpeedInfos.Count == 0)
            {
                calculationSpeedInfos.Add(speedInfo);
                return;
            }

            var lastSpeedInfo = calculationSpeedInfos.LastOrDefault();

            var speedEqual = speedInfo.Speed > speedThrRange.End && lastSpeedInfo.Speed > speedThrRange.End ||
                             speedInfo.Speed < speedThrRange.Begin && lastSpeedInfo.Speed < speedThrRange.Begin;

            if (MathHelper.TestDoubleEquals(lastSpeedInfo.Distance.End, speedInfo.Distance.Begin) && speedEqual)
            {
                lastSpeedInfo.Speed = Math.Max(speedInfo.Speed, lastSpeedInfo.Speed);
                lastSpeedInfo.Distance = new Range<double>(lastSpeedInfo.Distance.Begin, speedInfo.Distance.End);
                calculationSpeedInfos[calculationSpeedInfos.Count - 1] = lastSpeedInfo;
            }
            else
                calculationSpeedInfos.Add(speedInfo);
        }

        private void FindCdlHaltingSensors(DiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, double distStart, double distStop)
        {
            using (var dataHandle = diagnosticData.GetCdlData(distStart, distStop))
            {
                if (dataHandle == null) return;
                var sensors = CalcCdHaltingSensors(dataHandle, diagParams);
                CombineHaltingSensors(diagData, diagParams, sensors, distStart, distStop);
            }
        }

        private void FindCdmHaltingSensors(CdmDiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, double distStart, double distStop)
        {
            using (var dataHandle = diagnosticData.GetCdmData(diagData.ToCdmDirection(), distStart, distStop))
            {
                if (dataHandle == null) return;
                var sensors = CalcCdHaltingSensors(dataHandle, diagParams);

                for (var i = 0; i < sensors.Count; i++)
                    sensors[i] = diagnosticData.CdmSensorIndexToSensor(sensors[i], diagData.ToCdmDirection());

                CombineHaltingSensors(diagData, diagParams, sensors, distStart, distStop);
            }
        }

        private unsafe float GetMedian(CDPASensorItem sensorItem)
        {
            var list = new List<float>();

            for (var j = 0; j < sensorItem.RayCount; j++)
            {
                for (var k = 0; k < sensorItem.Data->EchoCount; k++)
                {
                    if (!MathHelper.TestFloatEquals(sensorItem.Data->Echos->Amplitude, 0.0d)) //sensorItem.Data->Echos->Amplitude != 0
                        list.Add(sensorItem.Data->Echos->Amplitude);
                    sensorItem.Data->Echos++;
                }
                sensorItem.Data++;
            }

            if (list.Count > 0)
            {
                list.Sort();

                var itemIndex = list.Count / 2;

                if (list.Count % 2 == 0)
                    return (list[itemIndex] + list[itemIndex - 1]) / 2;

                return list[itemIndex];
            }

            return 0f;
        }

        //TODO: 36853
        private unsafe List<int> CalcCDpaHaltingSensors(DataHandleCDpa dataHandle, DiagDataParams diagParams)
        {
            var result = new List<int>();
            var row    = dataHandle.RowCount;
            var col    = dataHandle.ColCount;

            var percentPdi = (int)(col * 0.8f);

            for (var sensorIndex = 0; sensorIndex < row; sensorIndex++)
            {
                var countPdiDistance = 0; // колличество ПДИ вдоль diagParams.Distance у sensorIndex. Вслучии привешние 80% от col, sensorIndex == ПДИ
                for (var scanIndex = 0; scanIndex < col; scanIndex++)
                {
                    var value = *dataHandle.GetDataPointer(sensorIndex, scanIndex);
                    var median = GetMedian(value)/*0.0f*/;

                    if (Math.Abs(median) > 0) // median != 0
                    {
                        if (median > 50.0f)
                            countPdiDistance++;

                        if (countPdiDistance > percentPdi)
                        {
                            result.Add(sensorIndex);
                            break;
                        }
                    }
                    else
                    {
                        countPdiDistance++;
                        if (countPdiDistance > percentPdi)
                        {
                            result.Add(sensorIndex);
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private void FindCDpaHaltingSensors(CDpaDiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, Action<object> loggerInfo, double distStart, double distStop)
        {
            try
            {
                using (var dataHandle = diagnosticData.GetCDpaData(diagData.ToCDpaDirection(), distStart, distStop))
                {
                    if (dataHandle == null) return;
                    var sensors = CalcCDpaHaltingSensors(dataHandle, diagParams);

                    for (var i = 0; i < sensors.Count; i++)
                        sensors[i] = diagnosticData.CDpaSensorIndexToSensor(sensors[i], diagData.ToCDpaDirection());

                    CombineHaltingSensors(diagData, diagParams, sensors, distStart, distStop);
                }
            }
            catch (Exception e)
            {
                loggerInfo?.Invoke($"{"CDPA"}_DataType({diagData.DataType})_DirectionName({diagData.DirectionName})_Angle({diagData.Angle})_distStart({distStart})_distStop({distStop}) : Ошибка в расчете сбойных датчиков: {e}");
                if (!e.Source.Contains("Diascan.Utils.DataBuffers") && !e.Message.Contains("Операция успешно завершена"))
                    throw new Exception(e.Message, e);
            }
        }

        private List<int> CalcCdHaltingSensors(DataHandleCdl dataHandle, DiagDataParams diagParams)
        {
            var result = new List<int>();

            for (var sensorIndex = 0; sensorIndex < dataHandle.RowCount; sensorIndex++)
            {
                if (CheckEchoCount(dataHandle, sensorIndex, (int)diagParams.MinCdSignalCount))
                    result.Add(sensorIndex);
            }
            return result;
        }

        private unsafe bool CheckEchoCount(DataHandleCdl dataHandle, int sensorIndex, int signalCountThr)
        {
            var sensorDataPtr = dataHandle.GetDataPointer(sensorIndex, 0);
            for (var scanIndex = 0; scanIndex < dataHandle.ColCount; scanIndex++)
            {
                if (sensorDataPtr->Count > signalCountThr)
                    return false;
                sensorDataPtr++;
            }
            return true;
        }



        private unsafe float GetAverageEchoTime(DataHandleCdl dataHandle, int sensorIndex)
        {
            var echoCount = 0;
            var totalTime = 0f;
            var sensorDataPtr = dataHandle.GetDataPointer(sensorIndex, 0);

            for (var scanIndex = 0; scanIndex < dataHandle.ColCount; scanIndex++)
            {
                if (sensorDataPtr->Count > 0)
                {
                    for (var echoIndex = 0; echoIndex < sensorDataPtr->Count; echoIndex++)
                    {
                        var echo = sensorDataPtr->Echos + echoIndex;

                        totalTime += echo->Time;
                        echoCount++;
                    }
                }
                sensorDataPtr++;
            }
            return totalTime / echoCount;
        }

        private unsafe bool CheckSensorData(CDSensorDataEx* sensorData, float lowerBound, float upperBound)
        {
            for (var echoIndex = 0; echoIndex < sensorData->Count; echoIndex++)
            {
                var echo = sensorData->Echos + echoIndex;

                if (echo->Time > upperBound || echo->Time < lowerBound)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Комбинирование сенсоров и дистанций в основной контейнер данных Halting Sensors MPM данных
        /// </summary>
        /// <param name="diagData">Результат анализа Диагнастических данных MPM</param>
        /// <param name="haltingSensors36">Датчики и их Начало и Конец по 36 метров</param>
        /// <param name="haltingSensors9">Датчики и их Начало и Конец по 9 метров</param>
        private void CombineHaltingSensorsMPM(MpmDiagData diagData, Dictionary<int, SensorRange> haltingSensors36, Dictionary<int, List<SensorRange>> haltingSensors9)
        {
            var haltingSensors = diagData.HaltingSensors;
            var haltingSensors9And36 = new Dictionary<int, List<SensorRange>>();

            if (haltingSensors9.Count != 0)
                foreach (var haltingSensor9 in haltingSensors9)
                    if (haltingSensors36.ContainsKey(haltingSensor9.Key))
                        haltingSensors9And36.Add(haltingSensor9.Key, new List<SensorRange>() {haltingSensors36[haltingSensor9.Key]});
                    else if (haltingSensors9And36.ContainsKey(haltingSensor9.Key))
                        continue;
                    else
                        haltingSensors9And36.Add(haltingSensor9.Key, haltingSensor9.Value);
            else
                foreach (var haltingSensor36 in haltingSensors36)
                    haltingSensors9And36[haltingSensor36.Key] = new List<SensorRange>(){ haltingSensor36.Value };

            foreach (var haltingSensor9And36 in haltingSensors9And36)
            {
                if (haltingSensors.ContainsKey(haltingSensor9And36.Key))
                {
                    foreach (var itemSensorRange in haltingSensor9And36.Value)
                    {
                        var lastRange = haltingSensors[haltingSensor9And36.Key].LastOrDefault();

                        if (MathHelper.TestDoubleEquals(lastRange.End, itemSensorRange.Begin) && lastRange.RangeType == itemSensorRange.RangeType)
                        {
                            lastRange.End = itemSensorRange.End;
                            haltingSensors[haltingSensor9And36.Key][haltingSensors[haltingSensor9And36.Key].Count - 1] = lastRange;
                        }
                        else
                            haltingSensors[haltingSensor9And36.Key].Add(itemSensorRange);
                    }
                }
                else
                    haltingSensors[haltingSensor9And36.Key] = new List<SensorRange>(haltingSensor9And36.Value);
            }
        }


        /// <summary>
        /// Доп анализ диагностических данных Mpm TODO PBI 37183
        /// </summary>
        /// <param name="dataHandle">Прочитанные Диагностические данные в буфер</param>
        /// <param name="diagData">Результат анализа</param>
        /// <param name="diagnosticData">Диагностические данные</param>
        /// <param name="diagParams">Параметры анализа Диагностических данных</param>
        /// <param name="distStart">Начало участка анализа</param>
        /// <param name="distStop">Конец участка анализа</param>
        private unsafe void AdditionFindMpmHaltingSensors(DataHandle<float> dataHandle, MpmDiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, Dictionary<int, SensorRange> haltingSensor36, double distStart36 , double distStop36)
        {
            var offset     = Math.Round(distStop36 - distStart36, 3, MidpointRounding.AwayFromZero) * 0.25d;       // 36 метров разбиваем согласно алгоритму на 9 метров
            var distStart9 = distStart36;
            var distStop9  = distStart36 + offset;

            var offsetScan    = dataHandle.ColCount / 4; // 36 метров разбиваем согласно алгоритму на 9 метров (иначе говоря разбиваем общее количество сканов на 4 группы)
            var distScanStart = 0;
            var distScanStop  = 0;
            var allowedSensorsError = (int)Math.Round(offsetScan * 0.15d/*diagParams.AllowedSensorsError*/); // TODO 15% согласно функциональным требованиям PBI 37183

            distScanStop += offsetScan;
            var haltingSensors9 = new Dictionary<int, List<SensorRange>>();
            while (distScanStop <= dataHandle.ColCount)
            {
                var sensorsDist9 = new List<int>();
                for (var row = 0; row < dataHandle.RowCount ; row++) // Идем по датчикам
                {
                    var counter = 0;
                    for (var col = distScanStart; col < distScanStop; col++) // Идем по сканам датчика
                    {
                        var value = *dataHandle.GetDataPointer(row, col);
                        if (value > 250.0f)
                            counter++;

                        if (counter > allowedSensorsError) // Если количество сканов датчика > 30% от общего числа, то датчик ПДИ
                        {
                            sensorsDist9.Add(row);
                            break;
                        }
                    }
                }

                // Преобразовываем SensorIndex в физический Sensor
                for (var i = 0; i < sensorsDist9.Count; i++)
                {
                    var sensor = diagnosticData.SensorIndexToSensor(sensorsDist9[i], diagData.DataType);
                    if (haltingSensors9.ContainsKey(sensor))
                        haltingSensors9[sensor].Add(new SensorRange() { Begin = distStart9, End = distStop9, RangeType = enSensorRangeType.Other });
                    else
                        haltingSensors9.Add(sensor,new List<SensorRange>() { new SensorRange() { Begin = distStart9, End = distStop9, RangeType = enSensorRangeType.Other } });
                }

                // Сдвигаем участок в метрах
                distStart9 = distStop9;
                distStop9 += offset;

                // Сдвигаем участок в сканах
                distScanStart = distScanStop;
                distScanStop += offsetScan;
            }

            CombineHaltingSensorsMPM(diagData, haltingSensor36, haltingSensors9);
        }

        ///// <summary>
        ///// Поиск просечек Mpm
        ///// </summary>
        ///// <param name="diagnosticData"></param>
        ///// <param name="dataHandle">Прочитанные сырые данные</param>
        ///// <param name="diagData"></param>
        ///// <param name="distStart">Начало дистанции</param>
        ///// <param name="distStop">Конец дистанции</param>
        ///// <returns>Просечки сенсор/сканы</returns>
        //private unsafe Dictionary<int, List<int>> SearchNotchMpm(DiagnosticData diagnosticData, DataHandle<float> dataHandle, DiagData diagData, double distStart, double distStop)
        //{
        //    var scanStart  = diagnosticData.DistToDataTypeScan(distStart, diagData.DataType);
        //    var scanStop   = diagnosticData.DistToDataTypeScan(distStop,  diagData.DataType);
        //    var notchsList = new Dictionary<int, List<int>>();

        //    if (dataHandle.ColCount >= scanStop - scanStart)
        //        for (var row = 0; row < dataHandle.RowCount; row++)
        //        {
        //            var ScansIndx = new List<int>();
        //            for (var col = 1; col+1 < dataHandle.ColCount; col++)
        //            {
        //                var pointer      = *dataHandle.GetDataPointer(row, col);
        //                var leftPointer  = *dataHandle.GetDataPointer(row, col-1);
        //                var rightPointer = *dataHandle.GetDataPointer(row, col+1);

        //                if (pointer >= 250.0f && leftPointer < 250.0f && rightPointer < 250.0f)
        //                    ScansIndx.Add((int)scanStart);

        //                scanStart++;
        //            }

        //            if (ScansIndx.Count != 0)
        //                if (notchsList.ContainsKey(row))
        //                    notchsList[row].AddRange(ScansIndx);
        //                else
        //                    notchsList.Add(diagnosticData.SensorIndexToSensor(row, diagData.DataType), ScansIndx);
        //        }
            
        //    return notchsList;
        //}

        /// <summary>
        /// Поиск просечек Mpm
        /// </summary>
        /// <param name="diagnosticData"></param>
        /// <param name="dataHandle">Прочитанные сырые данные</param>
        /// <param name="diagData"></param>
        /// <param name="distStart">Начало дистанции</param>
        /// <param name="distStop">Конец дистанции</param>
        /// <returns>Просечки сенсор/сканы</returns>
        private unsafe List<Notch> SearchNotchMpm(DiagnosticData diagnosticData, DataHandle<float> dataHandle, DiagDataParams diagParams, MpmDiagData diagData, double distStart)
        {
            var scanStart  = diagnosticData.DistToDataTypeScan(distStart, diagData.DataType) ?? -1;
            var notches = new List<Notch>();

            for (var rowIndex = 0; rowIndex < dataHandle.RowCount; rowIndex++)
            {
                var scansInfo = new List<Scan>();
                for (var colIndex = 1; colIndex + 1 < dataHandle.ColCount; colIndex++)
                {
                    var dataPtr = *dataHandle.GetDataPointer(rowIndex, colIndex);
                    var leftPointer = *dataHandle.GetDataPointer(rowIndex, colIndex - 1);
                    var rightPointer = *dataHandle.GetDataPointer(rowIndex, colIndex + 1);

                    var currentScan = scanStart + colIndex;
                    if (dataPtr >= 250.0f && leftPointer < 250.0f && rightPointer < 250.0f)
                        scansInfo.Add( new Scan()
                        {
                            ScansIndx = currentScan,
                            Distance = diagnosticData.DataTypeScanToDist(currentScan, diagData.DataType) ?? -1d
                        });
                }

                //mx.AddValueToRow(diagnosticData.SensorIndexToSensor(rowIndex, DataType.Mpm));
                //mx.AddValueToRow(distStart);
                //mx.AddValueToRow(distStart + diagParams.Distance);
                //mx.AddValueToRow(scansInfo.Count);
                //mx.AddValueToRow((int)(diagParams.AllowedSensorsError * dataHandle.ColCount));
                //mx.AddValueToRow(dataHandle.ColCount);
                //mx.FinishRow();

                if (scansInfo.Count == 0 || scansInfo.Count >= diagParams.AllowedSensorsError * dataHandle.ColCount) continue;

                if (notches.Any(sensor => sensor.SensorIndex == rowIndex))
                    notches.Find(x => x.SensorIndex == rowIndex).Scans.AddRange(scansInfo);
                else
                    notches.Add(new Notch(){SensorIndex = diagnosticData.SensorIndexToSensor(rowIndex, diagData.DataType), Scans = scansInfo });
            }

            return notches;
        }

        ///// <summary>
        ///// Комбинирование сенсоров и сканов в основной контейнер данных diagData.NotchsList MPM данных
        ///// </summary>
        ///// <param name="diagData">Результат анализа Диагнастических данных MPM</param>
        ///// <param name="notchsListMpm">Датчики и их Начало и Конец по 36 метров</param>
        //private void CombineNotchsListMpm(DiagData diagData, Dictionary<int, List<int>> notchsListMpm)
        //{
        //    var notchsList = diagData.NotchsList;

        //    foreach (var notchListMpm in notchsListMpm)
        //        if (notchsList.ContainsKey(notchListMpm.Key))
        //            notchsList[notchListMpm.Key].AddRange(notchListMpm.Value);
        //        else
        //            notchsList.Add(notchListMpm.Key, new List<int>(notchListMpm.Value));
        //}

        /// <summary>
        /// Комбинирование сенсоров и сканов в основной контейнер данных diagData.NotchsList MPM данных
        /// </summary>
        /// <param name="diagData">Результат анализа Диагнастических данных MPM</param>
        /// <param name="notchsListMpm">Датчики и их Начало и Конец по 36 метров</param>
        private void CombineNotches(MpmDiagData diagData, List<Notch> notches)
        {
            foreach (var notch in notches)
                if (diagData.NotchsList.Any(sensor => sensor.SensorIndex == notch.SensorIndex))
                    diagData.NotchsList.Find(x => x.SensorIndex == notch.SensorIndex).Scans.AddRange(notch.Scans);
                else
                    diagData.NotchsList.Add(new Notch() { SensorIndex = notch.SensorIndex, Scans = notch.Scans });
        }

        private void FindMpmHaltingSensors(MpmDiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, double distStart, double distStop)
        {
            using (var dataHandle = GetDataHandle(diagnosticData, diagParams.DataType, distStart, distStop))
            {
                if (dataHandle == null) return;

                // Поиск просечек
                var notches = SearchNotchMpm(diagnosticData, dataHandle, diagParams,  diagData, distStart);
                CombineNotches(diagData, notches);

                // Поиск ПДИ (потеря диагностической информации)
                var sensorsDist36 = CalcHaltingSensors(dataHandle, diagParams);
                var haltingSensor36 = new Dictionary<int, SensorRange>();
                foreach (var sensor in sensorsDist36)
                    haltingSensor36.Add(diagnosticData.SensorIndexToSensor(sensor, diagData.DataType), new SensorRange() { Begin = distStart, End = distStop, RangeType = enSensorRangeType.Other });

                // TODO PBI 37183
                AdditionFindMpmHaltingSensors(dataHandle, diagData, diagnosticData, diagParams, haltingSensor36, distStart, distStop);
            }
        }


        /// <summary>
        ///  Получить сенсор индекс и его тип TODO PBI 37698
        /// </summary>
        /// <param name="dataHandle"></param>
        /// <param name="sensorIndex"></param>
        /// <returns></returns>
        private unsafe KeyValuePair<int, enSensorRangeType> ? GetSensorIndexRangeType(DataHandle<WM32SensorDataEx> dataHandle, int sensorIndex)
        {
            KeyValuePair<int, enSensorRangeType> ? sensorIndexRangeType = null;

            var sensorDataPtr = dataHandle.GetDataPointer(sensorIndex, 0);

            var pdiCount = 0;
            var otherCount = 0;
            var failureCount = 0;

            for (var scanIndex = 0; scanIndex < dataHandle.ColCount; scanIndex++)
            {
                if (sensorDataPtr->Count < 10) // ВГС\Загрязнение\Повреждение enSensorRangeType.Other
                {
                    otherCount++;
                    pdiCount++;
                }

                if (sensorDataPtr->Count <= 0) // Отказ enSensorRangeType.Failure 
                {
                    failureCount++;
                    pdiCount++;
                }
                sensorDataPtr++;
            }

            if ((pdiCount*100.0f)/(float)dataHandle.ColCount > 50.0f) // При превышении доли сканов с ПДИ более 50% по одному датчику на участке длиной 35 м, регистрируется ПДИ датчика.
                if (failureCount> otherCount)
                    sensorIndexRangeType = new KeyValuePair<int, enSensorRangeType>(sensorIndex, enSensorRangeType.Failure);
                else if (otherCount> failureCount)
                    sensorIndexRangeType = new KeyValuePair<int, enSensorRangeType>(sensorIndex, enSensorRangeType.Other);

            return sensorIndexRangeType;
        }


        /// <summary>
        /// Поиск ПДИ данных WM32 на дистанции distStart и distStop TODO PBI 37698
        /// </summary>
        /// <param name="dataHandle">Сырые lанные WM32</param>
        /// <param name="diagParams">Параметры диагностических данных</param>
        /// <returns></returns>
        private unsafe void CalcHaltingSensorsWM32(DiagnosticData diagnosticData, DataHandle<WM32SensorDataEx> dataHandle, DiagDataParams diagParams, DiagData diagData, double distStart, double distStop)
        {
            var row = dataHandle.RowCount;
            var haltingSensors = diagData.HaltingSensors;

            for (var sensorIndex = 0; sensorIndex < row; sensorIndex++)
            {
                var sensorIndexRangeType = GetSensorIndexRangeType(dataHandle, sensorIndex);
                if (sensorIndexRangeType != null)
                {
                   var sensor = diagnosticData.SensorIndexToSensor(sensorIndexRangeType.Value.Key, diagData.DataType);
                   var addRange = new SensorRange() { Begin = distStart, End = distStop, RangeType = sensorIndexRangeType.Value.Value };

                   if (haltingSensors.ContainsKey(sensor))
                   {
                       var lastRange = haltingSensors[sensor].LastOrDefault();

                       if (MathHelper.TestDoubleEquals(lastRange.End, addRange.Begin) && lastRange.RangeType == addRange.RangeType)
                       {
                           lastRange.End = addRange.End;
                           haltingSensors[sensor][haltingSensors[sensor].Count - 1] = lastRange;
                       }
                       else
                           haltingSensors[sensor].Add(addRange);
                   }
                   else
                       haltingSensors[sensor] = new List<SensorRange> { addRange };
                }
            }
        }

        private void FindWmHaltingSensors(DiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, double distStart, double distStop)
        {
            if (diagnosticData.GetWMDataProvider().WmParameters.IsWM32)
            {
                using (var dataHandle = diagnosticData.GetWm32RawData(distStart, distStop))
                {
                    if (dataHandle == null) return;
                    CalcHaltingSensorsWM32(diagnosticData, dataHandle, diagParams, diagData, distStart, distStop);
                }
            }
            else
            {
                using (var dataHandle = diagnosticData.GetWmWtData(distStart, distStop))
                {
                    if (dataHandle == null) return;
                    var sensors = CalcHaltingSensors(dataHandle, diagParams);
                    for (var i = 0; i < sensors.Count; i++)
                        sensors[i] = diagnosticData.SensorIndexToSensor(sensors[i], diagData.DataType);
                    CombineHaltingSensors(diagData, diagParams, sensors, distStart, distStop);
                }
            }
        }

        private DataHandle<float> GetDataHandle(DiagnosticData diagnosticData, DataType dataType, double distStart, double distStop)
        {
            try
            {
                switch (dataType)
                {
                    case DataType.Mpm:
                        return diagnosticData.GetMpmData(distStart, distStop);
                    case DataType.TfiT4:
                    case DataType.TfiT41:
                    case DataType.MflT1:
                    case DataType.MflT11:
                    case DataType.MflT3:
                    case DataType.MflT31:
                    case DataType.MflT32:
                    case DataType.MflT33:
                    case DataType.MflT34:
                        return diagnosticData.GetMflData(distStart, distStop, dataType);
                    default:
                        return null;
                }
            }
            catch (Exception )
            {
                return null;
            }
        }

        private unsafe List<int> CalcHaltingSensors(DataHandle<float> dataHandle, DiagDataParams diagParams)
        {
            var result = new List<int>();

            var row = dataHandle.RowCount;
            var col = dataHandle.ColCount;
            var count = col * row;

            var median = GetMedian(dataHandle.Ptr, count);

            if (Math.Abs(median) > 0) // median != 0
            {
                var lowerBound = median * diagParams.LowerValue;
                var upperBound = median * diagParams.UpperValue;
                var allowedSensorsError = (int)Math.Round(dataHandle.ColCount * diagParams.AllowedSensorsError);

                for (var sensorIndex = 0; sensorIndex < row; sensorIndex++)
                {
                    var counter = 0;
                    for (var scanIndex = 0; scanIndex < col; scanIndex++)
                    {
                        var value = *dataHandle.GetDataPointer(sensorIndex, scanIndex);
                        if (!(value > lowerBound && value < upperBound))
                            counter++;

                        if (counter > allowedSensorsError)
                        {
                            result.Add(sensorIndex);
                            break;
                        }
                    }
                }
                return result;
            }

            for (var sensorIndex = 0; sensorIndex < dataHandle.RowCount; sensorIndex++)
                result.Add(sensorIndex);

            return result;
        }

        private unsafe float GetMedian(float* dataPtr, int count)
        {
            var list = new List<float>(count);

            for (var i = 0; i < count; i++)
            {
                if (!MathHelper.TestFloatEquals(*dataPtr, 0)) //*dataPtr != 0
                    list.Add(*dataPtr);
                dataPtr++;
            }

            if (list.Count > 0)
            {
                list.Sort();

                var itemIndex = list.Count / 2;

                if (list.Count % 2 == 0)
                    return (list[itemIndex] + list[itemIndex - 1]) / 2;

                return list[itemIndex];
            }
            return 0f;
        }

        private void FindMflHaltingSensors(DiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, double distStart, double distStop)
        {
            using (var dataHandle = GetDataHandle(diagnosticData, diagParams.DataType, distStart, distStop))
            {
                if (dataHandle == null) return;
                var sensors = CalcHaltingSensors(dataHandle, diagParams);
                var ranges = MflCalcCircularHaltingSensors(dataHandle, diagnosticData.DistToDataTypeScan(distStart, diagData.DataType) ?? 0);

                if (ranges.Count > 0)
                {
                    var excludeSensors = new List<int>();
                    for (var sensorIndex = 0; sensorIndex < dataHandle.RowCount; sensorIndex++)
                    {
                        if (!sensors.Contains(sensorIndex))
                            excludeSensors.Add(sensorIndex);
                    }
                    foreach (var range in ranges)
                    {
                        var distBeg = Math.Round(diagnosticData.DataTypeScanToDist(range.Begin, diagData.DataType) ?? 0, 3, MidpointRounding.AwayFromZero);
                        var distEnd = Math.Round(diagnosticData.DataTypeScanToDist(range.End, diagData.DataType) ?? 0, 3, MidpointRounding.AwayFromZero);
                        CombineHaltingSensors(diagData, diagParams, excludeSensors, distBeg, distEnd);
                    }
                }
                CombineHaltingSensors(diagData, diagParams, sensors, distStart, distStop);
            }
        }

        private unsafe List<int> CalcMflHaltingSensors(DiagnosticData diagnosticData, DataHandle<float> dataHandle, DiagDataParams diagParams) // медиана для групп датчиков
        {
            var result = new List<int>();

            var col = dataHandle.ColCount;
            var sensorsByGroup = diagnosticData.GetMflSensorsByGroup(diagParams.DataType);

            foreach (var groupItem in sensorsByGroup)
            {
                var median = GetMedian(dataHandle, groupItem.Value);

                if (Math.Abs(median) > 0) // median != 0
                {
                    var lowerBound = median * diagParams.LowerValue;
                    var upperBound = median * diagParams.UpperValue;
                    var allowedSensorsError = (int)Math.Round(dataHandle.ColCount * diagParams.AllowedSensorsError);

                    foreach (var sensors in groupItem.Value)
                    {
                        var counter = 0;
                        for (var scanIndex = 0; scanIndex < col; scanIndex++)
                        {
                            var value = *dataHandle.GetDataPointer(sensors, scanIndex);
                            if (!(value > lowerBound && value < upperBound))
                                counter++;

                            if (counter > allowedSensorsError)
                            {
                                result.Add(sensors);
                                break;
                            }
                        }
                    }
                }
                else
                    result.AddRange(groupItem.Value);
            }
            return result;
        }

        private void FindTfiHaltingSensors(DiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, double distStart, double distStop)
        {
            using (var dataHandle = GetDataHandle(diagnosticData, diagParams.DataType, distStart, distStop))
            {
                if (dataHandle == null) return;
                var sensors = CalcHaltingSensors(dataHandle, diagParams);
                var ranges = TfiCalcCircularHaltingSensors(dataHandle, diagnosticData.DistToDataTypeScan(distStart, diagData.DataType) ?? 0);

                var failureSensors = CalcTfiHaltingSensors(dataHandle);

                if (ranges.Count > 0)
                    foreach (var range in ranges)
                    {
                        var excludeSensors = new List<int>();
                        foreach (var sensorIndex in range.Key)
                            if (!sensors.Contains(sensorIndex))
                                excludeSensors.Add(sensorIndex);

                        var distBeg = Math.Round(diagnosticData.DataTypeScanToDist(range.Value.Begin, diagData.DataType) ?? 0, 3, MidpointRounding.AwayFromZero);
                        var distEnd = Math.Round(diagnosticData.DataTypeScanToDist(range.Value.End, diagData.DataType) ?? 0, 3, MidpointRounding.AwayFromZero);
                        CombineHaltingSensors(diagData, diagParams, excludeSensors, distBeg, distEnd);
                    }

                foreach (var failureSensor in failureSensors)
                {
                    var index = sensors.FindIndex(item => item == failureSensor);
                    if (index > -1)
                        sensors.RemoveAt(index);
                }

                CombineHaltingSensors(diagData, diagParams, sensors, distStart, distStop);
                CombineHaltingSensors(diagData, diagParams, failureSensors, distStart, distStop, enSensorRangeType.Failure); // <-
            }
        }

        private unsafe List<int> CalcTfiHaltingSensors(DataHandle<float> dataHandle)
        {
            var result = new List<int>();

            var row = dataHandle.RowCount;
            var col = dataHandle.ColCount;

            for (var sensorIndex = 0; sensorIndex < row; sensorIndex++)
            {
                var prevValue = *dataHandle.GetDataPointer(sensorIndex, 0);
                var repeatValueCount = 0;

                for (var scanIndex = 0; scanIndex < col; scanIndex++)
                {
                    var value = *dataHandle.GetDataPointer(sensorIndex, scanIndex);

                    if (MathHelper.TestFloatEquals(prevValue, value))
                        repeatValueCount++;
                }
                if (repeatValueCount == col)
                    result.Add(sensorIndex);
            }
            return result;

        }

        private unsafe float GetMedian(DataHandle<float> dataHandle, List<int> sensors)
        {
            var list = new List<float>();

            foreach (var sensor in sensors)
            {
                for (var scanIndex = 0; scanIndex < dataHandle.ColCount; scanIndex++)
                {
                    var value = *dataHandle.GetDataPointer(sensor, scanIndex);
                    if (!MathHelper.TestFloatEquals(value, 0f))
                        list.Add(value);
                }
            }

            if (list.Count > 0)
            {
                list.Sort();

                var itemIndex = list.Count / 2;

                if (list.Count % 2 == 0)
                    return (list[itemIndex] + list[itemIndex - 1]) / 2;

                return list[itemIndex];
            }
            return 0f;
        }

        private unsafe List<Range<int>> MflCalcCircularHaltingSensors(DataHandle<float> dataHandle, int scanStart)
        {
            var result = new List<Range<int>>();

            var row = dataHandle.RowCount;
            var col = dataHandle.ColCount;
            var scanCount = 0;
            var beginScan = scanStart;

            for (var scanIndex = 0; scanIndex < col; scanIndex++)
            {
                var sensorCount = 0;

                for (var sensorIndex = 0; sensorIndex < row; sensorIndex++)
                {
                    var dataPtr = dataHandle.GetDataPointer(sensorIndex, scanIndex);
                    if (MathHelper.TestFloatEquals(*dataPtr, 0f))
                        sensorCount++;
                    else
                        break;
                }

                if (sensorCount == row && col > scanIndex + 1)
                    scanCount++;
                else if (scanCount == 0)
                    beginScan++;
                else
                {
                    if (col == scanIndex + 1)
                        scanCount++;

                    result.Add(new Range<int>(beginScan, beginScan + scanCount));

                    beginScan = scanStart + scanIndex;
                    scanCount = 0;
                }
            }
            return result;
        }

        private unsafe List<KeyValuePair<List<int>, Range<int>>> TfiCalcCircularHaltingSensors(DataHandle<float> dataHandle, int scanStart)
        {
            var result = new List<KeyValuePair<List<int>, Range<int>>>();

            var row = dataHandle.RowCount;
            var col = dataHandle.ColCount;
            var beginScan = scanStart;

            var threshold = (int)(row * 0.25d);

            for (var scanIndex = 0; scanIndex < col; scanIndex++)
            {
                var listSensorIndex = new List<int>();
                for (var sensorIndex = 0; sensorIndex < row; sensorIndex++)
                {
                    var dataPtr = dataHandle.GetDataPointer(sensorIndex, scanIndex);
                    if (MathHelper.TestFloatEquals(*dataPtr, 0f))
                        listSensorIndex.Add(sensorIndex);
                }

                if (listSensorIndex.Count >= threshold && scanIndex + 1 < col)
                {
                    result.Add(new KeyValuePair<List<int>, Range<int>>(new List<int>(listSensorIndex), new Range<int>(beginScan, beginScan + 1)));
                    beginScan = scanStart + scanIndex;
                }

                beginScan++;
            }
            return result;
        }
        private bool CheckRelativeEmaRule(EmaRuleEnum rule)
        {
            switch (rule)
            {
                case EmaRuleEnum.L1:
                case EmaRuleEnum.L2:
                case EmaRuleEnum.R1:
                case EmaRuleEnum.R2:
                    return true;
                default: return false;
            }
        }

        private unsafe EmaEcho GetMaxEmaEchoByRule(EmaSensorData* sensorData)
        {
            var maxEcho = new EmaEcho() { Amplitude = float.MinValue, Time = float.MinValue };

            for (var echoIndex = 0; echoIndex < sensorData->EchoCount; echoIndex++)
            {
                var echo = *(sensorData->Echos + echoIndex);

                if (echo.Amplitude > maxEcho.Amplitude)
                {
                    maxEcho.Amplitude = echo.Amplitude;
                    maxEcho.Time = echo.Time;
                }
            }

            return maxEcho;
        }

        private unsafe bool CheckNullSensorItem(EmaSensorItem* sensorItemPtr)
        {
            var zeroRuleCount = 0;
            var maxEcho = EmaEcho.Empty;
            for (var ruleIndex = 0; ruleIndex < sensorItemPtr->RayCount; ruleIndex++)
            {
                var sensorData = sensorItemPtr->Data + ruleIndex;

                if (!CheckRelativeEmaRule(sensorData->Rule)) continue;

                var maxEchoByRule = GetMaxEmaEchoByRule(sensorData);

                if (maxEchoByRule.Amplitude > maxEcho.Amplitude)
                {
                    maxEcho.Amplitude = maxEchoByRule.Amplitude;
                    maxEcho.Time = maxEchoByRule.Time;
                }

                if (MathHelper.TestFloatEquals(maxEcho.Amplitude, 0) && MathHelper.TestFloatEquals(maxEcho.Time, 0))
                    zeroRuleCount++;
            }
            return zeroRuleCount == sensorItemPtr->RayCount;
        }

        private unsafe float GetMedianEMABScan(float* dataPtr, int count)
        {
            var list = new List<float>(count);

            for (var i = 0; i < count; i++)
            {
                if (!MathHelper.TestFloatEquals(*dataPtr, 0)) //*dataPtr != 0
                    list.Add(*dataPtr);
                dataPtr++;
            }

            if (list.Count > 0)
            {
                list.Sort();

                var itemIndex = list.Count / 2;

                if (list.Count % 2 == 0)
                    return (list[itemIndex] + list[itemIndex - 1]) / 2;

                return list[itemIndex];
            }
            return 0f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sensor"></param>
        /// <param name="sensorItemPtr"></param>
        /// <param name="diagData"></param>
        private unsafe void CalcEmaHaltingSensorBScan(int sensor, EmaSensorItem* sensorItemPtr, EmaDiagData diagData)
        {
            var BScanR1L1 = new List<EmaEcho>();
            var BScanR2L2 = new List<EmaEcho>();

            for (var i = 0; i < sensorItemPtr->RayCount; i++)
            {
                if (diagData.SensorAnalysisRules.ContainsKey(sensor))
                    foreach (var itemRule in diagData.SensorAnalysisRules[sensor])
                        if (sensorItemPtr->Data->Rule == itemRule.BScan)
                            switch (sensorItemPtr->Data->Rule)
                            {
                                case EmaRuleEnum.R1:
                                case EmaRuleEnum.L1:
                                    for (var j = 0; j < sensorItemPtr->Data->EchoCount; j++)
                                    {
                                        BScanR1L1.Add(*sensorItemPtr->Data->Echos);
                                        sensorItemPtr->Data->Echos++;
                                    }
                                    break;

                                case EmaRuleEnum.R2:
                                case EmaRuleEnum.L2:
                                    for (var j = 0; j < sensorItemPtr->Data->EchoCount; j++)
                                    {
                                        BScanR2L2.Add(*sensorItemPtr->Data->Echos);
                                        sensorItemPtr->Data->Echos++;
                                    }
                                    break;
                            }
                
                sensorItemPtr->Data++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sensor">Физический номер сенсора</param>
        /// <param name="scanCount">Количество сканов</param>
        /// <param name="sensorItemPtr">Данные скана датчика</param>
        /// <param name="diagData">Диагностические данные EMA</param>
        /// <returns></returns>
        private unsafe List<Range<int>> CalcEmaHaltingSensors(int sensor, int scanCount, EmaSensorItem* sensorItemPtr, EmaDiagData diagData)
        {


            for (var scanIndex = 0; scanIndex < scanCount; scanIndex++)
            {
                CalcEmaHaltingSensorBScan(sensor, sensorItemPtr, diagData);
                



                if (CheckNullSensorItem(sensorItemPtr) ) // <-
                {

                }

                sensorItemPtr++;
            }

            return new List<Range<int>>();
        }

        private unsafe void FindEmaHaltingSensors(EmaDiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, double distStart, double distStop)
        {
            using (var dataHandle = diagnosticData.GetEmaData(distStart, distStop))
            {
                if (dataHandle == null) return;
                var scanStart = diagnosticData.DistToDataTypeScan(distStart, diagData.DataType) ?? 0;

                for (var sensorIndex = 1; sensorIndex < dataHandle.RowCount; sensorIndex++)
                {
                    var sensorItemPtr = dataHandle.GetDataPointer(sensorIndex, 0);

                    var sensor = diagnosticData.SensorIndexToSensor(sensorIndex, diagData.DataType);

                    var ranges = CalcEmaHaltingSensors( sensor, dataHandle.ColCount, sensorItemPtr, diagData);

                    


                    foreach (var range in ranges)
                    {
                        var distBeg = Math.Round(diagnosticData.DataTypeScanToDist(range.Begin + scanStart, diagData.DataType) ?? 0, 3, MidpointRounding.AwayFromZero);
                        var distEnd = Math.Round(diagnosticData.DataTypeScanToDist(range.End + scanStart, diagData.DataType) ?? 0, 3, MidpointRounding.AwayFromZero);
                        CombineHaltingSensors(diagData, diagParams, new List<int>(1) { sensor }, distBeg, distEnd);
                    }
                }
            }
        }


        private void CombineHaltingSensors(IDiagData diagData, DiagDataParams diagParams, List<int> sensors, double distStart, double distStop, enSensorRangeType rangeType = enSensorRangeType.Other)
        {
            var haltingSensors = diagData.HaltingSensors;
            var addRange = new SensorRange(){Begin = distStart, End = distStop, RangeType = rangeType};

            foreach (var sensor in sensors)
            {
                if (haltingSensors.ContainsKey(sensor))
                {
                    var lastRange = haltingSensors[sensor].LastOrDefault();

                    if (MathHelper.TestDoubleEquals(lastRange.End, addRange.Begin) && lastRange.RangeType == addRange.RangeType)
                    {
                        lastRange.End = addRange.End;
                        haltingSensors[sensor][haltingSensors[sensor].Count - 1] = lastRange;
                    }
                    else if ((diagData.DataType == DataType.Cd360 || diagData.DataType == DataType.CDPA || diagData.DataType == DataType.Cdc || diagData.DataType == DataType.Cdl) && 
                             MathHelper.TestFloatEquals(lastRange.End + diagParams.Distance, addRange.Begin))
                    {
                        lastRange.End = addRange.End;
                        haltingSensors[sensor][haltingSensors[sensor].Count - 1] = lastRange;
                    }
                    else
                        haltingSensors[sensor].Add(addRange);
                }
                else
                    haltingSensors[sensor] = new List<SensorRange> { addRange };
            }
        }

        public void CheckCdTail(Calculation calculation, Session session, IDiagData diagData, CancellationTokenSource cts, Action<Session> updateAction, Action<object> loggerInfo)
        {
            using (var diagnosticData = new DiagnosticData())
            {
                try
                {
                    if (!diagnosticData.Open(calculation.DataLocation)) return;
                }
                catch (Exception e)
                {
                    loggerInfo?.Invoke(e.Message);
                }

                if (!diagnosticData.AvailableDataTypes.HasFlag(diagData.DataType) || calculation.DataOutput == null) return;

                var cdSeamChecker = new CdsChecking(calculation.DataOutput.Diameter) 
                {
                    OmniFile = calculation.DataOutput.WorkItemName
                };

                var distRanges = GetDistRanges(diagnosticData, diagData.DataType);
                const double readDistStep = 20d;
                var distRangeIndex = -1;

                foreach (var distRange in distRanges)
                {
                    distRangeIndex++;
                    if (calculation.CdTailDistProgress > distRange.End) continue;
                    var distStart = Math.Max(distRange.Begin, calculation.CdTailDistProgress);
                    var distStop = distStart + readDistStep;

                    var frames = new List<Rect>();

                    while (!MathHelper.TestDoubleEquals(distRange.End, distStart, 0.001))
                    {
                        if (cts.IsCancellationRequested) return;

                        var scanStart = diagnosticData.DistToDataTypeScan(distStart, diagData.DataType) ?? 0;

                        var cdData = PrepareCdData(diagnosticData, distStart, distStop, diagData);

                        var carrierRotatation = diagnosticData.CoordinateDataProvider.GetRotationVectorByAngle(scanStart, cdData.Item1.ColCount, 1);
                        var distances = new double[cdData.Item1.ColCount];

                        for (var scanIndex = 0; scanIndex < cdData.Item1.ColCount; scanIndex++)
                            distances[scanIndex] = diagnosticData.DataTypeScanToDist(scanIndex + scanStart, diagData.DataType) ?? 0d;

                        var stepRawFrames = cdSeamChecker.Execute(cdData.Item2, cdData.Item1, distances, carrierRotatation).OrderBy(item => item.Left).ToList();
                        cdData.Item1.Dispose();

                        for (var frameIndex = 0; frameIndex < stepRawFrames.Count; frameIndex++)
                        {
                            var frame = stepRawFrames[frameIndex];
                            frame.X += scanStart;
                            stepRawFrames[frameIndex] = frame;
                        }

                        frames.AddRange(GetRelativeFrames(diagnosticData, stepRawFrames, diagData.DataType));

                        var progressPercent = ((distStop - distRange.Begin) / (distRange.End - distRange.Begin) * 
                                               (1d / distRanges.Count) + (float)distRangeIndex / distRanges.Count) * 100;
                        calculation.ProgressCdlTail = progressPercent;
                        updateAction?.Invoke(session);
                        distStart = distStop;

                        if (distRange.End - distStop < readDistStep)
                            distStop += distRange.End - distStop;
                        else
                            distStop += readDistStep;

                        calculation.CdTailDistProgress = distStart;
                    }

                    //предполагаемое количество рамок на участке данных по дистанции
                    var supposedFrameCount = GetSupposedFrameCount(distRange.Length());
                    calculation.FramesTypeCds |= frames.Count >= supposedFrameCount;
                    calculation.Frames.AddRange(frames);

                    calculation.CdTailDistProgress = distRange.End;
                }
            }
        }

        private int GetSupposedFrameCount(double distLength)
        {
            const int frameCount = 20;
            return (int)(frameCount * Math.Round(distLength / 1000f, 3));
        }

        private List<Range<double>> GetDistRanges(DiagnosticData diagnosticData, DataType dataType)
        {
            var distRange = diagnosticData.GetDistanceRange(dataType);
            var distLength = distRange.End - distRange.Begin;

            if (distLength > 3000 && distLength <= 5000)
            {
                distRange.End = 3000;
                return new List<Range<double>>() { distRange };
            }

            if (distLength > 5000)
            {
                var dr1 = new Range<double>(distRange.Begin + 100, distRange.Begin + 1100);
                var dr2 = new Range<double>(distLength / 2, distLength / 2 + 1000);
                var dr3 = new Range<double>(distRange.End - 1100, distRange.End - 100);
                return new List<Range<double>>() { dr1, dr2, dr3 };
            }

            return new List<Range<double>>() { distRange };
        }

        private Tuple<DataHandleCdl, CdSensorInfo[]> PrepareCdData(DiagnosticData diagnosticData, double distStart, double distStop, IDiagData diagData)
        {
            switch (diagData.DataType)
            {
                case DataType.Cd360:
                    return PrepareCdmData(diagnosticData, ((CdmDiagData)diagData).ToCdmDirection(), distStart, distStop);
                case DataType.Cdl:
                    return PrepareCdlData(diagnosticData, distStart, distStop);
                default:
                    return null;
            }
        }

        private unsafe Tuple<DataHandleCdl, CdSensorInfo[]> PrepareCdmData(DiagnosticData diagnosticData, CdmDirection direction, double distStart, double distStop)
        {
            var dataHandles = diagnosticData.GetCdmData(direction.DirectionName, distStart, distStop).OrderBy(item => item.EntryAngle).ToList();

            if (dataHandles.Count == 0) return null;

            var row = dataHandles[0].RowCount;
            var col = dataHandles[0].ColCount;

            var fullDataHandle = new DataHandleCdl(row * dataHandles.Count, col);

            var cdSensorInfo = new CdSensorInfo[row * dataHandles.Count];
            var rowOffset = 0;

            foreach (var dataHandleCdm in dataHandles)
            {
                //var dir = diagnosticData.GetCdmDirections().Find(item => item.Id == dataHandleCdm.DirectionAngleCode);
                for (var rowIndex = 0; rowIndex < row; rowIndex++)
                {
                    var angle = diagnosticData.CdmSensorToAngle((short)(rowIndex + rowOffset), distStart);
                    cdSensorInfo[rowIndex + rowOffset] = new CdSensorInfo(angle, dataHandleCdm.EntryAngle, dataHandleCdm.DirectionAngle);

                    var directionDataPtr = dataHandleCdm.GetDataPointer(rowIndex, 0);
                    var dataPtr = fullDataHandle.GetDataPointer(rowIndex + rowOffset, 0);

                    for (var colIndex = 0; colIndex < col; colIndex++)
                    {
                        dataPtr->Count = directionDataPtr->Count;
                        dataPtr->Echos = fullDataHandle.Allocate<CDEcho>(dataPtr->Count);

                        for (var echoIndex = 0; echoIndex < dataPtr->Count; echoIndex++)
                            dataPtr->Echos[echoIndex] = directionDataPtr->Echos[echoIndex];
                        
                        dataPtr++;
                        directionDataPtr++;
                    }
                }
                rowOffset += row;
            }

            foreach (var dataHandleCdm in dataHandles)
                dataHandleCdm.Dispose();

            return new Tuple<DataHandleCdl, CdSensorInfo[]>(fullDataHandle, cdSensorInfo);
        }

        private Tuple<DataHandleCdl, CdSensorInfo[]> PrepareCdlData(DiagnosticData diagnosticData, double distStart, double distStop)
        {
            var dataHandle = diagnosticData.GetCdlData(distStart, distStop);
            var cdSensorInfo = new CdSensorInfo[dataHandle.RowCount];

            for (var rowIndex = 0; rowIndex < dataHandle.RowCount; rowIndex++) // <-
            {
                cdSensorInfo[rowIndex] = new CdSensorInfo(diagnosticData.CdlSensorToAngle((short)rowIndex, distStart), 
                    diagnosticData.GetCdlEntryAngle(), diagnosticData.GetCdlSensorRayDirection(rowIndex));
            }
            return new Tuple<DataHandleCdl, CdSensorInfo[]>(dataHandle, cdSensorInfo);
        }
        
        private IEnumerable<Rect> GetRelativeFrames(DiagnosticData diagnosticData, List<Rect> frameRects, DataType dataType)
        {
            var relativeRectIndexs = new List<int>();

            const double lowerDistThr = 0.01d;
            const double upperDistThr = 0.1d;
            const double angleHeightThr = 350d;

            for (var i = 0; i < frameRects.Count; i++)
            {
                var frameRect = frameRects[i];

                var frameDistStart = diagnosticData.DataTypeScanToDist((int)frameRect.Left, dataType) ?? 0;
                var frameDistStop = diagnosticData.DataTypeScanToDist((int)frameRect.Right, dataType) ?? 0;
                var frameDistLength = frameDistStop - frameDistStart;

                if (frameRect.Height < angleHeightThr || frameDistLength < lowerDistThr || frameDistLength > upperDistThr) continue;

                frameRect.X = frameDistStart;
                frameRect.Width = frameDistLength;

                frameRects[i] = frameRect;
                relativeRectIndexs.Add(i);
            }
            return frameRects.Where((rect, i) => relativeRectIndexs.Contains(i));
        }
        
        private void SoledSpotAngleCheck(NavSolidSpotAngleTypes solidExtent, NavSolidSpotMetersTypes solidExtentMeters, int scanIndex, float deltaPitchAngle, 
            float angleCheck, SolidSpotData solidSpotAngel, List<SolidSpotData> solidSpotsData, CoordinateDataProviderCrop cdp)
        {
            if( solidSpotAngel.IsSolidSpotAngelMoreStart /*flagSolidDataSpotAngel*/ )
            {
                if( deltaPitchAngle <= angleCheck )
                {
                    solidSpotAngel.IsSolidSpotAngelMoreStart = false; // flagSolidDataSpotAngel = false;
                    solidSpotAngel.DiapasonDeltaAngle.End = ( float )cdp.Scan2Dist( scanIndex );

                    var lengthSpot = solidSpotAngel.DiapasonDeltaAngle.End - solidSpotAngel.DiapasonDeltaAngle.Begin;

                    solidSpotAngel.AveDeltaPitchAngleSpot /= solidSpotAngel.СounteDeltaPitchAngleSpot;

                    if (lengthSpot > (float)solidExtentMeters)
                    {
                        solidSpotAngel.SolidSpotMetersType = solidExtentMeters;

                        var solidSpotData = new SolidSpotData()
                        {
                            AveDeltaPitchAngleSpot = solidSpotAngel.AveDeltaPitchAngleSpot,
                            СounteDeltaPitchAngleSpot = solidSpotAngel.СounteDeltaPitchAngleSpot,
                            IsSolidSpotAngelMoreStart = solidSpotAngel.IsSolidSpotAngelMoreStart,
                            SolidSpotAngleType = solidSpotAngel.SolidSpotAngleType,
                            SolidSpotMetersType = solidSpotAngel.SolidSpotMetersType,
                            DiapasonDeltaAngle = solidSpotAngel.DiapasonDeltaAngle
                        };

                        solidSpotsData.Add(solidSpotData);
                    }
                    
                    solidSpotAngel.IsSolidSpotAngelMoreStart = false;
                    solidSpotAngel.SolidSpotMetersType = NavSolidSpotMetersTypes.None;
                    solidSpotAngel.SolidSpotAngleType = NavSolidSpotAngleTypes.None;
                    solidSpotAngel.СounteDeltaPitchAngleSpot = 0;
                    solidSpotAngel.AveDeltaPitchAngleSpot = 0d;
                    solidSpotAngel.DiapasonDeltaAngle = new Range<float>();
                }
                else
                {
                    solidSpotAngel.СounteDeltaPitchAngleSpot++;
                    solidSpotAngel.AveDeltaPitchAngleSpot += deltaPitchAngle;
                }
            }
            else if( deltaPitchAngle > angleCheck )
            {
                solidSpotAngel.DiapasonDeltaAngle.Begin = (float)cdp.Scan2Dist( scanIndex );
                solidSpotAngel.SolidSpotAngleType = solidExtent;
                solidSpotAngel.IsSolidSpotAngelMoreStart = true; // flagSolidDataSpotAngel = true;
            }
        }

        private float WrapAngle(float angle)
        {
            var dif = Math.Abs(angle) % 360;
            if (dif < 0)
                dif = dif + 360;
            else if (dif > 180)
                dif = 360 - dif;

            return dif;
        }

        private unsafe void DiscrepancyBetweenAnglesRun(DataHandle<NavScanData> dataHandle, NavigationInfo navigationInfo, List<SolidSpotData> solidSpotsData, 
            SolidSpotData solidSpotAngelMore03, SolidSpotData solidSpotAngelMore1, SolidSpotData solidSpotAngelMore3, CoordinateDataProviderCrop cdp)

        {
            const float SolidSpotAngelMore03Check = 0.3f; // один из параметров вида непрерывного участка
            const float SolidSpotAngelMore1Check = 1f;   // один из параметров вида непрерывного участка
            const float SolidSpotAngelMore3Check = 3f;   // один из параметров вида непрерывного участка

            var aveDeltaRollAngle  = 0f; // выборочное среднее значение расхождения по углу крена в пропуске.
            var aveDeltaPitchAngle = 0f; // выборочное среднее значение расхождения по углу тангажа в пропуске.

            var value = dataHandle.GetDataPointer( 0, 0 );

            for( var scanIndex = 0; scanIndex < dataHandle.ColCount; scanIndex++ )
            {
                var deltaRollAngle  = Math.Abs(WrapAngle( value->RollAngleAccel - value->RollAngleGyro )); // расхождения по углу крена.
                var deltaPitchAngle = Math.Abs(WrapAngle( value->PitchAngleAccel - value->PitchAngleGyro )); // расхождения по углу тангажа.
                
                if (deltaPitchAngle > 4) // угол тангажа > 4 градусов
                    navigationInfo.PitchAngleAtMovement++;

                if ( deltaRollAngle > navigationInfo.MaxRollAngle)
                    navigationInfo.MaxRollAngle = deltaRollAngle;
                if ( deltaPitchAngle > navigationInfo.MaxPitchAngle)
                    navigationInfo.MaxPitchAngle = deltaPitchAngle;

                aveDeltaRollAngle  += deltaRollAngle;
                aveDeltaPitchAngle += deltaPitchAngle;
                

                SoledSpotAngleCheck(NavSolidSpotAngleTypes.SolidExtentThreeTenthsAngle, NavSolidSpotMetersTypes.SolidExtentEightMeters, scanIndex,
                    deltaPitchAngle, SolidSpotAngelMore03Check, solidSpotAngelMore03, solidSpotsData, cdp);

                SoledSpotAngleCheck(NavSolidSpotAngleTypes.SolidExtentOneAngle, NavSolidSpotMetersTypes.SolidExtentThreeMeters, scanIndex,                                    
                    deltaPitchAngle, SolidSpotAngelMore1Check, solidSpotAngelMore1, solidSpotsData, cdp);

                SoledSpotAngleCheck(NavSolidSpotAngleTypes.SolidExtentThreeAngle, NavSolidSpotMetersTypes.SolidExtentTwoMeters, scanIndex,
                    deltaPitchAngle, SolidSpotAngelMore3Check, solidSpotAngelMore3, solidSpotsData, cdp);
                
                value++;
            }
            aveDeltaRollAngle /= dataHandle.ColCount;
            aveDeltaPitchAngle /= dataHandle.ColCount;
            navigationInfo.AverageRollAngle += aveDeltaRollAngle;
            navigationInfo.AveragePitchAngle += aveDeltaPitchAngle;
        }

        public void CalculateNavigation(Session session, Calculation calculation, Action<object> loggerInfo, Action<Session> updateAction)
        {
            //  Определить тип снаряда
            using (var diagnosticData = new DiagnosticData())
            {
                try
                {
                    if (!diagnosticData.Open(calculation.DataLocation)) return;
                }
                catch (Exception e)
                {
                    loggerInfo?.Invoke(e.Message);
                }
                
                calculation.NavigationInfo.NavType = diagnosticData.GetNavType();

                var navInfo = calculation.NavigationInfo;
                var calcParams = ConstNavCalcParams.GetAllParams().FirstOrDefault(item => item.NavType == calculation.NavigationInfo.NavType);
                var diagData = calculation.DiagDataList.FirstOrDefault(item => item.DataType == DataType.Nav);
                diagData.MaxDistance = diagnosticData.GetDistanceRange(diagData.DataType).End;
                
                switch (navInfo.NavType)
                {
                    case enNavType.Bep:
                        CalculateBepNavigation(session, diagnosticData, calculation, calcParams, updateAction);
                        StandardDeviationBepState(navInfo, calcParams);
                        EarthAngularSpeedRotationBepAdisState(navInfo, calcParams);
                        break;
                    case enNavType.Adis:
                        CalculateAdisNavigation(session, diagnosticData, calculation, calcParams, updateAction);
                        StandardDeviationBinsAdisState(navInfo, calcParams);
                        EarthAngularSpeedRotationBepAdisState(navInfo, calcParams);
                        break;
                    case enNavType.Bins:
                        CalculateBinsNavigation(session, diagnosticData, calculation, calcParams, updateAction);
                        StandardDeviationBinsAdisState(navInfo, calcParams);
                        EarthAngularSpeedRotationBinsState(navInfo, calcParams);
                        DiffLatitudeState(navInfo, calcParams);
                        AverageRollPitchAngleState(navInfo, calcParams);
                        MaxRollPitchAngleState(navInfo, calcParams);
                        PitchAngleAtMovementState(navInfo);
                        break;
                }
                GravAccelState(navInfo, calcParams);
                AccelSumState(navInfo, calcParams);
                AccelMaxState(navInfo, calcParams);
                AngularSpeedSumState(navInfo, calcParams);
                AngularSpeedMaxState(navInfo, calcParams);
                
                diagData.ProcessedDist = diagData.MaxDistance;
            }
        }

        //  BEP
        private void CalculateBepNavigation(Session session, DiagnosticData diagnosticData, Calculation calculation, NavCalcParams constNavCalcParams, Action<Session> updateAction)
        {
            var rangeNavSetup = diagnosticData.GetScanRange(DataType.NavSetup);
            var rangeNav = diagnosticData.GetScanRange(DataType.Nav);
            countRangeNav = 2 * (rangeNavSetup.End - rangeNavSetup.Begin) + rangeNav.End - rangeNav.Begin;
            //  Вычислить S{axj} S{ayj} S{azj} S{ωyj} S{ωzj}
            using (var dataHandle = diagnosticData.GetNavSetupData(rangeNavSetup.Begin, rangeNavSetup.End - rangeNavSetup.Begin))
            {
                DeviceStillness(dataHandle, calculation, rangeNavSetup, constNavCalcParams.ScanTimeLengt);
                updateAction?.Invoke(session);
                //  3.1 Значение ускорение свободного падения на участке стоянки
                //  3.2 Значение угловой скорости вращения Земли на участке стоянки
                CalibrationExhibitionArea(diagnosticData, calculation, dataHandle);
                updateAction?.Invoke(session);
            }
            //  3.3 Значения параметров ускорения в пропуске
            //  3.4 Значения параметров угловой скорости в пропуске
            CalculateParametersInRun(session, diagnosticData, calculation, rangeNav);
            updateAction?.Invoke(session);
        }

        //  ADIS
        private void CalculateAdisNavigation(Session session, DiagnosticData diagnosticData, Calculation calculation, NavCalcParams constNavCalcParams, Action<Session> updateAction)
        {
            var rangeNavSetup = diagnosticData.GetScanRange(DataType.NavSetup);
            var rangeNav = diagnosticData.GetScanRange(DataType.Nav);
            countRangeNav = 2 * (rangeNavSetup.End - rangeNavSetup.Begin) + rangeNav.End - rangeNav.Begin;
            //  Вычислить S{axj} S{ayj} S{azj} S{ωxj} S{ωyj} S{ωzj}
            using (var dataHandle = diagnosticData.GetNavSetupData(rangeNavSetup.Begin, rangeNavSetup.End - rangeNavSetup.Begin))
            {
                DeviceStillness(dataHandle, calculation, rangeNavSetup, constNavCalcParams.ScanTimeLengt);
                updateAction?.Invoke(session);
                //  3.1 Значение ускорение свободного падения на участке стоянки
                //  3.2 Значение угловой скорости вращения Земли на участке стоянки
                CalibrationExhibitionArea(diagnosticData, calculation, dataHandle);
                updateAction?.Invoke(session);
            }
            //  3.3 Значения параметров ускорения в пропуске
            //  3.4 Значения параметров угловой скорости в пропуске
            CalculateParametersInRun(session, diagnosticData, calculation, rangeNav);
            updateAction?.Invoke(session);
        }

        //  BINS
        private void CalculateBinsNavigation(Session session, DiagnosticData diagnosticData, Calculation calculation, NavCalcParams constNavCalcParams, Action<Session> updateAction)
        {
            var rangeNavSetup = diagnosticData.GetScanRange(DataType.NavSetup);
            var scansPerMinute = Convert.ToInt32(60 / constNavCalcParams.ScanTimeLengt);
            var rangeNav = diagnosticData.GetScanRange(DataType.Nav);
            if (scansPerMinute > rangeNavSetup.End - rangeNavSetup.Begin)
            {
                countRangeNav = rangeNav.End - rangeNav.Begin;
                calculation.NavigationInfo.NavSetupTime = (rangeNavSetup.End - rangeNavSetup.Begin) * constNavCalcParams.ScanTimeLengt;
                calculation.NavigationInfo.NavCalcState |= enNavCalcStateTypes.WrongNavSetupTime;
                //Logger.Logger.WarnMessage($"Время выставки прибора меньше минуты. \nВычисления по выставке прибора невозможно продолжить!"); / <-
            }
            else
            {
                countRangeNav = rangeNavSetup.End - rangeNavSetup.Begin + scansPerMinute + rangeNav.End - rangeNav.Begin;
                //  3.1.  Вычислить S{axj} S{ayj} S{azj} S{ωxj} S{ωyj} S{ωzj}
                using (var dataHandle = diagnosticData.GetNavSetupData(rangeNavSetup.Begin, rangeNavSetup.End - rangeNavSetup.Begin))
                {
                    DeviceStillness(dataHandle, calculation, rangeNavSetup, constNavCalcParams.ScanTimeLengt);
                    updateAction?.Invoke(session);
                }
                //  3.2 Значение ускорение свободного падения на участке выставки (последняя минута)
                //  3.3 Значение угловой скорости вращения Земли на участке выставки (последняя минута)
                //  3.4 Значение широты местности на участке выставки (последняя минута)
                using (var dataHandle = diagnosticData.GetNavSetupData(rangeNavSetup.End - scansPerMinute, scansPerMinute)) //  данные за последнюю минуту
                {
                    CalibrationExhibitionArea(diagnosticData, calculation, dataHandle);
                    updateAction?.Invoke(session);
                }
            }

            //  3.5 Значения параметров ускорения в пропуске
            //  3.6 Значения параметров угловой скорости в пропуске
            //  3.7 Расхождение углов, расчитанных по гироскопам и акселерометрам в пропуске
            CalculateParametersInRun(session, diagnosticData, calculation, rangeNav);
            updateAction?.Invoke(session);
        }

        //  33340.  Реализовать алгоритм вычисления дополнительных параметров S{axj},S{ayj},S{azj},S{ωxj},S{ωyj},S{ωzj} на этапе выставки
        private unsafe void DeviceStillness(DataHandle<NavSetupScanData> dataHandle, Calculation calculation, Range<int> scanRange, double scanLength)
        {
            var navInfo = calculation.NavigationInfo;

            var scanCount = scanRange.End - scanRange.Begin;
            navInfo.NavSetupTime = scanCount * scanLength;
            if (navInfo.NavSetupTime < 600 &&
                navInfo.NavType == enNavType.Bins)
                navInfo.NavCalcState |= enNavCalcStateTypes.WrongNavSetupTime;   //  проверка времени выставки

            var accelerationCollectionX = new double[scanCount];
            var accelerationCollectionY = new double[scanCount];
            var accelerationCollectionZ = new double[scanCount];
            double[] angularSpeedCollectionX;
            double[] angularSpeedCollectionY;
            double[] angularSpeedCollectionZ;

            var deltaAngularSpeedX = 0d;
            var deltaAngularSpeedY = 0d;
            var deltaAngularSpeedZ = 0d;
            var accelerationX = 0d;
            var accelerationY = 0d;
            var accelerationZ = 0d;

            var value = dataHandle.GetDataPointer(0, 0);
            const double constant = (double)200 / 3600;
            if (navInfo.NavType == enNavType.Bep)
            {
                angularSpeedCollectionX = new double[scanCount];
                angularSpeedCollectionY = new double[scanCount];
                angularSpeedCollectionZ = new double[scanCount];
                for (var i = 0; i < scanCount; i++)
                {
                    accelerationCollectionX[i] = value->LinearAccelProjX;
                    accelerationCollectionY[i] = value->LinearAccelProjY;
                    accelerationCollectionZ[i] = value->LinearAccelProjZ;
                    accelerationX += value->LinearAccelProjX;
                    accelerationY += value->LinearAccelProjY;
                    accelerationZ += value->LinearAccelProjZ;

                    angularSpeedCollectionX[i] = value->AngularSpeedX;
                    angularSpeedCollectionY[i] = value->AngularSpeedY;
                    angularSpeedCollectionZ[i] = value->AngularSpeedZ;
                    deltaAngularSpeedX += value->AngularSpeedX;
                    deltaAngularSpeedY += value->AngularSpeedY;
                    deltaAngularSpeedZ += value->AngularSpeedZ;
                    calculation.ProgressNavData = (double)i / countRangeNav;
                    value++;
                }
            }
            else
            {
                angularSpeedCollectionX = new double[scanCount - 1];
                angularSpeedCollectionY = new double[scanCount - 1];
                angularSpeedCollectionZ = new double[scanCount - 1];
                for (var i = 0; i < scanCount; i++)
                {
                    accelerationCollectionX[i] = value->LinearAccelProjX;
                    accelerationCollectionY[i] = value->LinearAccelProjY;
                    accelerationCollectionZ[i] = value->LinearAccelProjZ;
                    accelerationX += value->LinearAccelProjX;
                    accelerationY += value->LinearAccelProjY;
                    accelerationZ += value->LinearAccelProjZ;

                    if (i != 0)
                    {
                        var asX = constant * (value->AngularSpeedX - (value - 1)->AngularSpeedX);
                        var asY = constant * (value->AngularSpeedY - (value - 1)->AngularSpeedY);
                        var asZ = constant * (value->AngularSpeedZ - (value - 1)->AngularSpeedZ);
                        angularSpeedCollectionX[i - 1] = asX;
                        angularSpeedCollectionY[i - 1] = asY;
                        angularSpeedCollectionZ[i - 1] = asZ;
                        deltaAngularSpeedX += asX;
                        deltaAngularSpeedY += asY;
                        deltaAngularSpeedZ += asZ;
                    }
                    calculation.ProgressNavData = (double)i / countRangeNav;
                    value++;
                }
            }

            navInfo.SquareDeviationAccelX = StandardDeviation(accelerationCollectionX, accelerationX / scanCount, scanCount);
            navInfo.SquareDeviationAccelY = StandardDeviation(accelerationCollectionY, accelerationY / scanCount, scanCount);
            navInfo.SquareDeviationAccelZ = StandardDeviation(accelerationCollectionZ, accelerationZ / scanCount, scanCount);
            navInfo.SquareDeviationAngularSpeedX = MathHelper.RadianToAngle(StandardDeviation(angularSpeedCollectionX, deltaAngularSpeedX / scanCount, scanCount));
            navInfo.SquareDeviationAngularSpeedY = MathHelper.RadianToAngle(StandardDeviation(angularSpeedCollectionY, deltaAngularSpeedY / scanCount, scanCount));
            navInfo.SquareDeviationAngularSpeedZ = MathHelper.RadianToAngle(StandardDeviation(angularSpeedCollectionZ, deltaAngularSpeedZ / scanCount, scanCount));
        }

        // 32945 Реализовать алгоритм анализа значения угловой скорости вращения Земли на участке выставки для ОПТ(БИНС или БЧЭ) и ПРН-6(ADIS)
        // 32946 Реализовать алгоритм анализа значения широты местности на участке выставки
        // 32883 Реализовать контроль ускорения свободного падения на участке выставки в течение последней минуты перед началом движения
        private unsafe void CalibrationExhibitionArea(DiagnosticData diagnosticData,
                                                      Calculation calculation, 
                                                      DataHandle<NavSetupScanData> dataHandle)
        {
            var navInfo = calculation.NavigationInfo;

            var count = dataHandle.ColCount;
            const double constValue = 7.29219 / 100000;
            
            double averageAngularSpeedX = 0;
            double averageAngularSpeedY = 0;
            double averageAngularSpeedZ = 0;
            double averageAccelerationX = 0;
            double averageAccelerationY = 0;
            double averageAccelerationZ = 0;
            var progressNavData = calculation.ProgressNavData;
            for (var i = 0; i < count; i++)
            {
                var value = *dataHandle.GetDataPointer(0, i);
                averageAngularSpeedX += value.AngularSpeedX / count;
                averageAngularSpeedY += value.AngularSpeedY / count;
                averageAngularSpeedZ += value.AngularSpeedZ / count;
                averageAccelerationX += value.LinearAccelProjX / count;
                averageAccelerationY += value.LinearAccelProjY / count;
                averageAccelerationZ += value.LinearAccelProjZ / count;
                calculation.ProgressNavData = progressNavData + (double)i / countRangeNav;
            }

            //  Значение ускорения свободного падения на участке выставки
            navInfo.GravitationalAcceleration = Math.Sqrt(averageAccelerationX * averageAccelerationX +
                                                                                 averageAccelerationY * averageAccelerationY +
                                                                                 averageAccelerationZ * averageAccelerationZ);
            
            //  Значение угловой скорости вращения Земли на участке выставки
            if (navInfo.NavType == enNavType.Bep)
            {
                navInfo.EarthAngularSpeedRotation = MathHelper.RadianToAngle(Math.Sqrt(averageAngularSpeedY * averageAngularSpeedY +
                                                                                               averageAngularSpeedZ * averageAngularSpeedZ));
            }
            else
            {
                navInfo.EarthAngularSpeedRotation = MathHelper.RadianToAngle(Math.Sqrt(averageAngularSpeedX * averageAngularSpeedX +
                                                                                               averageAngularSpeedY * averageAngularSpeedY +
                                                                                               averageAngularSpeedZ * averageAngularSpeedZ));
            }

            //  Значение широты местности на участке выставки
            if (navInfo.NavType == enNavType.Bins)
            {
                var heelAngle = -Math.Atan(averageAccelerationX / averageAccelerationZ); //  угол крена 
                var pitchAngle = -Math.Atan(averageAccelerationY /
                                            Math.Sqrt(averageAccelerationX * averageAccelerationX +
                                                      averageAccelerationZ * averageAccelerationZ)); //  угол тангажа

                var latitude = Math.Asin(Math.Cos(heelAngle) * Math.Cos(pitchAngle) * MathHelper.AngleToRadian(averageAngularSpeedZ) -
                              Math.Sin(heelAngle) * Math.Cos(pitchAngle) * MathHelper.AngleToRadian(averageAngularSpeedX) -
                              Math.Sin(pitchAngle) * MathHelper.AngleToRadian(averageAngularSpeedY)) / MathHelper.RadianToAngle(constValue);
                navInfo.DifferenceLatitudes =
                    Math.Abs(latitude - calculation.DataOutput.RealLatitude); // разница широт PBI 32946
            }
        }

        private unsafe void CalculateParametersInRun(Session session, DiagnosticData diagnosticData, Calculation calculation, Range<int> rangeNav)
        {
            var bins = false;
            Func<NavScanData, double> сalcGeneralizedAngulSpeed;
            if (calculation.NavigationInfo.NavType == enNavType.Bep)
            {
                сalcGeneralizedAngulSpeed = СalcGeneralizedAngulSpeedTwoParam;
            }
            else
            {
                if (calculation.NavigationInfo.NavType == enNavType.Bins)
                    bins = true;
                сalcGeneralizedAngulSpeed = СalcGeneralizedAngulSpeedThreeParam;
            }

            var scanStep = 60000; //    в сканах

            double angularSpeedsSum = 0;
            double maxAngularSpeed = 0;
            double accelerationsSum = 0;
            double maxAcceleration = 0;

            var solidSpotsData = new List<SolidSpotData>();
            var solidSpotAngelMore03 = new SolidSpotData();
            var solidSpotAngelMore1 = new SolidSpotData();
            var solidSpotAngelMore3 = new SolidSpotData();

            var scanStart = rangeNav.Begin;
            var scanEnd = rangeNav.End;
            var currenScanStart = scanStart;
            var handlerCount = 0;

            var countProgress = 0d;
            var progressNavData = calculation.ProgressNavData;

            while (currenScanStart < scanEnd)
            {
                var currentScanEnd = currenScanStart + scanStep;
                if (currentScanEnd > scanEnd)
                    currentScanEnd = scanEnd;

                var dataHandle = diagnosticData.GetNavData(currenScanStart, currentScanEnd - currenScanStart);
                //  Значения ускорения и угловой скорости

                for (var colIndex = 0; colIndex < dataHandle.ColCount; colIndex++)
                {
                    var value = *dataHandle.GetDataPointer(0, colIndex);
                    var currentAngularSpeed = сalcGeneralizedAngulSpeed(value);

                    var currentAcceleration = Math.Sqrt(value.LinearAccelProjX * value.LinearAccelProjX +
                                                        value.LinearAccelProjY * value.LinearAccelProjY +
                                                        value.LinearAccelProjZ * value.LinearAccelProjZ);

                    angularSpeedsSum += currentAngularSpeed;
                    accelerationsSum += currentAcceleration;
                    if (currentAngularSpeed > maxAngularSpeed)
                        maxAngularSpeed = currentAngularSpeed;
                    if (currentAcceleration > maxAcceleration)
                        maxAcceleration = currentAcceleration;
                    countProgress++;
                    calculation.ProgressNavData = progressNavData + countProgress / countRangeNav;
                }

                //  Расхождение значений углов, рассчитанных по гироскопам и акселерометрам в пропуске
                if (bins)
                {
                    DiscrepancyBetweenAnglesRun(dataHandle,
                        calculation.NavigationInfo,
                        solidSpotsData,
                        solidSpotAngelMore03,
                        solidSpotAngelMore1,
                        solidSpotAngelMore3,
                        diagnosticData.CoordinateDataProvider);
                }
                dataHandle.Dispose();
                handlerCount++;
                currenScanStart = currentScanEnd;
            }

            //matrixUploaderCoefficient.Upload();

            //  1. Проверка критерия ускорения в пропуске 3.5 Значения параметров ускорения в пропуске
            calculation.NavigationInfo.AccelSum = accelerationsSum / (scanEnd - scanStart);
            calculation.NavigationInfo.AccelMax = maxAcceleration;
            //  2. Проверка критерия угловой скорости в пропуске 3.6 Значения параметров угловой скорости в пропуске
            calculation.NavigationInfo.AngularSpeedSum = MathHelper.RadianToAngle(angularSpeedsSum) / (scanEnd - scanStart);
            calculation.NavigationInfo.AngularSpeedMax = MathHelper.RadianToAngle(maxAngularSpeed);

            //  3.Расхождение значений углов, рассчитанных по гироскопам и акселерометрам в пропуске
            if (bins)
            {
                IsSoledSpotCheck(calculation.NavigationInfo, solidSpotsData);

                calculation.NavigationInfo.AverageRollAngle /= handlerCount;
                calculation.NavigationInfo.AveragePitchAngle /= handlerCount;
            }
        }

        private void StandardDeviationBepState(NavigationInfo navInfo, NavCalcParams calcParams)
        {
            if (navInfo.SquareDeviationAccelX < calcParams.SquareDeviationAccelX.Value &&
                navInfo.SquareDeviationAccelY < calcParams.SquareDeviationAccelY.Value &&
                navInfo.SquareDeviationAccelZ < calcParams.SquareDeviationAccelZ.Value &&
                navInfo.SquareDeviationAngularSpeedY < calcParams.SquareDeviationAngularSpeedY.Value &&
                navInfo.SquareDeviationAngularSpeedZ < calcParams.SquareDeviationAngularSpeedZ.Value)
                navInfo.NavCalcState |= enNavCalcStateTypes.StandardDeviations;
        }
        private void StandardDeviationBinsAdisState(NavigationInfo navInfo, NavCalcParams calcParams)
        {
            if (navInfo.SquareDeviationAccelX < calcParams.SquareDeviationAccelX.Value &&
                navInfo.SquareDeviationAccelY < calcParams.SquareDeviationAccelY.Value &&
                navInfo.SquareDeviationAccelZ < calcParams.SquareDeviationAccelZ.Value &&
                navInfo.SquareDeviationAngularSpeedX < calcParams.SquareDeviationAngularSpeedX.Value &&
                navInfo.SquareDeviationAngularSpeedY < calcParams.SquareDeviationAngularSpeedY.Value &&
                navInfo.SquareDeviationAngularSpeedZ < calcParams.SquareDeviationAngularSpeedZ.Value)
                navInfo.NavCalcState |= enNavCalcStateTypes.StandardDeviations;
        }

        private void GravAccelState(NavigationInfo navInfo, NavCalcParams calcParams)
        {
            if (navInfo.GravitationalAcceleration > calcParams.GravAccel.Value - calcParams.GravAccel.Threshold &&
                navInfo.GravitationalAcceleration < calcParams.GravAccel.Value + calcParams.GravAccel.Threshold)
                navInfo.NavCalcState |= enNavCalcStateTypes.GravAccel;
        }

        private void EarthAngularSpeedRotationBinsState(NavigationInfo navInfo, NavCalcParams calcParams)
        {
            if (navInfo.EarthAngularSpeedRotation > calcParams.EarthAngularSpeedRotation.Value - calcParams.EarthAngularSpeedRotation.Threshold &&
                navInfo.EarthAngularSpeedRotation < calcParams.EarthAngularSpeedRotation.Value + calcParams.EarthAngularSpeedRotation.Threshold)
                navInfo.NavCalcState |= enNavCalcStateTypes.EarthAngularSpeedRotation;
        }

        private void EarthAngularSpeedRotationBepAdisState(NavigationInfo navInfo, NavCalcParams calcParams)
        {
            if (navInfo.EarthAngularSpeedRotation < calcParams.EarthAngularSpeedRotation.Value)
                navInfo.NavCalcState |= enNavCalcStateTypes.EarthAngularSpeedRotation;
        }

        private void DiffLatitudeState(NavigationInfo navInfo, NavCalcParams calcParams)
        {
            if (navInfo.DifferenceLatitudes < calcParams.DifferenceLatitudes.Value)
                navInfo.NavCalcState |= enNavCalcStateTypes.DiffLatitude;
        }

        private void AccelSumState(NavigationInfo navInfo, NavCalcParams calcParams)
        {
            if (navInfo.AccelSum > calcParams.AccelSum.Value - calcParams.AccelSum.Threshold &&
                navInfo.AccelSum<calcParams.AccelSum.Value + calcParams.AccelSum.Threshold)
                navInfo.NavCalcState |= enNavCalcStateTypes.AccelSum;
        }

        private void AccelMaxState(NavigationInfo navInfo, NavCalcParams calcParams)
        {
            if (navInfo.AccelMax<calcParams.AccelMax.Value)
                navInfo.NavCalcState |= enNavCalcStateTypes.AccelMax;
        }

        private void AngularSpeedSumState(NavigationInfo navInfo, NavCalcParams calcParams)
        {
            if (navInfo.AngularSpeedSum < calcParams.AngularSpeedSum.Value)
                navInfo.NavCalcState |= enNavCalcStateTypes.AngularSpeedSum;
        }

        private void AngularSpeedMaxState(NavigationInfo navInfo, NavCalcParams calcParams)
        {
            if (navInfo.AngularSpeedMax < calcParams.AngularSpeedMax.Value)
                navInfo.NavCalcState |= enNavCalcStateTypes.AngularSpeedMax;
        }

        private void AverageRollPitchAngleState(NavigationInfo navInfo, NavCalcParams calcParams)
        {
            if (navInfo.AverageRollAngle < calcParams.AverageRollAngle.Value &&
                navInfo.AveragePitchAngle < calcParams.AveragePitchAngle.Value)
                navInfo.NavCalcState |= enNavCalcStateTypes.AverageRollPitchAngle;
        }

        private void MaxRollPitchAngleState(NavigationInfo navInfo, NavCalcParams calcParams)
        {
            if (navInfo.MaxRollAngle < calcParams.MaxRollAngle.Value &&
                navInfo.MaxPitchAngle < calcParams.MaxPitchAngle.Value)
                navInfo.NavCalcState |= enNavCalcStateTypes.MaxRollPitchAngle;
        }

        private void PitchAngleAtMovementState(NavigationInfo navInfo)
        {
            if (navInfo.PitchAngleSection6 != 0 ||
                navInfo.PitchAngleSection3 != 0 ||
                navInfo.PitchAngleSection2 != 0 ||
                navInfo.PitchAngleAtMovement != 0)
                navInfo.NavCalcState |= enNavCalcStateTypes.PitchAngleAtMovement;
        }

        private void IsSoledSpotCheck(NavigationInfo navigationInfo, List<SolidSpotData> solidSpotsData )
        {
            foreach (var solidSpots in solidSpotsData)
            {

                if (solidSpots.AveDeltaPitchAngleSpot > 0.6f &&
                    solidSpots.SolidSpotAngleType == NavSolidSpotAngleTypes.SolidExtentThreeTenthsAngle &&
                    solidSpots.SolidSpotMetersType == NavSolidSpotMetersTypes.SolidExtentEightMeters)
                {
                    navigationInfo.PitchAngleSection6++;
                }
                else if (solidSpots.AveDeltaPitchAngleSpot > 2f &&
                         solidSpots.SolidSpotAngleType == NavSolidSpotAngleTypes.SolidExtentOneAngle &&
                         solidSpots.SolidSpotMetersType == NavSolidSpotMetersTypes.SolidExtentThreeMeters)
                     {
                         navigationInfo.PitchAngleSection3++;
                     }
                else if (solidSpots.AveDeltaPitchAngleSpot > 5f &&
                         solidSpots.SolidSpotAngleType == NavSolidSpotAngleTypes.SolidExtentThreeAngle &&
                         solidSpots.SolidSpotMetersType == NavSolidSpotMetersTypes.SolidExtentTwoMeters)
                     {
                         navigationInfo.PitchAngleSection2++;
                     }
            }
        }

        public void SplitDataTypeRange(double receptionChamberLocal, double triggerChamberLocal, SensorRange sensorRange, List<SensorRange> haltingSensor)
        {
            //  1. (кпп)
            if (sensorRange.Begin <= triggerChamberLocal && sensorRange.End <= triggerChamberLocal || sensorRange.Begin >= receptionChamberLocal && sensorRange.End >= receptionChamberLocal)
            {
                sensorRange.PipeType = enPipeType.Сhamber;
                return;
            }
            //  2.
            if (sensorRange.Begin < triggerChamberLocal && sensorRange.End < receptionChamberLocal)
            {
                // часть до (кпп)
                haltingSensor.Add(new SensorRange { Begin = sensorRange.Begin, End = triggerChamberLocal, PipeType = enPipeType.Сhamber });
                // часть после (лч)
                haltingSensor.Add(new SensorRange { Begin = triggerChamberLocal, End = sensorRange.End, PipeType = enPipeType.Linear });
                haltingSensor.Remove(sensorRange);
                return;
            }
            //  3.
            if (sensorRange.Begin < triggerChamberLocal && sensorRange.End > receptionChamberLocal)
            {
                // часть до (кпп)
                haltingSensor.Add(new SensorRange { Begin = sensorRange.Begin, End = triggerChamberLocal, PipeType = enPipeType.Сhamber });
                // середина (лч)
                haltingSensor.Add(new SensorRange { Begin = triggerChamberLocal, End = receptionChamberLocal, PipeType = enPipeType.Linear });
                // часть после (кпп)
                haltingSensor.Add(new SensorRange { Begin = receptionChamberLocal, End = sensorRange.End, PipeType = enPipeType.Linear });
                haltingSensor.Remove(sensorRange);
                return;
            }
            //  5.
            if (sensorRange.Begin > triggerChamberLocal && sensorRange.End > receptionChamberLocal)
            {
                // часть до (лч)
                haltingSensor.Add(new SensorRange { Begin = sensorRange.Begin, End = receptionChamberLocal, PipeType = enPipeType.Linear });
                // часть после (кпп)
                haltingSensor.Add(new SensorRange { Begin = receptionChamberLocal, End = sensorRange.End, PipeType = enPipeType.Сhamber });
                haltingSensor.Remove(sensorRange);
            }
        }

        /// <summary>
        /// Расчет общего ускорения скорости по углу с тремя параметрами
        /// </summary>
        /// <param name="value">параметры </param>
        /// <returns></returns>
        private double СalcGeneralizedAngulSpeedThreeParam(NavScanData value)
        {
            return Math.Sqrt(value.AngularSpeedX * value.AngularSpeedX +
                             value.AngularSpeedY * value.AngularSpeedY +
                             value.AngularSpeedZ * value.AngularSpeedZ);
        }

        /// <summary>
        /// Расчет общего ускорения скорости по углу с двумя параметрами
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private double СalcGeneralizedAngulSpeedTwoParam(NavScanData value)
        {
            return Math.Sqrt(value.AngularSpeedY * value.AngularSpeedY +
                             value.AngularSpeedZ * value.AngularSpeedZ);
        }

        private double StandardDeviation(double[] valueCollection, double average, int count)
        {
            double buffValue = 0;
            foreach (var value in valueCollection)
            {
                var difference = value - average;
                buffValue += difference * difference / count;
            }
            return Math.Sqrt(buffValue);
        }

        #region parallel calc
        /*
        public bool CalcParallel(Action<Calculation> updateAction, Action<object> loggerInfo, Calculation calculation, DiagData diagData, CancellationTokenSource cts)
        {
            var coreCount = 0;
            foreach (var item in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }

            if (calculation.OmniFilePath == null) return false;
            var diagDataBag = new ConcurrentBag<DiagnosticData>();
            var distStopBag = new ConcurrentBag<double>();
            for (var i = 0; i < coreCount; i++)
            {
                var diagnosticData = new DiagnosticData();
                try
                {
                    if (!diagnosticData.Open(calculation.OmniFilePath))
                        return false;
                }
                catch (Exception e)
                {
                    loggerInfo?.Invoke(e.Message);
                }
                distStopBag.Add(0);
                diagDataBag.Add(diagnosticData);
            }

            try
            {
                diagDataBag.TryTake(out var diagnosticDataFirst);

                var distRange = diagnosticDataFirst.GetDistanceRange(diagData.DataType);
                diagData.StartDist = distRange.Begin;
                diagData.StopDist = distRange.End;
                diagData.DistanceLength = distRange.End - distRange.Begin;
                diagData.SensorCount = diagnosticDataFirst.GetSensorCount(diagData.DataType);

                if (diagData.DataType == DataType.Cd360)
                    diagData.SensorCount /= diagnosticDataFirst.GetCdmDirections().Count;
                diagData.MaxDistance = Math.Round(distRange.End, 3, MidpointRounding.AwayFromZero);

                var diagParams = ConstDiagDataParams.GetAllParams().FirstOrDefault(item => item.DataType.HasFlag(diagData.DataType));
                if (diagParams == null) return false;

                var dist = Math.Max(Math.Round(distRange.Begin, 3, MidpointRounding.AwayFromZero), diagData.ProcessedDist);
                var progress = (int)(dist / 100);

                var distEnd = dist + diagParams.Distance;

                var rangeDistances = new List<DiCore.Lib.NDT.Types.Range<double>>();
                while (diagData.MaxDistance - diagParams.IgnoreAreasCount * diagParams.Distance > distEnd)
                {
                    rangeDistances.Add(new DiCore.Lib.NDT.Types.Range<double>(dist, distEnd));
                    dist = distEnd;
                    distEnd += diagParams.Distance;
                }

                diagDataBag.Add(diagnosticDataFirst);

                Parallel.ForEach(rangeDistances, new ParallelOptions() { MaxDegreeOfParallelism = coreCount }, (range, state) =>
                {
                    if (!Directory.Exists(calculation.SourcePath)) state.Break();
                    if (cts.IsCancellationRequested) state.Break();

                    diagDataBag.TryTake(out var diagnosticData);

                    distStopBag.TryTake(out var distStop);
                    distStop = range.Begin;
                    distStopBag.Add(distStop);
                    switch (diagData.DataType)
                    {
                        case DataType.Cdl:
                            FindCdlHaltingSensorsParallel(diagData, diagnosticData, diagParams, range.Begin, range.End);
                            break;
                        case DataType.Cd360:
                            FindCdmHaltingSensorsParallel((CdmDiagData)diagData, diagnosticData, diagParams, range.Begin, range.End);
                            break;
                        case DataType.Mpm:
                        case DataType.Wm:
                            FindHaltingSensorsParallel(diagData, diagnosticData, diagParams, range.Begin, range.End);
                            break;
                        case DataType.TfiT4:
                        case DataType.TfiT41:
                        case DataType.MflT1:
                        case DataType.MflT11:
                        case DataType.MflT3:
                        case DataType.MflT31:
                        case DataType.MflT32:
                        case DataType.MflT33:
                        case DataType.MflT34:
                            FindMflHaltingSensorsParallel(diagData, diagnosticData, diagParams, range.Begin, range.End);
                            break;
                        case DataType.Ema:
                            FindEmaHaltingSensorsParallel(diagData, diagnosticData, diagParams, range.Begin, range.End);
                            break;
                    }

                    var minStopDist = distStopBag.ToArray().Min();
                    if ((int)(minStopDist / 100f) > progress)
                    {
                        diagData.ProcessedDist = minStopDist;
                        updateAction?.Invoke(calculation);
                        Interlocked.Increment(ref progress);
                    }

                    diagDataBag.Add(diagnosticData);
                });

                diagData.ProcessedDist = diagData.MaxDistance;
                updateAction?.Invoke(calculation);
                diagData.State = true;
                return true;
            }
            finally
            {
                while (diagDataBag.Count > 0)
                {
                    diagDataBag.TryTake(out var diagnosticData);
                    diagnosticData.Dispose();
                }
            }
        }

        private void CombineHaltingSensorsParallel(DiagData diagData, DiagDataParams diagParams, List<int> sensors, double distStart, double distStop)
        {
            lock (diagData)
            {
                var haltingSensors = diagData.HaltingSensors;
                var addRange = new Range<double>(distStart, distStop);
                var distance = double.Epsilon;
                if (diagData.DataType == DataType.Cd360 || diagData.DataType == DataType.Cdc || diagData.DataType == DataType.Cdl)
                    distance = diagParams.Distance;

                foreach (var sensor in sensors)
                {
                    if (haltingSensors.ContainsKey(sensor))
                    {
                        var leftRangeIndex = -1;
                        var rightRangeIndex = -1;
                        var skipAddRange = false;
                        for (var i = 0; i < haltingSensors[sensor].Count; i++)
                        {
                            if (haltingSensors[sensor][i].End >= addRange.End &&
                                haltingSensors[sensor][i].Begin <= addRange.Begin)
                            {
                                skipAddRange = true;
                                continue;
                            }
                            //  смотрим слева от addRange
                            if (MathHelper.IntersectPoints(haltingSensors[sensor][i].End, addRange.Begin, distance))
                                leftRangeIndex = i;
                            //  смотрим справа от addRange
                            else if (MathHelper.IntersectPoints(haltingSensors[sensor][i].Begin, addRange.End, distance))
                                rightRangeIndex = i;
                        }
                        if (leftRangeIndex > -1 && rightRangeIndex == -1)
                        {
                            var rangeLeft = haltingSensors[sensor][leftRangeIndex];
                            rangeLeft.End = addRange.End;
                            haltingSensors[sensor][leftRangeIndex] = rangeLeft;
                        }
                        else if (rightRangeIndex > -1 && leftRangeIndex == -1)
                        {
                            var rangeRight = haltingSensors[sensor][rightRangeIndex];
                            rangeRight.Begin = addRange.Begin;
                            haltingSensors[sensor][rightRangeIndex] = rangeRight;
                        }
                        else if (leftRangeIndex > -1 && rightRangeIndex > -1)
                        {
                            var range = addRange;
                            range.Begin = haltingSensors[sensor][leftRangeIndex].Begin;
                            range.End = haltingSensors[sensor][rightRangeIndex].End;
                            if (rightRangeIndex > leftRangeIndex)
                            {
                                haltingSensors[sensor].RemoveAt(rightRangeIndex);
                                haltingSensors[sensor].RemoveAt(leftRangeIndex);
                            }
                            else
                            {
                                haltingSensors[sensor].RemoveAt(leftRangeIndex);
                                haltingSensors[sensor].RemoveAt(rightRangeIndex);
                            }
                            haltingSensors[sensor].Add(range);
                        }
                        else if (!skipAddRange)
                            haltingSensors[sensor].Add(addRange);
                    }
                    else
                    {
                        haltingSensors[sensor] = new List<Range<double>> { addRange };
                    }
                }
            }
        }

        private void FindCdlHaltingSensorsParallel(DiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, double distStart, double distStop)
        {
            using (var dataHandle = diagnosticData.GetCdlData(distStart, distStop))
            {
                if (dataHandle == null) return;
                var sensors = CalcCdHaltingSensors(dataHandle, diagParams);
                CombineHaltingSensorsParallel(diagData, diagParams, sensors, distStart, distStop);
            }
        }

        private void FindCdmHaltingSensorsParallel(CdmDiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, double distStart, double distStop)
        {
            using (var dataHandle = diagnosticData.GetCdmData(diagData.ToDirection(), distStart, distStop))
            {
                if (dataHandle == null) return;
                var sensors = CalcCdHaltingSensors(dataHandle, diagParams);
                for (var i = 0; i < sensors.Count; i++)
                    sensors[i] = diagnosticData.CdmSensorIndexToSensor(sensors[i], diagData.ToDirection());
                CombineHaltingSensorsParallel(diagData, diagParams, sensors, distStart, distStop);
            }
        }

        private void FindHaltingSensorsParallel(DiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, double distStart, double distStop)
        {
            using (var dataHandle = GetDataHandle(diagnosticData, diagParams.DataType, distStart, distStop))
            {
                if (dataHandle == null) return;
                var sensors = CalcHaltingSensors(dataHandle, diagParams);
                for (var i = 0; i < sensors.Count; i++)
                    sensors[i] = diagnosticData.SensorIndexToSensor(sensors[i], diagData.DataType);
                CombineHaltingSensorsParallel(diagData, diagParams, sensors, distStart, distStop);
            }
        }

        private void FindMflHaltingSensorsParallel(DiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, double distStart, double distStop)
        {
            using (var dataHandle = GetDataHandle(diagnosticData, diagParams.DataType, distStart, distStop))
            {
                if (dataHandle == null) return;
                var sensors = CalcHaltingSensors(dataHandle, diagParams);
                var ranges = CalcCircularHaltingSensors(dataHandle, diagnosticData.DistToDataTypeScan(distStart, diagData.DataType) ?? 0);

                if (ranges.Count > 0)
                {
                    var excludeSensors = new List<int>();
                    for (var sensorIndex = 0; sensorIndex < dataHandle.RowCount; sensorIndex++)
                    {
                        if (!sensors.Contains(sensorIndex))
                            excludeSensors.Add(sensorIndex);
                    }
                    foreach (var range in ranges)
                    {
                        var distBeg = Math.Round(diagnosticData.DataTypeScanToDist(range.Begin, diagData.DataType) ?? 0, 3, MidpointRounding.AwayFromZero);
                        var distEnd = Math.Round(diagnosticData.DataTypeScanToDist(range.End, diagData.DataType) ?? 0, 3, MidpointRounding.AwayFromZero);
                        CombineHaltingSensorsParallel(diagData, diagParams, excludeSensors, distBeg, distEnd);
                    }
                }
                CombineHaltingSensorsParallel(diagData, diagParams, sensors, distStart, distStop);
            }
        }

        private unsafe void FindEmaHaltingSensorsParallel(DiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, double distStart, double distStop)
        {
            using (var dataHandle = diagnosticData.GetEmaData(distStart, distStop))
            {
                if (dataHandle == null) return;
                var scanStart = diagnosticData.DistToDataTypeScan(distStart, diagData.DataType) ?? 0;
                for (var sensorIndex = 0; sensorIndex < dataHandle.RowCount; sensorIndex++)
                {
                    var sensorItemPtr = dataHandle.GetDataPointer(sensorIndex, 0);
                    var nextSensorIndex = (sensorIndex + 1 + dataHandle.RowCount) % dataHandle.RowCount;
                    var nextSensorItemPtr = dataHandle.GetDataPointer(nextSensorIndex, 0);
                    var ranges = CalcEmaHaltingSensors(sensorItemPtr, nextSensorItemPtr, dataHandle.ColCount);
                    var sensor = diagnosticData.SensorIndexToSensor(sensorIndex, diagData.DataType);

                    foreach (var range in ranges)
                    {
                        var distBeg = Math.Round(diagnosticData.DataTypeScanToDist(range.Begin + scanStart, diagData.DataType) ?? 0, 3, MidpointRounding.AwayFromZero);
                        var distEnd = Math.Round(diagnosticData.DataTypeScanToDist(range.End + scanStart, diagData.DataType) ?? 0, 3, MidpointRounding.AwayFromZero);
                        CombineHaltingSensorsParallel(diagData, diagParams, new List<int>(1) { sensor }, distBeg, distEnd);
                    }
                }
            }
        }
        */
        #endregion
    }
}
