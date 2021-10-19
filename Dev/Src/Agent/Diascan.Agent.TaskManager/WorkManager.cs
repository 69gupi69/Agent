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
using Diascan.Agent.Types.ModelCalculationDiagData;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.Types;
using Newtonsoft.Json;

namespace Diascan.Agent.TaskManager
{
    public class WorkManager
    {
        private readonly DiagDataLoader dataLoader;
        private readonly DiagDataLossTask diagDataLossTask;
        private readonly Action<Session> updateAction;
        private readonly Action<object> loggerInfoAction;
        private readonly Action<string> sharingEventsWarnMessage;

        private CancellationTokenSource token;
        private Session session;

        public WorkManager(Action<Session> updateAction, Action<object> loggerInfoAction, Action<string> sharingEventsWarnMessage)
        {
            dataLoader = new DiagDataLoader();
            diagDataLossTask = new DiagDataLossTask();

            this.updateAction = updateAction;
            this.loggerInfoAction = loggerInfoAction;
            this.sharingEventsWarnMessage = sharingEventsWarnMessage;
        }

        public void DoWork(Session session, CancellationTokenSource cts)
        {
            token = cts;
            this.session = session;

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
            if (session.Calculations.Aggregate(false, (current, calculation) => current | calculation.State.HasFlag(enCalculationStateTypes.FindTypes) ))
                return;

            if (session.Calculations.Aggregate(false, (current, calculation) => current | !Directory.Exists(calculation.DataLocation.BaseDirectory)))
                return;

            try
            {
                foreach (var calculation in session.Calculations)
                {
                    dataLoader.FindAllTypesInFolder(calculation, loggerInfoAction);

                    if (calculation.DiagDataList.Count == 0)
                    {
                        calculation.WorkState = enWorkState.Error;
                        loggerInfoAction?.Invoke(
                            $"calculation.DiagDataList.Count = {calculation.DiagDataList.Count}\n calculation.WorkState = {calculation.WorkState}");
                        return;
                    }

                    var cdDiagData = calculation.DiagDataList.FirstOrDefault(item => item is CdmDiagData data &&
                                                                                     (data.DirectionName == enCdmDirectionName.Cdl ||
                                                                                      data.DirectionName == enCdmDirectionName.Cds) ||
                                                                                     item.DataType == DataType.Cdl);
                    if (cdDiagData == null)
                        cdDiagData = calculation.DiagDataList.FirstOrDefault(item => item is CDpaDiagData data &&
                                                                                     (data.DirectionName == enCdmDirectionName.Cdl ||
                                                                                      data.DirectionName == enCdmDirectionName.Cds) ||
                                                                                     item.DataType == DataType.Cdl);

                    calculation.CdChange = calculation.Carriers.Exists(item => item.Change) && cdDiagData != null;

                    foreach (var diagData in calculation.DiagDataList)
                        GetSpeedByTypeData(calculation.DataOutput.Diameter, diagData, calculation.Carriers, calculation.DataOutput.OmniCarrierIds);
                }

                foreach (var calculation in session.Calculations)
                    calculation.State |= enCalculationStateTypes.FindTypes;
                
                loggerInfoAction?.Invoke($"{session.Calculations[0].DataOutput.WorkItemName}: Поиск типов данных завершен");
            }
            catch (Exception ex)
            {
                foreach (var calculation in session.Calculations)
                    calculation.WorkState = enWorkState.Error;
                updateAction?.Invoke(session);
                sharingEventsWarnMessage?.Invoke(ex.Message);
                loggerInfoAction?.Invoke(ex.Message);
            }
        }

        private void ByDefaultPassportSpeedDiapason(IDiagData diagData)
        {
            diagData.PassportSpeedDiapason = new Range<double>(0d, 0d);
        }

        private bool СheckСarriersCount(IDiagData diagData, List<CarrierData> carriers)
        {
            if (carriers.Count == 0)
            {
                ByDefaultPassportSpeedDiapason(diagData);
                return true;
            }
            return false;
        }

        private void GetSpeedByTypeData(double diameter, IDiagData diagData, List<CarrierData> carriers, List<int> omniCarrierIds)
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

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.Wm) && omniCarrierIds.Any(i => i == item.Id)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.MflT1:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.MflT1) && omniCarrierIds.Any(i => i == item.Id)) ?? throw new Exception(errMesseg);

                    diagData.PassportSpeedDiapason = new Range<double>( carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.MflT11:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.MflT11) && omniCarrierIds.Any(i => i == item.Id)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.MflT3:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.MflT3) && omniCarrierIds.Any(i => i == item.Id)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;
                case DataType.MflT31:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.MflT31) && omniCarrierIds.Any(i => i == item.Id)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;
                case DataType.MflT32:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.MflT32) && omniCarrierIds.Any(i => i == item.Id)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;
                case DataType.MflT33:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.MflT33) && omniCarrierIds.Any(i => i == item.Id)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;
                case DataType.MflT34:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.MflT34) && omniCarrierIds.Any(i => i == item.Id)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.TfiT4:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.TfiT4) && omniCarrierIds.Any(i => i == item.Id)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.TfiT41:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.TfiT41) && omniCarrierIds.Any(i => i == item.Id)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.Cdl:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.Cdl) && omniCarrierIds.Any(i => i == item.Id)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.Cdc:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.Cdc) && omniCarrierIds.Any(i => i == item.Id)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.Cd360:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.Cd360) && omniCarrierIds.Any(i => i == item.Id)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;
                case DataType.CDPA:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.CDPA ) && omniCarrierIds.Any(i => i == item.Id)) ?? throw new Exception(errMesseg);
                    diagData.PassportSpeedDiapason = new Range<double>(carrierData.SpeedMax, carrierData.SpeedMin);
                    diagData.NumberSensorsBlock = carrierData.NumberSensorsBlock;
                    break;

                case DataType.Ema:
                    if (СheckСarriersCount(diagData, carriers)) break;

                    carrierData = carriers.FirstOrDefault(item => item.Type.HasFlag(DataTypesExt.Ema) && omniCarrierIds.Any(i => i == item.Id)) ?? throw new Exception(errMesseg);

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
            if (session.Calculations.Aggregate(false, (current, calculation) => current | (!calculation.State.HasFlag(enCalculationStateTypes.FindTypes) ||
                                                                                           calculation.State.HasFlag(enCalculationStateTypes.Hashe)
                                                                                          )))
                return;

            //dataLoader.CalcHash(updateAction, loggerInfoAction, calculation, token);
            foreach (var calculation in session.Calculations)
            {
                calculation.State |= enCalculationStateTypes.Hashe;
                calculation.ProgressHashes = 100.0d;
            }

            updateAction?.Invoke(session);
            loggerInfoAction?.Invoke($"{session.Calculations[0].DataOutput.WorkItemName}: Расчет хешей завершен");
        }

        private void CalcOverSpeed()
        {
            if (session.Calculations.Aggregate(false, (current, calculation) => current | (CheckWorkState() ||
                                                                                           !CheckBaseCalcState() ||
                                                                                           calculation.State.HasFlag(enCalculationStateTypes.OverSpeed)
                                                                                          )))
                return;

            foreach (var calculation in session.Calculations)
                diagDataLossTask.CalcSpeedLdi(calculation);

            foreach (var calculation in session.Calculations)
                calculation.State |= enCalculationStateTypes.OverSpeed;
            updateAction?.Invoke(session);
            loggerInfoAction($"{session.Calculations[0].DataOutput.WorkItemName}: Расчет превышения скорости завершен");
        }

        private void CheckNavigation()
        {
            if (session.Calculations.Aggregate(false, (current, calculation) => current | (CheckWorkState() ||
                                                                                           !CheckBaseCalcState() ||
                                                                                           !calculation.NavigationInfo.State.HasFlag(NavigationStateTypes.NavigationData) ||
                                                                                           calculation.NavigationInfo.State.HasFlag(NavigationStateTypes.CalcNavigation)
                                                                                )))
                return;

            foreach (var calculation in session.Calculations)
                diagDataLossTask.CalculateNavigation(session, calculation, loggerInfoAction, updateAction);

            foreach (var calculation in session.Calculations)
            {
                calculation.NavigationInfo.State |= NavigationStateTypes.CalcNavigation;
                calculation.ProgressNavData = 1.0d;
            }

            updateAction?.Invoke(session);
            loggerInfoAction($"{session.Calculations[0].DataOutput.WorkItemName}: Расчет навигационных данных завершен");
        }


        private void ParallelDiagDataList(Calculation calculation)
        {
            //for (var i = 0; i < calculation.DiagDataList.Count; i++)
            Parallel.ForEach(calculation.DiagDataList, (diagData, state) =>
            {
                try
                {
                    //diagDataLossTask.Calc(updateAction, loggerInfoAction, calculation, session, calculation.DiagDataList[i], token);
                    diagDataLossTask.Calc(updateAction, loggerInfoAction, calculation, session, diagData, token);
                }
                catch (Exception e)
                {
                    state.Stop();
                    token?.Cancel();
                    if (!Directory.Exists(calculation.DataLocation.BaseDirectory)) return;

                    loggerInfoAction?.Invoke($"{calculation.DataOutput.WorkItemName}_{/*calculation.DiagDataList[i].DataType*/ diagData.DataType}: Ошибка в расчете сбойных датчиков: {e}");

                    if (e.Message.Contains("Операция успешно завершена"))
                        loggerInfoAction?.Invoke($"{calculation.DataOutput.WorkItemName}_{/*calculation.DiagDataList[i].DataType*/ diagData.DataType}: перезапуск расчета Ошибка очистки буфера буфера");
                    else
                        calculation.WorkState = enWorkState.Error;

                    updateAction?.Invoke(session);
                    return;
                }
            });
        }

        private void  CalcHaltingSensors()
        {
            if (session.Calculations.Aggregate(false, (current, calculation) => current | (CheckWorkState() ||
                                                                                           !CheckBaseCalcState() ||
                                                                                           calculation.State.HasFlag(enCalculationStateTypes.HaltingSensors)
                                                                                )))
                return;

            if (session.Calculations.Aggregate(false, (current, calculation) => current | !Directory.Exists(calculation.DataLocation.BaseDirectory)))
                return;

            var tasksCalculations = new Task[session.Calculations.Count];

            var j = 0;
            foreach (var calculation in session.Calculations)
            {
                tasksCalculations[j] = Task.Factory.StartNew(() => { ParallelDiagDataList(calculation); }, token.Token);
                j++;
            }

            Task.WaitAll(tasksCalculations);

            if (session.Calculations.Aggregate(false, (current, calculation) => current | calculation.DiagDataList.All(item => item.State)))
            {
                foreach (var calculation in session.Calculations)
                    calculation.State |= enCalculationStateTypes.HaltingSensors;

                updateAction?.Invoke(session);
                loggerInfoAction?.Invoke($"{session.Calculations[0].DataOutput.WorkItemName}: Расчет сбойных датчиков завершен");
            }
            else
            {
                loggerInfoAction?.Invoke($"{session.Calculations[0].DataOutput.WorkItemName}: Есть незавершенные диагностические данные:\n");
                foreach (var calculation in session.Calculations)
                    foreach (var DiagData in calculation.DiagDataList)
                        if (!DiagData.State)
                            loggerInfoAction?.Invoke($"{calculation.DataOutput.WorkItemName} - {DiagData.DataType}: \n MaxDistance - {DiagData.MaxDistance} \n ProcessedDist - {DiagData.ProcessedDist} \n StopDist - {DiagData.StopDist} \n  State - {DiagData.State} :");
            }
        }

        private void CheckCdlTail()
        {
            if (session.Calculations.Aggregate(false, (current, calculation) => current | (CheckWorkState() ||
                                                                                           !CheckBaseCalcState() ||
                                                                                           calculation.State.HasFlag(enCalculationStateTypes.CdlTail)
                                                                                           )))
                return;

            if (session.Calculations.Aggregate(false, (current, calculation) => current | !Directory.Exists(calculation.DataLocation.BaseDirectory)))
                return;

            try
            {
                foreach (var calculation in session.Calculations)
                {


                    var cdDiagData = calculation.DiagDataList.FirstOrDefault(item => item is CdmDiagData data &&
                                                                                     (data.DirectionName == enCdmDirectionName.Cdl ||
                                                                                      data.DirectionName == enCdmDirectionName.Cds) ||
                                                                                      item.DataType == DataType.Cdl);

                    if (calculation.CdChange && cdDiagData != null)
                        diagDataLossTask.CheckCdTail(calculation, session, cdDiagData, token, updateAction, loggerInfoAction);
                }

                foreach (var calculation in session.Calculations)
                    calculation.State |= enCalculationStateTypes.CdlTail;
                updateAction?.Invoke(session);
                loggerInfoAction?.Invoke($"{session.Calculations[0].DataOutput.WorkItemName}: Проверка ДД хвостовой секции завершена");
            }
            catch (SEHException e)
            {
                foreach (var calculation in session.Calculations)
                    calculation.WorkState = enWorkState.Error;
                loggerInfoAction?.Invoke($"{session.Calculations[0].DataOutput.WorkItemName}: Ошибка при проверки ДД хвостовой секции: {e}");
            }
        }

        private void CalcSplitDataTypeRange()
        {
            if (session.Calculations.Aggregate(false, (current, calculation) => current | (CheckWorkState() ||
                                                                                           !CheckBaseCalcState() ||
                                                                                           calculation.State.HasFlag(enCalculationStateTypes.SplitDataTypeRange)
                                                                                           )))
                return;

            foreach (var calculation in session.Calculations)
                foreach (var diagData in calculation.DiagDataList)
                {
                    var triggerChamberLocal = calculation.DataOutput.ReceptionChamber + diagData.StartDist;
                    var receptionChamberLocal = diagData.StopDist - calculation.DataOutput.TriggerChamber;
                    foreach (var i in diagData.HaltingSensors.Keys)
                    {
                        diagDataLossTask.SplitDataTypeRange(receptionChamberLocal, triggerChamberLocal, diagData.HaltingSensors[i].Last(), diagData.HaltingSensors[i]);
                        diagDataLossTask.SplitDataTypeRange(receptionChamberLocal, triggerChamberLocal, diagData.HaltingSensors[i].First(), diagData.HaltingSensors[i]);
                    }
                }

            foreach (var calculation in session.Calculations)
                calculation.State |= enCalculationStateTypes.SplitDataTypeRange;
            
            updateAction?.Invoke(session);
            loggerInfoAction?.Invoke($"{session.Calculations[0].DataOutput.WorkItemName}: CalcSplitDataTypeRange(): true");
        }

        public void SendCalculation(Session session, string url)
        {
            if (session.Calculations.Aggregate(false, (current, calculation) => current | ( calculation.WorkState.HasFlag(enWorkState.Error)               ||
                                                                                            !(calculation.State.HasFlag(enCalculationStateTypes.FindTypes) &&
                                                                                             calculation.State.HasFlag(enCalculationStateTypes.Hashe))     ||
                                                                                             calculation.State.HasFlag(enCalculationStateTypes.Sended)     ||
                                                                                             calculation.DataOutput.Local
                                                                                           )))
                return;

            try
            {
                var fileNameJson = $"{session.GlobalID}.json";
                var jsonSettings = new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.Auto, MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead};
                var json = JsonConvert.SerializeObject(session, jsonSettings);
                var sendData = Encoding.UTF8.GetBytes(json);

                if (!SetFile(url, fileNameJson, sendData)) return;

                foreach (var calculation in session.Calculations)
                    calculation.State |= enCalculationStateTypes.Sended;

                updateAction?.Invoke(session);
                loggerInfoAction($"{session.Calculations[0].DataOutput.WorkItemName}: Данные отправлены");
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
            catch (Exception )
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
            return session.Calculations.Aggregate(false, (current, calculation) => current | (calculation.State.HasFlag(enCalculationStateTypes.FindTypes) && 
                                                                                              calculation.State.HasFlag(enCalculationStateTypes.Hashe) &&
                                                                                              !calculation.WorkState.HasFlag(enWorkState.Error)));
        }
        private bool CheckWorkState()
        {
            return session.Calculations.Aggregate(false, (current, calculation) => current | calculation.WorkState.HasFlag(enWorkState.Error));
        }
    }
}
