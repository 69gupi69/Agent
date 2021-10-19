using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Diascan.Agent.CalcDiagDataLossTask;
using Diascan.Agent.Types;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.Types;
using Newtonsoft.Json;

namespace Diascan.Agent.TaskManager
{
    public class WorkManager
    {
        private readonly DiagDataLoader dataLoader;
        private readonly DiagDataLossTask diagDataLossTask;
        private readonly Action<Calculation> updateAction;
        private readonly Action<object> loggerInfoAction;
        private readonly Action<string> sharingEventsWarnMessage;

        private CancellationTokenSource token;
        private Calculation calculation;

        public WorkManager(Action<Calculation> updateAction, Action<object> loggerInfoAction, Action<string> sharingEventsWarnMessage)
        {
            dataLoader = new DiagDataLoader();
            diagDataLossTask = new DiagDataLossTask();

            this.updateAction = updateAction;
            this.loggerInfoAction = loggerInfoAction;
            this.sharingEventsWarnMessage = sharingEventsWarnMessage;
        }

        public void DoWork(Calculation calc, CancellationTokenSource cts)
        {
            token = cts;
            calculation = calc;

            FindDiagDataTypes();
            CalcHash();
            CalcOverSpeed();
            CheckNavigation();
            CalcHaltingSensors();
            CheckCdlTail();
            CalcSplitDataTypeRange();
        }

        private void FindDiagDataTypes()
        {
            if (calculation.State.HasFlag(enCalculationStateTypes.FindTypes)) return;
            if (!Directory.Exists(calculation.SourcePath)) return;

            try
            {
                var path = calculation.SourcePath;
                var omniFilePath = calculation.OmniFilePath;
                dataLoader.FindAllTypesInFolder(path, omniFilePath, calculation, loggerInfoAction);

                if (calculation.DiagDataList.Count == 0)
                {
                    calculation.WorkState = enWorkState.Error;
                    loggerInfoAction?.Invoke($"calculation.DiagDataList.Count = {calculation.DiagDataList.Count}\n calculation.WorkState = {calculation.WorkState}");
                    return;
                }
                
                var cdDiagData = calculation.DiagDataList.FirstOrDefault(item => item is CdmDiagData data &&
                                                                                 ( data.DirectionName == enCdmDirectionName.Cdl ||
                                                                                   data.DirectionName == enCdmDirectionName.Cds ) ||
                                                                                 item.DataType == DataType.Cdl);
                if ( cdDiagData== null )
                    cdDiagData = calculation.DiagDataList.FirstOrDefault(item => item is CDpaDiagData data &&
                                                                                 ( data.DirectionName == enCdmDirectionName.Cdl ||
                                                                                   data.DirectionName == enCdmDirectionName.Cds ) ||
                                                                                 item.DataType == DataType.Cdl);

                calculation.CdChange = calculation.Carriers.Exists(item => item.Change) && cdDiagData != null;

                foreach (var diagData in calculation.DiagDataList)
                    GetSpeedByTypeData(calculation.DataOutput.Diameter, diagData, calculation.Carriers);

                calculation.State |= enCalculationStateTypes.FindTypes;
                loggerInfoAction?.Invoke($"{calculation.DataOutput.WorkItemName}: Поиск типов данных завершен");
            }
            catch (Exception ex)
            {
                calculation.WorkState = enWorkState.Error;
                updateAction?.Invoke(calculation);
                sharingEventsWarnMessage?.Invoke(ex.Message);
                loggerInfoAction?.Invoke(ex.Message);
            }
        }

        private void ByDefaultPassportSpeedDiapason(DiagData diagData)
        {
            diagData.PassportSpeedDiapason = new Range<double>(0d, 0d);
        }

        private bool СheckСarriersCount(DiagData diagData, List<CarrierData> carriers)
        {
            if (carriers.Count == 0)
            {
                ByDefaultPassportSpeedDiapason(diagData);
                return true;
            }
            return false;
        }

        private void GetSpeedByTypeData(double diameter, DiagData diagData, List<CarrierData> carriers)
        {
            const string errMesseg = "В справочнике отсутствует идентификатор данного носителя!";
            CarrierData carrierData;
            switch (diagData.DataType)
            {
                case DataType.Spm:
                case DataType.Mpm:
                    if (Math.Abs(diameter - 159) < double.Epsilon || Math.Abs(diameter - 219) < double.Epsilon)
                    {
                        diagData.PassportSpeedDiapason = new Range<double>(3d, 0d);
                        diagData.NumberSensorsBlock = 0;
                        break;
                    }
                    else if (Math.Abs(diameter - 273) < double.Epsilon || Math.Abs(diameter - 325) < double.Epsilon)
                    {
                        diagData.PassportSpeedDiapason = new Range<double>(4d, 0d);
                        diagData.NumberSensorsBlock = 0;
                        break;
                    }
                    else if (diameter >= 377)
                    {
                        diagData.PassportSpeedDiapason = new Range<double>(6d, 0d);
                        diagData.NumberSensorsBlock = 0;
                        break;
                    }

                    diagData.PassportSpeedDiapason = new Range<double>(0d, 0d);
                    diagData.NumberSensorsBlock = 0;
                    break;

                case DataType.Wm:
                    if (СheckСarriersCount( diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.Wm)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.MflT1:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.MflT1)) ?? throw new Exception(errMesseg);

                    diagData.PassportSpeedDiapason = new Range<double>( carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.MflT11:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.MflT11)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.MflT3:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.MflT3)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;
                case DataType.MflT31:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.MflT31)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;
                case DataType.MflT32:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.MflT32)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;
                case DataType.MflT33:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.MflT33)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;
                case DataType.MflT34:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.MflT34)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.TfiT4:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.TfiT4)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.TfiT41:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.TfiT41)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.Cdl:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.Cdl)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.Cdc:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.Cdc)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.Cd360:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.Cd360 | ((CdmDiagData)diagData).DirectionName2DataTypesExt())) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;
                case DataType.CDPA:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.CDPA | ((CDpaDiagData)diagData).DirectionName2DataTypesExt())) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.Ema:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.Ema)) ?? throw new Exception(errMesseg);

                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                default: diagData.PassportSpeedDiapason = new Range<double>(0d, 0d);
                         diagData.NumberSensorsBlock = 0;
                    break;
            }
        }

        private void CalcHash()
        {
            if (!calculation.State.HasFlag(enCalculationStateTypes.FindTypes) ||
                calculation.State.HasFlag(enCalculationStateTypes.Hashe)) return;

            //dataLoader.CalcHash(updateAction, loggerInfoAction, calculation, token);
            calculation.State |= enCalculationStateTypes.Hashe;
            calculation.ProgressHashes = "100%";
            updateAction?.Invoke(calculation);
            loggerInfoAction?.Invoke($"{calculation.DataOutput.WorkItemName}: Расчет хешей завершен");
        }

        private void CalcOverSpeed()
        {
            if (CheckWorkState() || !CheckBaseCalcState() || calculation.State.HasFlag(enCalculationStateTypes.OverSpeed)) return;

            diagDataLossTask.CalcSpeedLdi(calculation);

            calculation.State |= enCalculationStateTypes.OverSpeed;
            updateAction?.Invoke(calculation);
            loggerInfoAction($"{calculation.DataOutput.WorkItemName}: Расчет превышения скорости завершен");
        }

        private void CheckNavigation()
        {
            if (CheckWorkState() || !CheckBaseCalcState() || !calculation.NavigationInfo.State.HasFlag(NavigationStateTypes.NavigationData) || 
                calculation.NavigationInfo.State.HasFlag(NavigationStateTypes.CalcNavigation)) return;

            diagDataLossTask.CalculateNavigation(calculation, loggerInfoAction);

            calculation.NavigationInfo.State |= NavigationStateTypes.CalcNavigation;
            updateAction?.Invoke(calculation);
            loggerInfoAction($"{calculation.DataOutput.WorkItemName}: Расчет навигационных данных завершен");
        }

        private void CalcHaltingSensors()
        {
            if (CheckWorkState() || !CheckBaseCalcState() || calculation.State.HasFlag(enCalculationStateTypes.HaltingSensors)) return;
            if (!Directory.Exists(calculation.SourcePath)) return;


            //for (var i = 0; i < calculation.DiagDataList.Count; i++)
            Parallel.ForEach(calculation.DiagDataList, (diagData, state) =>
            {
                try
                {
                    //diagDataLossTask.Calc(updateAction, loggerInfoAction, calculation, calculation.DiagDataList[i], token);
                    diagDataLossTask.Calc(updateAction, loggerInfoAction, calculation, diagData, token);
                }
                catch (Exception e)
                {
                    state.Stop();
                    token?.Cancel();
                    if (!Directory.Exists(calculation.SourcePath)) return;

                    loggerInfoAction?.Invoke($"{calculation.DataOutput.WorkItemName}_{ /*calculation.DiagDataList[i].DataType*/diagData.DataType}: Ошибка в расчете сбойных датчиков: {e}");

                    if (e.Message.Contains("restart calculation"))
                        loggerInfoAction?.Invoke($"{calculation.DataOutput.WorkItemName}_{ /*calculation.DiagDataList[i].DataType*/diagData.DataType}: перезапуск расчета Ошибка очистки буфера буфера");
                    else
                        calculation.WorkState = enWorkState.Error;

                    updateAction?.Invoke(calculation);
                    return;
                }
            });

            if (calculation.DiagDataList.All(item => item.State))
            {
                calculation.State |= enCalculationStateTypes.HaltingSensors;
                updateAction?.Invoke(calculation);
                loggerInfoAction?.Invoke($"{calculation.DataOutput.WorkItemName}: Расчет сбойных датчиков завершен");
            }
            else
            {
                loggerInfoAction?.Invoke($"{calculation.DataOutput.WorkItemName}: Есть незавершенные диагностические данные:\n");
                foreach (var DiagData in calculation.DiagDataList)
                    if (!DiagData.State)
                        loggerInfoAction?.Invoke($"{calculation.DataOutput.WorkItemName} - {DiagData.DataType}: \n MaxDistance - {DiagData.MaxDistance} \n ProcessedDist - {DiagData.ProcessedDist} \n StopDist - {DiagData.StopDist} \n  State - {DiagData.State} :");
            }
        }

        private void CheckCdlTail()
        {
            if (CheckWorkState() || !CheckBaseCalcState() || calculation.State.HasFlag(enCalculationStateTypes.CdlTail)) return;
            if (!Directory.Exists(calculation.SourcePath)) return;

            try
            {
                var cdDiagData = calculation.DiagDataList.FirstOrDefault( item => item is CdmDiagData data && 
                                                                                  ( data.DirectionName == enCdmDirectionName.Cdl || data.DirectionName == enCdmDirectionName.Cds ) ||
                                                                                  item.DataType == DataType.Cdl );

                if (calculation.CdChange && cdDiagData != null)
                    diagDataLossTask.CheckCdTail(calculation, cdDiagData, token, updateAction, loggerInfoAction);

                calculation.State |= enCalculationStateTypes.CdlTail;
                updateAction?.Invoke(calculation);
                loggerInfoAction?.Invoke($"{calculation.DataOutput.WorkItemName}: Проверка ДД хвостовой секции завершена");
            }
            catch (SEHException e)
            {
                calculation.WorkState = enWorkState.Error;
                loggerInfoAction?.Invoke($"{calculation.DataOutput.WorkItemName}: Ошибка при проверки ДД хвостовой секции: {e}");
            }
        }

        private void SplitDataTypeRange(double receptionChamberLocal, double triggerChamberLocal, SensorRange sensorRange, List<SensorRange> haltingSensor)
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

        private void CalcSplitDataTypeRange()
        {
            if (CheckWorkState() || !CheckBaseCalcState() || calculation.State.HasFlag(enCalculationStateTypes.SplitDataTypeRange)) return;

            foreach (var diagData in calculation.DiagDataList)
            {
                var triggerChamberLocal = calculation.DataOutput.ReceptionChamber + diagData.StartDist;
                var receptionChamberLocal = diagData.StopDist - calculation.DataOutput.TriggerChamber;
                foreach (var i in diagData.HaltingSensors.Keys)
                {
                    SplitDataTypeRange(receptionChamberLocal, triggerChamberLocal, diagData.HaltingSensors[i].Last(), diagData.HaltingSensors[i]);
                    SplitDataTypeRange(receptionChamberLocal, triggerChamberLocal, diagData.HaltingSensors[i].First(), diagData.HaltingSensors[i]);
                }
            }

            calculation.State |= enCalculationStateTypes.SplitDataTypeRange;
            updateAction?.Invoke(calculation);
            loggerInfoAction?.Invoke($"{calculation.DataOutput.WorkItemName}: CalcSplitDataTypeRange(): true");
        }

        public void SendCalculation(Calculation calc, string url)
        {
            if (calc.WorkState.HasFlag(enWorkState.Error) ||
                !(calc.State.HasFlag(enCalculationStateTypes.FindTypes) && calc.State.HasFlag(enCalculationStateTypes.Hashe)) ||
                calc.State.HasFlag(enCalculationStateTypes.Sended) ||
                calc.DataOutput.Local) return;

            try
            {
                var fileNameJson = $"{calc.GlobalId}.json";
                var jsonSettings = new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.Auto, MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead};
                var json = JsonConvert.SerializeObject(calc, jsonSettings);
                var sendData = Encoding.UTF8.GetBytes(json);

                if (!SetFile(url, fileNameJson, sendData)) return;

                calc.State |= enCalculationStateTypes.Sended;
                updateAction?.Invoke(calc);
                loggerInfoAction($"{calc.DataOutput.WorkItemName}: Данные отправлены");
            }
            catch (Exception ex)
            {
                loggerInfoAction?.Invoke(ex);
            }
        }

        public static bool TestConnection(string url)
        {
            var httpWebResponse = GetWebResponce(url);
            if (httpWebResponse == null) return false;

            if (httpWebResponse.StatusCode != HttpStatusCode.OK) return false;
            httpWebResponse.Close();
            return true;
        }

        private static HttpWebResponse GetWebResponce(string url)
        {
            try
            {
                var webRequest = WebRequest.Create(url);
                return (HttpWebResponse)webRequest.GetResponse();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public bool SetFile(string serviceUrl, string fileName, byte[] data)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    using (var content = new MultipartFormDataContent())
                    {
                        var fileContent = new ByteArrayContent(data); //(System.IO.File.ReadAllBytes(fileName));
                        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = fileName };
                        content.Add(fileContent);

                        var task = client.PostAsync(serviceUrl, content);
                        task.Wait();
                        return task.Result.StatusCode == HttpStatusCode.OK;
                    }
                }
            }
            catch (Exception ex)
            {
                loggerInfoAction?.Invoke(ex);
                return false;
            }
        }

        private bool CheckBaseCalcState()
        {
            return calculation.State.HasFlag(enCalculationStateTypes.FindTypes) &&
                   calculation.State.HasFlag(enCalculationStateTypes.Hashe) && !calculation.WorkState.HasFlag(enWorkState.Error);
        }
        private bool CheckWorkState()
        {
            return calculation.WorkState.HasFlag(enWorkState.Error);
        }
    }
}
