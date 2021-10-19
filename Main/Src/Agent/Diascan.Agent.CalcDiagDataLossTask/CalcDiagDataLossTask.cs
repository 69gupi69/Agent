using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Diascan.Agent.ModelDB;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.DiagnosticData;
using CdlCheсking;
using DiCore.Lib.NDT.CoordinateProvider;
using DiCore.Lib.NDT.DataProviders.CDL;
using DiCore.Lib.NDT.DataProviders.CDM;
using DiCore.Lib.NDT.DataProviders.NAV;
using DiCore.Lib.NDT.DataProviders.NAV.NavSetup;
using DiCore.Lib.NDT.Types;
using File = System.IO.File;

namespace Diascan.Agent.CalcDiagDataLossTask
{
    public class DiagDataLossTask
    {
        public List<int> CancellableId;
        private const int DistanceChecking = 1000;
        private int countRangeNav;


        public DiagDataLossTask()
        {
            CancellableId = new List<int>();
        }

        public static void CalcAreas(Calculation calculation)
        {
            if (calculation.DataOutput == null) return;

            calculation.DataOutput.OverallSpeedAreaLdi = 0;
            calculation.DataOutput.OverallAreaLdi = 0;
            calculation.DataOutput.OverallArea = 0;

            var pipeDiameter = calculation.DataOutput.Diameter / 1000f;

            // Расчёт площади ПДИ по скорости

            foreach( var diagData in calculation.DiagDataList )
            {
                if ( diagData.SpeedInfos != null )
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

        public bool Calc(Action<Calculation> updateAction, Calculation calculation, DiagData diagData)
        {
            if (calculation.OmniFilePath == null) return false;

            using (var diagnosticData = new DiagnosticData())
            {
                try
                {
                    if (!diagnosticData.Open(calculation.OmniFilePath)) return false;
                }
                catch (Exception e)
                {
                    Logger.Logger.Info(e.Message);
                }

                var distRange = diagnosticData.GetDistanceRange(diagData.DataType);
                diagData.StartDist = distRange.Begin;
                diagData.EndDist = distRange.End;
                diagData.DistanceLength = distRange.End - distRange.Begin;
                diagData.SensorCount = diagnosticData.GetSensorCount(diagData.DataType);

                if (diagData.DataType == DataType.Cd360)
                    diagData.SensorCount /= diagnosticData.GetCdmDirections().Count;

                diagData.MaxDistance = Math.Round(distRange.End, 3, MidpointRounding.AwayFromZero);

                var dist = Math.Max(Math.Round(distRange.Begin, 3, MidpointRounding.AwayFromZero),
                    diagData.ProcessedDist);

                var progress = (int) (dist / 100);

                var diagParams = ConstDiagDataParams.GetAllParams().FirstOrDefault(item => item.DataType.HasFlag(diagData.DataType));
                if (diagParams == null) return false;

                var distEnd = dist + diagParams.Distance;

                while (diagData.MaxDistance - diagParams.IgnoreAreasCount * diagParams.Distance > distEnd)
                {
                    if (!Directory.Exists(calculation.SourcePath))
                        return false;
                    if (CancellableId.Any(q => q == calculation.Id))
                        return false;
                    
                    switch (diagData.DataType)
                    {
                        case DataType.Cdl:
                            FindCdlHaltingSensors(diagData, diagnosticData, diagParams, dist, distEnd);
                            break;
                        case DataType.Cd360:
                            FindCdmHaltingSensors((CdmDiagData)diagData, diagnosticData, diagParams, dist, distEnd);
                            break;
                        case DataType.Mpm:
                        case DataType.Wm:
                            FindHaltingSensors(diagData, diagnosticData, diagParams, dist, distEnd);
                            break;
                        case DataType.TfiT4:
                        case DataType.TfiT41:
                        case DataType.MflT1:
                        case DataType.MflT11:
                        case DataType.MflT3:
                            FindMflHaltingSensors(diagData, diagnosticData, diagParams, dist, distEnd);
                            break;
                    }
                    
                    dist = distEnd;
                    distEnd += diagParams.Distance;

                    if ((int) (dist / 100f) > progress)
                    {
                        diagData.ProcessedDist = dist;
                        updateAction(calculation);
                        progress++;
                    }
                }
                diagData.ProcessedDist = diagData.MaxDistance;
            }

            updateAction(calculation);
            diagData.State = true;
            return true;
        }

        public void CalcSpeedLdi(Action<Calculation> UpdateAction, Calculation calculation)
        {
            if (calculation.OmniFilePath == null) return;

            using (var dataProvider = new DiagnosticData())
            {
                if (!dataProvider.Open(calculation.OmniFilePath)) return;

                calculation.DataOutput.DistRange =
                    new ModelDB.Range<double>(dataProvider.CoordinateDataProvider.MinDistance, dataProvider.CoordinateDataProvider.MaxDistance);

                if (!(calculation.DataOutput.Diameter > 0))
                    calculation.DataOutput.Diameter = dataProvider.CoordinateDataProvider.CalcParameters.PipeDiameterMm;

                var minRangeLength = calculation.DataOutput.Diameter / 2000;

                var scanStart = dataProvider.CoordinateDataProvider.MinScan;

                var buildInterval = /*(int)(1 / dataProvider.CoordinateDataProvider.CalcParameters.OdoFactor);*/ (int)(calculation.DataOutput.Diameter * 0.5);
                var scanCount = (dataProvider.CoordinateDataProvider.MaxScan - dataProvider.CoordinateDataProvider.MinScan) / buildInterval;
                var speedInfos = dataProvider.CoordinateDataProvider.GetSpeedInfo(scanStart, scanCount, buildInterval);

                var range = new ModelDB.Range<double>();
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
                calculation.State |= CalculationStateTypes.Speed;
                UpdateAction(calculation);
            }
        }

        private void CombineSpeedLdiDistance( List<OverSpeedInfo> calculationSpeedInfos, OverSpeedInfo speedInfo, ModelDB.Range<double> speedThrRange)
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
                lastSpeedInfo.Distance = new ModelDB.Range<double>(lastSpeedInfo.Distance.Begin, speedInfo.Distance.End);
                calculationSpeedInfos[calculationSpeedInfos.Count - 1] = lastSpeedInfo;
            }
            else
                calculationSpeedInfos.Add(speedInfo);
        }

        private void FindCdlHaltingSensors(DiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, double distStart, double distStop)
        {
            var dataHandle = diagnosticData.GetCdlData(distStart, distStop);
            var sensors = CalcCdHaltingSensors(dataHandle, diagParams);

            CombineHaltingSensors(diagData, diagParams, sensors, distStart, distStop);

            dataHandle.Dispose();
        }

        private void FindCdmHaltingSensors(CdmDiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, double distStart, double distStop)
        {
            var dataHandle = diagnosticData.GetCdmData(diagData.ToDirection(), distStart, distStop);
            var sensors = CalcCdHaltingSensors(dataHandle, diagParams);

            for (var i = 0; i < sensors.Count; i++)
                sensors[i] = diagnosticData.CdmSensorIndexToSensor(sensors[i], diagData.ToDirection());
            
            CombineHaltingSensors(diagData, diagParams, sensors, distStart, distStop);

            dataHandle.Dispose();
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
        
        private void FindHaltingSensors(DiagData diagData, DiagnosticData diagnosticData, DiagDataParams diagParams, double distStart, double distStop)
        {
            var dataHandle = GetDataHandle(diagnosticData, diagParams.DataType, distStart, distStop);
            var sensors = CalcHaltingSensors(dataHandle, diagParams);

            for (var i = 0; i < sensors.Count; i++)
                sensors[i] = diagnosticData.SensorIndexToSensor(sensors[i], diagData.DataType);
            
            CombineHaltingSensors(diagData, diagParams, sensors, distStart, distStop);

            dataHandle.Dispose();
        }

        private DataHandle<float> GetDataHandle(DiagnosticData diagData, DataType dataType, double distStart, double distStop)
        {
            try
            {
                switch (dataType)
                {
                    case DataType.Mpm:
                        return diagData.GetMpmData(distStart, distStop);
                    case DataType.TfiT4:
                    case DataType.TfiT41:
                    case DataType.MflT1:
                    case DataType.MflT11:
                    case DataType.MflT3:
                        return diagData.GetMflData(distStart, distStop, dataType);
                    case DataType.Wm:
                        return diagData.GetWmWtData(distStart, distStop);
                    default:
                        return null;
                }
            }
            catch (Exception e)
            {
                //using (StreamWriter writer = new StreamWriter(Application.StartupPath + @"\Log.txt", true))
                //{
                //    writer.WriteLine(e.ToString());
                //    writer.Flush();
                //}
                Logger.Logger.Info(e.Message);
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
            var list = new List<float>();

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
            var dataHandle = GetDataHandle(diagnosticData, diagParams.DataType, distStart, distStop);
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
                    var distBeg = Math.Round(diagnosticData.DataTypeScanToDist(range.Begin, diagData.DataType) ?? 0, 3 , MidpointRounding.AwayFromZero) ;
                    var distEnd = Math.Round(diagnosticData.DataTypeScanToDist(range.End, diagData.DataType) ?? 0, 3, MidpointRounding.AwayFromZero);

                    CombineHaltingSensors(diagData, diagParams, excludeSensors, distBeg, distEnd);
                }
            }

            CombineHaltingSensors(diagData, diagParams, sensors, distStart, distStop);

            dataHandle.Dispose();
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

        private unsafe List<ModelDB.Range<int>> CalcCircularHaltingSensors(DataHandle<float> dataHandle, int scanStart)
        {
            var result = new List<ModelDB.Range<int>>();

            var row = dataHandle.RowCount;
            var col = dataHandle.ColCount;
            var scanCount = 0;
            var beginScan = scanStart;

            for (var scanIndex = 0; scanIndex < col; scanIndex++)
            {
                var mflSensorCount = 0;

                for (var sensorIndex = 0; sensorIndex < row; sensorIndex++)
                {
                    var dataPtr = dataHandle.GetDataPointer(sensorIndex, scanIndex);
                    if (MathHelper.TestFloatEquals(*dataPtr, 0f))
                        mflSensorCount++;
                    else
                        break;
                }

                if (mflSensorCount == row && col > scanIndex + 1)
                    scanCount++;
                else if (scanCount == 0)
                    beginScan++;
                else
                {
                    if (col == scanIndex + 1)
                        scanCount++;

                    result.Add(new ModelDB.Range<int>(beginScan, beginScan + scanCount));

                    beginScan = scanStart + scanIndex;
                    scanCount = 0;
                }
            }
            return result;
        }

        private void CombineHaltingSensors(DiagData diagData, DiagDataParams diagParams, List<int> sensors, double distStart, double distStop)
        {
            var haltingSensors = diagData.HaltingSensors;
            var addRange = new ModelDB.Range<double>(distStart, distStop);

            foreach (var sensor in sensors)
            {
                if (haltingSensors.ContainsKey(sensor))
                {
                    var lastRange = haltingSensors[sensor].LastOrDefault();

                    if (MathHelper.TestDoubleEquals(lastRange.End, addRange.Begin))
                    {
                        lastRange.End = addRange.End;
                        haltingSensors[sensor][haltingSensors[sensor].Count - 1] = lastRange;
                    }
                    else if ((diagData.DataType == DataType.Cd360 || diagData.DataType == DataType.Cdc || diagData.DataType == DataType.Cdl) && 
                             MathHelper.TestFloatEquals(lastRange.End + diagParams.Distance, addRange.Begin))
                    {
                        lastRange.End = addRange.End;
                        haltingSensors[sensor][haltingSensors[sensor].Count - 1] = lastRange;
                    }
                    else
                        haltingSensors[sensor].Add(addRange);
                }
                else
                    haltingSensors[sensor] = new List<ModelDB.Range<double>> { addRange };
            }
        }

        public void CheckCdTail(Calculation calculation, DiagData diagData, Action<Calculation> updateAction)
        {
            using (var diagnosticData = new DiagnosticData())
            {
                try
                {
                    if (!diagnosticData.Open(calculation.OmniFilePath)) return;
                }
                catch (Exception e)
                {
                    Logger.Logger.Info(e.Message);
                }

                if (!diagnosticData.AvailableDataTypes.HasFlag(diagData.DataType) || calculation.DataOutput == null) return;

                var cdSeamChecker = new CdsCheking(calculation.DataOutput.Diameter) 
                {
                    OmniFile = calculation.DataOutput.WorkItemName
                };

                var distRanges = GetDistRanges(diagnosticData, diagData.DataType);
                const double readDistStep = 20d;
                var distRangeIndex = -1;

                foreach (var distRange in distRanges)
                {
                    distRangeIndex++;
                    if (calculation.Helper.CdTailDistProgress > distRange.End) continue;
                    var distStart = Math.Max(distRange.Begin, calculation.Helper.CdTailDistProgress);
                    var distStop = distStart + readDistStep;

                    var frames = new List<Rect>();

                    while (!MathHelper.TestDoubleEquals(distRange.End, distStart))
                    {
                        if (CancellableId.Any(q => q == calculation.Id))
                            return;

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
                        calculation.Helper.ProgressCdlTail = $"{Math.Round(progressPercent, 2)}%";

                        distStart = distStop;

                        if (distRange.End - distStop < readDistStep)
                            distStop += distRange.End - distStop;
                        else
                            distStop += readDistStep;

                        calculation.Helper.CdTailDistProgress = distStart;
                    }

                    //предполагаемое количество рамок на участке данных по дистанции
                    var supposedFrameCount = GetSupposedFrameCount(distRange.Length());
                    calculation.FramesTypeCds |= frames.Count >= supposedFrameCount;
                    calculation.Frames.AddRange(frames);

                    calculation.Helper.CdTailDistProgress = distRange.End;

                    updateAction(calculation);
                }
                calculation.State |= CalculationStateTypes.CdlTail;
                updateAction(calculation);
            }
        }

        private int GetSupposedFrameCount(double distLength)
        {
            const int frameCount = 20;
            return (int)(frameCount * Math.Round(distLength / 1000f, 3));
        }

        private List<DiCore.Lib.NDT.Types.Range<double>> GetDistRanges(DiagnosticData diagnosticData, DataType dataType)
        {
            var distRange = diagnosticData.GetDistanceRange(dataType);
            var distLength = distRange.End - distRange.Begin;

            if (distLength > 3000 && distLength <= 5000)
            {
                distRange.End = 3000;
                return new List<DiCore.Lib.NDT.Types.Range<double>>() { distRange };
            }

            if (distLength > 5000)
            {
                var dr1 = new DiCore.Lib.NDT.Types.Range<double>(100, 1100);
                var dr2 = new DiCore.Lib.NDT.Types.Range<double>(distLength / 2, distLength / 2 + 1000);
                var dr3 = new DiCore.Lib.NDT.Types.Range<double>(distLength - 1100, distRange.End - 100);
                return new List<DiCore.Lib.NDT.Types.Range<double>>() { dr1, dr2, dr3 };
            }

            return new List<DiCore.Lib.NDT.Types.Range<double>>() { distRange };
        }

        private Tuple<DataHandleCdl, CdSensorInfo[]> PrepareCdData(DiagnosticData diagnosticData, double distStart, double distStop, DiagData diagData)
        {
            switch (diagData.DataType)
            {
                case DataType.Cd360:
                    return PrepareCdmData(diagnosticData, ((CdmDiagData)diagData).ToDirection(), distStart, distStop);
                case DataType.Cdl:
                    return PrepareCdlData(diagnosticData, distStart, distStop);
                default:
                    return null;
            }
        }

        private unsafe Tuple<DataHandleCdl, CdSensorInfo[]> PrepareCdmData(DiagnosticData diagnosticData, Direction direction, double distStart, double distStop)
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

        private bool StringToDataType(string strType, DiagData diagData)
        {
            if (diagData is DiagData && strType.ToLower().Contains(diagData.DataType.ToString().ToLower()))
                return true;

            if (diagData is CdmDiagData cdmDiagData && strType.ToLower().Contains(cdmDiagData.DirectionName.ToString().ToLower()))
                return true;

            return false;
        }

        public void NearSensors(IEnumerable<DiagData> diagDataList, List<RestartCriterion> restartCriterion, int nearSensorCount)
        {
            foreach (var diagData in diagDataList)
            {
                var check = CheckDataTypecheck(diagData.HaltingSensors, diagData.SensorCount, DistanceChecking, nearSensorCount);
                if (check)
                {
                    var criterion =
                        $"Количество смежных сбойных датчиков {nearSensorCount} и более протяженностью от {DistanceChecking / 1000} - го км";
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

        private bool CheckDataTypecheck(Dictionary<int, List<ModelDB.Range<double>>> haltingSensors, int sensorCount, double distance, int nearSensorCount)
        {
            var nearRangeSensors = NearRangeSensors(haltingSensors, sensorCount, nearSensorCount);
            foreach (var sensors in nearRangeSensors)
            {
                for (var i = 0; i < sensors.Count; i++)
                {
                    var firstSensorDistances = new Dictionary<ModelDB.Range<double>, List<int>>();
                    foreach (var sensor in haltingSensors[sensors[i]])
                    {
                        if (sensor.End - sensor.Begin < distance) continue;
                        firstSensorDistances.Add(sensor, new List<int>() { sensors[i] });
                    }

                    for (var j = i + 1; j < sensors.Count; j++)
                    {
                        var nextSensorDistances = haltingSensors[sensors[j]];
                        var buff = new Dictionary<ModelDB.Range<double>, List<int>>();
                        foreach (var firstSensorDistance in firstSensorDistances)
                        {
                            foreach (var nextSensorDistance in nextSensorDistances)
                            {
                                if (nextSensorDistance.End - nextSensorDistance.Begin < distance) continue;
                                if (firstSensorDistance.Key.End < nextSensorDistance.Begin ||
                                    firstSensorDistance.Key.Begin > nextSensorDistance.End) continue;
                                var min = Math.Min(firstSensorDistance.Key.End, nextSensorDistance.End);
                                var max = Math.Max(firstSensorDistance.Key.Begin, nextSensorDistance.Begin);
                                if (min - max < distance) continue;
                                var range = new ModelDB.Range<double>(max, min);
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

        private List<List<int>> NearRangeSensors(Dictionary<int, List<ModelDB.Range<double>>> haltingSensors, int sensorCount, int nearSensorCount)
        {
            var nearRangeSensors = new List<List<int>>();
            var buff = new List<int>();
            var sensors = haltingSensors.Keys.OrderBy(q => q).ToList();
            var count = 0;
            var lastSensor = sensorCount - 1;

            for (var i = 0; i < sensors.Count(); i++)
            {
                if (sensors[i] == lastSensor && sensors[0] == 0)
                {
                    for (var j = 0; j < sensors.Count(); j++)
                    {
                        if (haltingSensors.ContainsKey(sensors[j] + 1))
                        {
                            if (!buff.Contains(sensors[j]))
                            {
                                buff.Add(sensors[j]);
                                count++;
                            }
                            if (!buff.Contains(sensors[j] + 1))
                            {
                                buff.Add(sensors[j + 1]);
                                count++;
                            }
                        }
                        else
                        {
                            if (nearRangeSensors.Count > 0)
                            {
                                if (nearRangeSensors[0][0] == buff[count - 1 - j])
                                    nearRangeSensors.RemoveAt(0);
                                break;
                            }
                        }
                    }
                }
                if (haltingSensors.ContainsKey(sensors[i] + 1))
                {
                    if (!buff.Contains(sensors[i]))
                    {
                        buff.Add(sensors[i]);
                        count++;
                    }
                    if (!buff.Contains(sensors[i] + 1))
                    {
                        buff.Add(sensors[i + 1]);
                        count++;
                    }
                }
                else
                {
                    if (count < nearSensorCount)
                        buff = new List<int>();
                    else
                    {
                        nearRangeSensors.Add(buff);
                        buff = new List<int>();
                    }
                    count = 0;
                }
            }
            return nearRangeSensors;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="solidExtent"></param>
        /// <param name="scanIndex"></param>
        /// <param name="deltaPitchAngle"></param>
        /// <param name="angleCheck"></param>
        /// <param name="solidSpotAngel"></param>
        /// <param name="solidSpotsData"></param>
        /// <param name="coordinateDataProvider"></param>
        private void SoledSpotAngleCheck(/*bool flagSolidDataSpotAngel,*/
                                         NavSolidSpotAngleTypes solidExtent,
                                         NavSolidSpotMetersTypes solidExtentMeters,
                                         int scanIndex,
                                         float deltaPitchAngle,
                                         float angleCheck,
                                         SolidSpotData solidSpotAngel,
                                         List<SolidSpotData> solidSpotsData,
                                         CoordinateDataProviderCrop coordinateDataProvider)
        {
            if( solidSpotAngel.IsSolidSpotAngelMoreStart /*flagSolidDataSpotAngel*/ )
            {
                if( deltaPitchAngle <= angleCheck )
                {
                    solidSpotAngel.IsSolidSpotAngelMoreStart = false; // flagSolidDataSpotAngel = false;
                    solidSpotAngel.DiapasonDeltaAngle.End = ( float )coordinateDataProvider.Scan2Dist( scanIndex );

                    var lengthSpot = solidSpotAngel.DiapasonDeltaAngle.End - solidSpotAngel.DiapasonDeltaAngle.Begin;

                    solidSpotAngel.AveDeltaPitchAngleSpot /= solidSpotAngel.СounteDeltaPitchAngleSpot;

                    if (lengthSpot > (float)solidExtentMeters)
                    {
                        solidSpotAngel.SolidSpotMetersType = solidExtentMeters;
                        solidSpotsData.Add( new SolidSpotData(){ AveDeltaPitchAngleSpot    = solidSpotAngel.AveDeltaPitchAngleSpot,
                                                                 СounteDeltaPitchAngleSpot = solidSpotAngel.СounteDeltaPitchAngleSpot,
                                                                 IsSolidSpotAngelMoreStart = solidSpotAngel.IsSolidSpotAngelMoreStart,
                                                                 SolidSpotAngleType        = solidSpotAngel.SolidSpotAngleType,
                                                                 SolidSpotMetersType       = solidSpotAngel.SolidSpotMetersType,
                                                                 DiapasonDeltaAngle        = solidSpotAngel.DiapasonDeltaAngle } );
                    }
                    
                    solidSpotAngel.IsSolidSpotAngelMoreStart = false;
                    solidSpotAngel.SolidSpotMetersType       = NavSolidSpotMetersTypes.None;
                    solidSpotAngel.SolidSpotAngleType        = NavSolidSpotAngleTypes.None;
                    solidSpotAngel.СounteDeltaPitchAngleSpot = 0;
                    solidSpotAngel.AveDeltaPitchAngleSpot    = 0d;
                    solidSpotAngel.DiapasonDeltaAngle        = new ModelDB.Range<float>();
                }
                else
                {
                    solidSpotAngel.СounteDeltaPitchAngleSpot++;
                    solidSpotAngel.AveDeltaPitchAngleSpot += deltaPitchAngle;
                }
            }
            else if( deltaPitchAngle > angleCheck )
            {
                solidSpotAngel.DiapasonDeltaAngle.Begin = (float)coordinateDataProvider.Scan2Dist( scanIndex );
                solidSpotAngel.SolidSpotAngleType = solidExtent;
                solidSpotAngel.IsSolidSpotAngelMoreStart = true; // flagSolidDataSpotAngel = true;
            }
        }


        public float WrapAngle(float angle)
        {
            var dif = Math.Abs(angle) % 360;
            if (dif < 0)
                dif = dif + 360;
            else if (dif > 180)
                dif = 360 - dif;

            return dif;
        }


        private unsafe void DiscrepancyBetweenAnglesRun(DataHandle<NavScanData> dataHandle,
                                                        NavigationInfo navigationInfo,
                                                        List<SolidSpotData> solidSpotsData,
                                                        SolidSpotData solidSpotAngelMore03,
                                                        SolidSpotData solidSpotAngelMore1 ,
                                                        SolidSpotData solidSpotAngelMore3 ,
                                                        CoordinateDataProviderCrop coordinateDataProvider/*, MatrixUploader matrixUploaderCoefficient*/)

        {
            const float SolidSpotAngelMore03Check = 0.3f; // один из параметров вида непрерывного участка
            const float SolidSpotAngelMore1Check  = 1f;   // один из параметров вида непрерывного участка
            const float SolidSpotAngelMore3Check  = 3f;   // один из параметров вида непрерывного участка

            var deltaRollAngle  = 0f; /// расхождения по углу крена.
            var deltaPitchAngle = 0f; /// расхождения по углу тангажа.

            var aveDeltaRollAngle  = 0f; /// выборочное среднее значение расхождения по углу крена в пропуске.
            var aveDeltaPitchAngle = 0f; /// выборочное среднее значение расхождения по углу тангажа в пропуске.

            var value = dataHandle.GetDataPointer( 0, 0 );

            for( var scanIndex = 0; scanIndex < dataHandle.ColCount; scanIndex++ )
            {
                deltaRollAngle  = Math.Abs(WrapAngle( value->RollAngleAccel - value->RollAngleGyro ));   // Угол крена по акселерометрам - Угол крена по гироскопам
                deltaPitchAngle = Math.Abs(WrapAngle( value->PitchAngleAccel - value->PitchAngleGyro )); // Угол тангажа по акселерометрам - Угол тангажа по гироскопам
                
                if (deltaPitchAngle > 4) // угол тангажа > 4 градусов
                    navigationInfo.PitchAngleAtMovement++;

                if ( deltaRollAngle > navigationInfo.MaxRollAngle)
                    navigationInfo.MaxRollAngle = deltaRollAngle;
                if ( deltaPitchAngle > navigationInfo.MaxPitchAngle)
                    navigationInfo.MaxPitchAngle = deltaPitchAngle;

                aveDeltaRollAngle  += deltaRollAngle;
                aveDeltaPitchAngle += deltaPitchAngle;



                SoledSpotAngleCheck(NavSolidSpotAngleTypes.SolidExtentThreeTenthsAngle,
                                    NavSolidSpotMetersTypes.SolidExtentEightMeters,
                                    scanIndex,
                                    deltaPitchAngle,
                                    SolidSpotAngelMore03Check,
                                    solidSpotAngelMore03,
                                    solidSpotsData,
                                    coordinateDataProvider );

                SoledSpotAngleCheck(NavSolidSpotAngleTypes.SolidExtentOneAngle,
                                    NavSolidSpotMetersTypes.SolidExtentThreeMeters,
                                    scanIndex,
                                    deltaPitchAngle,
                                    SolidSpotAngelMore1Check,
                                    solidSpotAngelMore1,
                                    solidSpotsData,
                                    coordinateDataProvider );

                SoledSpotAngleCheck(NavSolidSpotAngleTypes.SolidExtentThreeAngle,
                                    NavSolidSpotMetersTypes.SolidExtentTwoMeters,
                                    scanIndex,
                                    deltaPitchAngle,
                                    SolidSpotAngelMore3Check,
                                    solidSpotAngelMore3,
                                    solidSpotsData,
                                    coordinateDataProvider);


                value++;
            }
            aveDeltaRollAngle /= dataHandle.ColCount;
            aveDeltaPitchAngle /= dataHandle.ColCount;
            navigationInfo.AverageRollAngle += aveDeltaRollAngle;
            navigationInfo.AveragePitchAngle += aveDeltaPitchAngle;
        }

        public void CalculateNavigation(Calculation calculation)
        {
            //  Определить тип снаряда
            using (var diagnosticData = new DiagnosticData())
            {
                try
                {
                    if (!diagnosticData.Open(calculation.OmniFilePath)) return;
                }
                catch (Exception e)
                {
                    Logger.Logger.Info(e.Message);
                }
                
                calculation.NavigationInfo.NavType = diagnosticData.GetNavType();
                var calcParams = ConstNavCalcParams.GetAllParams().FirstOrDefault(item => item.NavType == calculation.NavigationInfo.NavType);

                var diagData = calculation.DiagDataList.First(item => item.DataType == DataType.Nav);
                diagData.MaxDistance = diagnosticData.GetDistanceRange(diagData.DataType).End;
                
                switch (calculation.NavigationInfo.NavType)
                {
                    case enNavType.Bep:
                        CalculateBepNavigation(diagnosticData, calculation, calcParams);
                        StandardDeviationBepState(calculation, calcParams);
                        EarthAngularSpeedRotationBepAdisState(calculation, calcParams);
                        break;
                    case enNavType.Adis:
                        CalculateAdisNavigation(diagnosticData, calculation, calcParams);
                        StandardDeviationBinsAdisState(calculation, calcParams);
                        EarthAngularSpeedRotationBepAdisState(calculation, calcParams);
                        break;
                    case enNavType.Bins:
                        CalculateBinsNavigation(diagnosticData, calculation, calcParams);
                        StandardDeviationBinsAdisState(calculation, calcParams);
                        EarthAngularSpeedRotationBinsState(calculation, calcParams);
                        DiffLatitudeState(calculation, calcParams);
                        AverageRollPitchAngleStat(calculation, calcParams);
                        MaxRollPitchAngleStat(calculation, calcParams);
                        PitchAngleAtMovementState(calculation, calcParams);
                        break;
                }

                GravAccelState(calculation, calcParams);
                AccelSumState(calculation, calcParams);
                AccelMaxState(calculation, calcParams);
                AngularSpeedSumState(calculation, calcParams);
                AngularSpeedMaxState(calculation, calcParams);
                
                diagData.ProcessedDist = diagData.MaxDistance;
            }
        }

        //  BEP
        private unsafe void CalculateBepNavigation(DiagnosticData diagnosticData, Calculation calculation, NavCalcParams constNavCalcParams)
        {
            var rangeNavSetup = diagnosticData.GetScanRange(DataType.NavSetup);
            var rangeNav = diagnosticData.GetScanRange(DataType.Nav);
            countRangeNav = 2 * (rangeNavSetup.End - rangeNavSetup.Begin) + rangeNav.End - rangeNav.Begin;
            //  Вычислить S{axj} S{ayj} S{azj} S{ωyj} S{ωzj}
            var dataHandle = diagnosticData.GetNavSetupData(rangeNavSetup.Begin, rangeNavSetup.End - rangeNavSetup.Begin);
            DeviceStillness(dataHandle, calculation, rangeNavSetup, constNavCalcParams.ScanTimeLengt);
            //  3.1 Значение ускорение свободного падения на участке стоянки
            //  3.2 Значение угловой скорости вращения Земли на участке стоянки
            CalibrationExhibitionArea(diagnosticData, calculation, dataHandle);
            dataHandle.Dispose();
            //  3.3 Значения параметров ускорения в пропуске
            //  3.4 Значения параметров угловой скорости в пропуске
            CalculateParametersInRun(diagnosticData, calculation, rangeNav);
        }

        //  ADIS
        private void CalculateAdisNavigation(DiagnosticData diagnosticData, Calculation calculation, NavCalcParams constNavCalcParams)
        {
            var rangeNavSetup = diagnosticData.GetScanRange(DataType.NavSetup);
            var rangeNav = diagnosticData.GetScanRange(DataType.Nav);
            countRangeNav = 2 * (rangeNavSetup.End - rangeNavSetup.Begin) + rangeNav.End - rangeNav.Begin;
            //  Вычислить S{axj} S{ayj} S{azj} S{ωxj} S{ωyj} S{ωzj}
            var dataHandle = diagnosticData.GetNavSetupData(rangeNavSetup.Begin, rangeNavSetup.End - rangeNavSetup.Begin);
            DeviceStillness(dataHandle, calculation, rangeNavSetup, constNavCalcParams.ScanTimeLengt);
            //  3.1 Значение ускорение свободного падения на участке стоянки
            //  3.2 Значение угловой скорости вращения Земли на участке стоянки
            CalibrationExhibitionArea(diagnosticData, calculation, dataHandle);
            dataHandle.Dispose();
            //  3.3 Значения параметров ускорения в пропуске
            //  3.4 Значения параметров угловой скорости в пропуске
            CalculateParametersInRun(diagnosticData, calculation, rangeNav);
        }

        //  BINS
        private void CalculateBinsNavigation(DiagnosticData diagnosticData, Calculation calculation, NavCalcParams constNavCalcParams)
        {
            var scansPerMinute = Convert.ToInt32(60/ constNavCalcParams.ScanTimeLengt);
            var rangeNavSetup = diagnosticData.GetScanRange(DataType.NavSetup);
            var rangeNav = diagnosticData.GetScanRange(DataType.Nav);
            countRangeNav = rangeNavSetup.End - rangeNavSetup.Begin + scansPerMinute + rangeNav.End - rangeNav.Begin;
            //  3.1.  Вычислить S{axj} S{ayj} S{azj} S{ωxj} S{ωyj} S{ωzj}
            var dataHandle = diagnosticData.GetNavSetupData(rangeNavSetup.Begin, rangeNavSetup.End - rangeNavSetup.Begin);
            DeviceStillness(dataHandle, calculation, rangeNavSetup, constNavCalcParams.ScanTimeLengt);
            dataHandle.Dispose();
            //  3.2 Значение ускорение свободного падения на участке выставки (последняя минута)
            //  3.3 Значение угловой скорости вращения Земли на участке выставки (последняя минута)
            //  3.4 Значение широты местности на участке выставки (последняя минута)
            dataHandle = diagnosticData.GetNavSetupData(rangeNavSetup.End - scansPerMinute, scansPerMinute);  //  данные за последнюю минуту
            CalibrationExhibitionArea(diagnosticData, calculation, dataHandle);
            dataHandle.Dispose();
            //  3.5 Значения параметров ускорения в пропуске
            //  3.6 Значения параметров угловой скорости в пропуске
            //  3.7 Расхождение углов, расчитанных по гироскопам и акселерометрам в пропуске
            CalculateParametersInRun(diagnosticData, calculation, rangeNav);
        }

        //  33340.  Реализовать алгоритм вычисления дополнительных параметров S{axj},S{ayj},S{azj},S{ωxj},S{ωyj},S{ωzj} на этапе выставки
        private unsafe void DeviceStillness(DataHandle<NavSetupScanData> dataHandle,
                                            Calculation calculation,
                                            DiCore.Lib.NDT.Types.Range<int> scanRange,
                                            double scanLength)
        {
            var scanCount = scanRange.End - scanRange.Begin;
            calculation.NavigationInfo.NavSetupTime = scanCount * scanLength;
            if (calculation.NavigationInfo.NavSetupTime < 600 &&
                calculation.NavigationInfo.NavType == enNavType.Bins)
                calculation.NavigationInfo.NavCalcState |= NavCalcStateTypes.WrongNavSetupTime;   //  проверка времени выставки

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
            if (calculation.NavigationInfo.NavType == enNavType.Bep)
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
                    calculation.Helper.ProgressNavData += (double)i / countRangeNav;
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
                    calculation.Helper.ProgressNavData = (double)i / countRangeNav;
                    value++;
                }
            }

            calculation.NavigationInfo.SquareDeviationAccelX = StandardDeviation(accelerationCollectionX, accelerationX / scanCount, scanCount);
            calculation.NavigationInfo.SquareDeviationAccelY = StandardDeviation(accelerationCollectionY, accelerationY / scanCount, scanCount);
            calculation.NavigationInfo.SquareDeviationAccelZ = StandardDeviation(accelerationCollectionZ, accelerationZ / scanCount, scanCount);
            calculation.NavigationInfo.SquareDeviationAngularSpeedX = RadianToAngle(StandardDeviation(angularSpeedCollectionX, deltaAngularSpeedX / scanCount, scanCount));
            calculation.NavigationInfo.SquareDeviationAngularSpeedY = RadianToAngle(StandardDeviation(angularSpeedCollectionY, deltaAngularSpeedY / scanCount, scanCount));
            calculation.NavigationInfo.SquareDeviationAngularSpeedZ = RadianToAngle(StandardDeviation(angularSpeedCollectionZ, deltaAngularSpeedZ / scanCount, scanCount));
        }

        // 32945 Реализовать алгоритм анализа значения угловой скорости вращения Земли на участке выставки для ОПТ(БИНС или БЧЭ) и ПРН-6(ADIS)
        // 32946 Реализовать алгоритм анализа значения широты местности на участке выставки
        // 32883 Реализовать контроль ускорения свободного падения на участке выставки в течение последней минуты перед началом движения
        private unsafe void CalibrationExhibitionArea(DiagnosticData diagnosticData,
                                                      Calculation calculation, 
                                                      DataHandle<NavSetupScanData> dataHandle)
        {
            var count = dataHandle.ColCount;
            const double constValue = 7.29219 / 100000;
            
            double averageAngularSpeedX = 0;
            double averageAngularSpeedY = 0;
            double averageAngularSpeedZ = 0;
            double averageAccelerationX = 0;
            double averageAccelerationY = 0;
            double averageAccelerationZ = 0;
            var progressNavData = calculation.Helper.ProgressNavData;
            for (var i = 0; i < count; i++)
            {
                var value = *dataHandle.GetDataPointer(0, i);
                averageAngularSpeedX += value.AngularSpeedX / count;
                averageAngularSpeedY += value.AngularSpeedY / count;
                averageAngularSpeedZ += value.AngularSpeedZ / count;
                averageAccelerationX += value.LinearAccelProjX / count;
                averageAccelerationY += value.LinearAccelProjY / count;
                averageAccelerationZ += value.LinearAccelProjZ / count;
                calculation.Helper.ProgressNavData = progressNavData + (double)i / countRangeNav;
            }

            //  Значение ускорения свободного падения на участке выставки
            calculation.NavigationInfo.GravitationalAcceleration = Math.Sqrt(averageAccelerationX * averageAccelerationX +
                                                                                 averageAccelerationY * averageAccelerationY +
                                                                                 averageAccelerationZ * averageAccelerationZ);
            
            //  Значение угловой скорости вращения Земли на участке выставки
            if (calculation.NavigationInfo.NavType == enNavType.Bep)
            {
                calculation.NavigationInfo.EarthAngularSpeedRotation = RadianToAngle(Math.Sqrt(averageAngularSpeedY * averageAngularSpeedY +
                                                                                               averageAngularSpeedZ * averageAngularSpeedZ));
            }
            else
            {
                calculation.NavigationInfo.EarthAngularSpeedRotation = RadianToAngle(Math.Sqrt(averageAngularSpeedX * averageAngularSpeedX +
                                                                                               averageAngularSpeedY * averageAngularSpeedY +
                                                                                               averageAngularSpeedZ * averageAngularSpeedZ));
            }

            //  Значение широты местности на участке выставки
            if (calculation.NavigationInfo.NavType == enNavType.Bins)
            {
                var heelAngle = -Math.Atan(averageAccelerationX / averageAccelerationZ); //  угол крена 
                var pitchAngle = -Math.Atan(averageAccelerationY /
                                            Math.Sqrt(averageAccelerationX * averageAccelerationX +
                                                      averageAccelerationZ * averageAccelerationZ)); //  угол тангажа
                var latitude =
                    Math.Asin(Math.Cos(heelAngle) * Math.Cos(pitchAngle) * AngleToRadian(averageAngularSpeedZ) -
                              Math.Sin(heelAngle) * Math.Cos(pitchAngle) * AngleToRadian(averageAngularSpeedX) -
                              Math.Sin(pitchAngle) * AngleToRadian(averageAngularSpeedY)) / RadianToAngle(constValue);
                calculation.NavigationInfo.DifferenceLatitudes =
                    Math.Abs(latitude - calculation.DataOutput.RealLatitude); // разница широт PBI 32946
            }
        }

        private unsafe void CalculateParametersInRun(DiagnosticData diagnosticData, Calculation calculation, DiCore.Lib.NDT.Types.Range<int> rangeNav)
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
            var progressNavData = calculation.Helper.ProgressNavData;

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
                    calculation.Helper.ProgressNavData = progressNavData + countProgress / countRangeNav;

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
            calculation.NavigationInfo.AngularSpeedSum = RadianToAngle(angularSpeedsSum) / (scanEnd - scanStart);
            calculation.NavigationInfo.AngularSpeedMax = RadianToAngle(maxAngularSpeed);

            //  3.Расхождение значений углов, рассчитанных по гироскопам и акселерометрам в пропуске
            if (bins)
            {
                IsSoledSpotCheck(calculation.NavigationInfo, solidSpotsData);

                calculation.NavigationInfo.AverageRollAngle /= handlerCount;
                calculation.NavigationInfo.AveragePitchAngle /= handlerCount;
            }
        }

        private void StandardDeviationBepState(Calculation calculation, NavCalcParams calcParams)
        {
            if (calculation.NavigationInfo.SquareDeviationAccelX < calcParams.SquareDeviationAccelX.Value &&
                calculation.NavigationInfo.SquareDeviationAccelY < calcParams.SquareDeviationAccelY.Value &&
                calculation.NavigationInfo.SquareDeviationAccelZ < calcParams.SquareDeviationAccelZ.Value &&
                calculation.NavigationInfo.SquareDeviationAngularSpeedY < calcParams.SquareDeviationAngularSpeedY.Value &&
                calculation.NavigationInfo.SquareDeviationAngularSpeedZ < calcParams.SquareDeviationAngularSpeedZ.Value)
                calculation.NavigationInfo.NavCalcState |= NavCalcStateTypes.StandardDeviations;
        }
        private void StandardDeviationBinsAdisState(Calculation calculation, NavCalcParams calcParams)
        {
            if (calculation.NavigationInfo.SquareDeviationAccelX < calcParams.SquareDeviationAccelX.Value &&
                calculation.NavigationInfo.SquareDeviationAccelY < calcParams.SquareDeviationAccelY.Value &&
                calculation.NavigationInfo.SquareDeviationAccelZ < calcParams.SquareDeviationAccelZ.Value &&
                calculation.NavigationInfo.SquareDeviationAngularSpeedX < calcParams.SquareDeviationAngularSpeedX.Value &&
                calculation.NavigationInfo.SquareDeviationAngularSpeedY < calcParams.SquareDeviationAngularSpeedY.Value &&
                calculation.NavigationInfo.SquareDeviationAngularSpeedZ < calcParams.SquareDeviationAngularSpeedZ.Value)
                calculation.NavigationInfo.NavCalcState |= NavCalcStateTypes.StandardDeviations;
        }

        private void GravAccelState(Calculation calculation, NavCalcParams calcParams)
        {
            if (calculation.NavigationInfo.GravitationalAcceleration > calcParams.GravAccel.Value - calcParams.GravAccel.Threshold &&
                calculation.NavigationInfo.GravitationalAcceleration < calcParams.GravAccel.Value + calcParams.GravAccel.Threshold)
                calculation.NavigationInfo.NavCalcState |= NavCalcStateTypes.GravAccel;
        }

        private void EarthAngularSpeedRotationBinsState(Calculation calculation, NavCalcParams calcParams)
        {
            if (calculation.NavigationInfo.EarthAngularSpeedRotation > calcParams.EarthAngularSpeedRotation.Value - calcParams.EarthAngularSpeedRotation.Threshold &&
                calculation.NavigationInfo.EarthAngularSpeedRotation < calcParams.EarthAngularSpeedRotation.Value + calcParams.EarthAngularSpeedRotation.Threshold)
                calculation.NavigationInfo.NavCalcState |= NavCalcStateTypes.EarthAngularSpeedRotation;
        }

        private void EarthAngularSpeedRotationBepAdisState(Calculation calculation, NavCalcParams calcParams)
        {
            if (calculation.NavigationInfo.EarthAngularSpeedRotation < calcParams.EarthAngularSpeedRotation.Value)
                calculation.NavigationInfo.NavCalcState |= NavCalcStateTypes.EarthAngularSpeedRotation;
        }

        private void DiffLatitudeState(Calculation calculation, NavCalcParams calcParams)
        {
            if (calculation.NavigationInfo.DifferenceLatitudes < calcParams.DifferenceLatitudes.Value)
                calculation.NavigationInfo.NavCalcState |= NavCalcStateTypes.DiffLatitude;
        }

        private void AccelSumState(Calculation calculation, NavCalcParams calcParams)
        {
            if (calculation.NavigationInfo.AccelSum > calcParams.AccelSum.Value - calcParams.AccelSum.Threshold &&
                calculation.NavigationInfo.AccelSum<calcParams.AccelSum.Value + calcParams.AccelSum.Threshold)
                calculation.NavigationInfo.NavCalcState |= NavCalcStateTypes.AccelSum;
        }

        private void AccelMaxState(Calculation calculation, NavCalcParams calcParams)
        {
            if (calculation.NavigationInfo.AccelMax<calcParams.AccelMax.Value)
                calculation.NavigationInfo.NavCalcState |= NavCalcStateTypes.AccelMax;
        }

        private void AngularSpeedSumState(Calculation calculation, NavCalcParams calcParams)
        {
            if (calculation.NavigationInfo.AngularSpeedSum < calcParams.AngularSpeedSum.Value)
                calculation.NavigationInfo.NavCalcState |= NavCalcStateTypes.AngularSpeedSum;
        }

        private void AngularSpeedMaxState(Calculation calculation, NavCalcParams calcParams)
        {
            if (calculation.NavigationInfo.AngularSpeedMax < calcParams.AngularSpeedMax.Value)
                calculation.NavigationInfo.NavCalcState |= NavCalcStateTypes.AngularSpeedMax;
        }

        private void AverageRollPitchAngleStat(Calculation calculation, NavCalcParams calcParams)
        {
            if (calculation.NavigationInfo.AverageRollAngle < calcParams.AverageRollAngle.Value &&
                calculation.NavigationInfo.AveragePitchAngle < calcParams.AveragePitchAngle.Value)
                calculation.NavigationInfo.NavCalcState |= NavCalcStateTypes.AverageRollPitchAngle;
        }

        private void MaxRollPitchAngleStat(Calculation calculation, NavCalcParams calcParams)
        {
            if (calculation.NavigationInfo.MaxRollAngle < calcParams.MaxRollAngle.Value &&
                calculation.NavigationInfo.MaxPitchAngle < calcParams.MaxPitchAngle.Value)
                calculation.NavigationInfo.NavCalcState |= NavCalcStateTypes.MaxRollPitchAngle;
        }

        private void PitchAngleAtMovementState(Calculation calculation, NavCalcParams calcParams)
        {
            if (calculation.NavigationInfo.PitchAngleSection6 != 0 ||
                calculation.NavigationInfo.PitchAngleSection3 != 0 ||
                calculation.NavigationInfo.PitchAngleSection2 != 0 ||
                calculation.NavigationInfo.PitchAngleAtMovement != 0)
                calculation.NavigationInfo.NavCalcState |= NavCalcStateTypes.PitchAngleAtMovement;
        }

        private void IsSoledSpotCheck(NavigationInfo navigationInfo, List<SolidSpotData> solidSpotsData )
        {
            foreach (var solidSpots in solidSpotsData )
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

        private double AngleToRadian(double value)
        {
            return value * Math.PI / 180;
        }

        private double RadianToAngle(double value)
        {
            return value * 180 / Math.PI;
        }
    }

    internal static class Helper
    {
        internal static Direction ToDirection(this CdmDiagData cdmDiagData)
        {
            return new Direction
            {
                Angle = cdmDiagData.Angle,
                EntryAngle = cdmDiagData.EntryAngle,
                DirectionName = (enDirectionName)cdmDiagData.DirectionName,
                Id = cdmDiagData.Id
            };
        }
    }
}
