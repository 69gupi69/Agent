using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Diascan.Agent.CalcDiagDataLossTask;
using Diascan.Agent.ModelDB;
using Diascan.Agent.DiagDataLoader;
using LiteDB;
using Newtonsoft.Json;
using Diascan.Agent.Manager.Properties;
using System.Configuration;
using System.Diagnostics;
using System.Management;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Diascan.Agent.SOHandling;
using Diascan.NDT.Enums;
using File = System.IO.File;

namespace Diascan.Agent.Manager
{
    public class Controller : IDisposable
    {
        private bool connectionFlag = true;
        private readonly LiteDatabase db;
        private readonly LiteCollection<Calculation> calcCollection;
        private readonly List<Calculation> buffCalcCollection;

        private System.Threading.Timer timerMain;
        private System.Threading.Timer timerSendData;
        private System.Threading.Timer timerGetDirectoryDataModel;
        private System.Threading.Timer timerSendLogFile;

        private readonly DiagDataLossTask diagDataLossTask;
        private readonly DataLoader dataLoader;

        private readonly string directoryDataModelFilePath;

        //  Инициализация контроллера
        public Controller()
        {
            directoryDataModelFilePath = Application.StartupPath + @"\" + Resources.NameDirectoryDataBaseFile;

            db = new LiteDatabase(Application.StartupPath + @"\DiascanAgent.db");
            calcCollection = db.GetCollection<Calculation>("Calculation");
            diagDataLossTask = new DiagDataLossTask();
            dataLoader = new DataLoader();
            buffCalcCollection = calcCollection.FindAll().ToList();

            // таймеры
            timerMain = new System.Threading.Timer(TimerMain, null, TimeSpan.Zero, new TimeSpan(0, 0, 0, 2));
            timerSendData = new System.Threading.Timer(SendData, null, TimeSpan.Zero, new TimeSpan(0, 0, 10, 0));
            timerGetDirectoryDataModel = new System.Threading.Timer(CheckUpdateTimeDirectory, null, TimeSpan.Zero, new TimeSpan(0, 24, 0, 0));
            timerSendLogFile = new System.Threading.Timer(CheckTimeSendLog, null, TimeSpan.Zero, new TimeSpan(0, 24, 0, 0));
        }

        public void TransferNewAddress(string omniFilePath, ReferenceInputData referenceInputData )
        {
            var path = Path.GetDirectoryName(omniFilePath);

            if (Path.GetFileName(path) != Path.GetFileNameWithoutExtension(omniFilePath))
            {
                SharingEvents.SharingEvents.OnWarnMessage("Не удалось создать расчет. Данные для расчета не найдены: " + path);
                return;
            }
            if (Directory.Exists(path))
                AddNewCalculation(omniFilePath, referenceInputData);
        }

        private void AddNewCalculation(string omniFilePath, ReferenceInputData referenceInputData)
        {
            var path = Path.GetDirectoryName(omniFilePath);
            try
            {
                Directory.GetAccessControl(path);
                referenceInputData.AccountUserName = Logger.Logger.GetProcessUser();
                referenceInputData.ComputerName = SystemInformation.ComputerName;
                var calc = new Calculation(path)
                {
                    GlobalId = Guid.NewGuid(),
                    TimeAddCalculation = DateTime.Now,
                    OmniFilePath = omniFilePath,
                    DataOutput = referenceInputData
                };

                calcCollection.Insert(calc);
                buffCalcCollection.Add(calc);
                var mes = $"(Добавление нового расчета, путь к данным: {path}) " +
                          $"(Индификатор расчета: {calc.GlobalId}) " +
                          $"(Название (код) прогона: {calc.DataOutput.WorkItemName}) " +
                          $"(Дефектоскоп: {calc.DataOutput.FlawDetector}) " +
                          $"(Заказчик: {calc.DataOutput.Contractor}) " +
                          $"(Трубопровод: {calc.DataOutput.PipeLine}) " +
                          $"(Участок: {calc.DataOutput.Route}) " +
                          $"(Диамерт: {calc.DataOutput.Diameter}) " +
                          $"(Дата пропуска: {calc.DataOutput.DateWorkItem}) " +
                          $"(Ответственный за пропуск: {calc.DataOutput.ResponsibleWorkItem}) " +
                          $"(Конец камеры пуска: {calc.DataOutput.ReceptionChamber}) " +
                          $"(Начало камеры приема: {calc.DataOutput.TriggerChamber})";
                Logger.Logger.Info(mes);
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex);
                throw new FaultException(ex.Message);
            }
        }

        private void TimerMain(object sender)
        {
            timerMain.Dispose();
            FindAllTypes();
            HashesTasks();
            CalcSpeedLdi();
            CheckNavigation();
            CalcHaltingSensorsTasks();
            CalculatedCdlTail();
            DefinitionOfRerun();
            timerMain = new System.Threading.Timer(TimerMain, null, new TimeSpan(0, 0, 0, 2), new TimeSpan(0, 0, 0, 2));
        }

        private void CheckNavigation()
        {
            var calcCollectionCheckNavigation = buffCalcCollection
                .FindAll(calculation =>
                    calculation.NavigationInfo.NavigationState.HasFlag(NavigationStateTypes.NavigationData) &&
                    !calculation.NavigationInfo.NavigationState.HasFlag(NavigationStateTypes.CalcNavigation));

            if(calcCollectionCheckNavigation.Count == 0) return;
            foreach (var calculation in calcCollectionCheckNavigation)
            {
                diagDataLossTask.CalculateNavigation(calculation);
                calculation.NavigationInfo.NavigationState |= NavigationStateTypes.CalcNavigation;
                calcCollection.Update(calculation);
            }

        }

        private Range<double> GetSpeedByTypeData( double diameter, DataType diagDataType, List<SensorMediaIdentifier> carriers  )
        {
            SensorMediaIdentifier sensorMediaIdentifier;
            switch ( diagDataType )
            {
                case DataType.Spm    : 
                case DataType.Mpm    :
                                       if ( diameter >= 152.4 && diameter <= 203.2 )
                                       {
                                            return new Range<double>( 3d, 0d );
                                       }
                                       else if ( diameter >= 254 && diameter <= 304.8 )
                                       {
                                            return new Range<double>( 4d, 0d );
                                       }
                                       else if( diameter >= 355.6 )
                                       {
                                            return new Range<double>( 6d, 0d );
                                       }
                                       return new Range<double>( 0d, 0d );
                                       
                                      
                case DataType.Wm     : sensorMediaIdentifier = carriers.First( item => item.Type.HasFlag( Types.Wm ) );
                                       return new Range<double>( sensorMediaIdentifier.SpeedMax, 0d );

                case DataType.MflT1  : sensorMediaIdentifier = carriers.First( item => item.Type.HasFlag( Types.MflT1 ) );
                                       return new Range<double>( sensorMediaIdentifier.SpeedMax, 0d );

                case DataType.MflT3  : sensorMediaIdentifier = carriers.First( item => item.Type.HasFlag( Types.MflT3 ) );
                                       return new Range<double>( sensorMediaIdentifier.SpeedMax, 0d );

                case DataType.TfiT4  :
                    sensorMediaIdentifier = carriers.First(item => item.Type.HasFlag(Types.TfiT4));
                    return new Range<double>(sensorMediaIdentifier.SpeedMax, 0d);

                case DataType.Cdl    : sensorMediaIdentifier = carriers.First( item => item.Type.HasFlag( Types.Cdl) );
                                       return new Range<double>( sensorMediaIdentifier.SpeedMax, 0d );
                                      
                case DataType.Cdc    : sensorMediaIdentifier = carriers.First( item => item.Type.HasFlag( Types.Cdc ) );
                                       return new Range<double>( sensorMediaIdentifier.SpeedMax, 0d );
                                      
                case DataType.Cd360  : sensorMediaIdentifier = carriers.First( item => item.Type.HasFlag( Types.Cd360 ) );
                                       return new Range<double>( sensorMediaIdentifier.SpeedMax, 0d );

                default              : return new Range<double>(0d, 0d);
            }
        }

        private List<SensorMediaIdentifier> GetUniqueSensorMediaIdentifier( Calculation calculation, ControllerHelper controllerHelper )
        {
            var carriers = new List<SensorMediaIdentifier>();
            using ( var dbSensorMediaIdentifier = new LiteDatabase( Application.StartupPath + @"\SensorMediaIdentifiers.db" ) )
            {
                var sensorMediaIdentifierCollection = dbSensorMediaIdentifier.GetCollection<SensorMediaIdentifier>( "SensorMediaIdentifier" ).FindAll();

                //  найти CarrierId
                var idList = controllerHelper.GetCarrierId( calculation.OmniFilePath );
                //  находим тип прибора для каждого кериера

                var mediaIdentifierCollection = sensorMediaIdentifierCollection as IList<SensorMediaIdentifier> ?? sensorMediaIdentifierCollection.ToList();
                var defectoscopeName = string.Empty;

                // ищем первое вхождение
                foreach ( var id in idList )
                {
                    defectoscopeName = mediaIdentifierCollection.FirstOrDefault(item => item.Id == id)?.Defectoscope;
                    if( !String.IsNullOrEmpty( defectoscopeName ) )
                        break;
                }

                if ( defectoscopeName != string.Empty )
                    carriers = mediaIdentifierCollection.Where( item => item.Defectoscope == defectoscopeName ).ToList();
            }

            return carriers;
        }

        private void FindAllTypes()
        {
            var calcCollectionFindTypes = buffCalcCollection.FindAll( calculation => !calculation.State.HasFlag( CalculationStateTypes.FindTypes ) );
            if (calcCollectionFindTypes.Count == 0) return;

            foreach (var calculation in calcCollectionFindTypes)
            {
                if ( !Directory.Exists( calculation.SourcePath ) )
                    continue;
                try
                {
                    var path = calculation.SourcePath;
                    var omniFilePath = calculation.OmniFilePath;
                    dataLoader.FindAllTypesInFolder(path, omniFilePath, calculation);

                    if (calculation.DiagDataList.Count == 0)
                    {
                        var calcId = calculation.Id;
                        Canseled(calcId);
                        SharingEvents.SharingEvents.OnDeleteRow(
                            "Ошибка, не удалось создать расчет. Расчет будет удален. Данные для расчета (carrier) не найдены: " + path, calcId);
                        return;
                    }
                    
                    var controllerHelper = new ControllerHelper();
                        calculation.Helper.Carriers = GetUniqueSensorMediaIdentifier( calculation, controllerHelper );
                    
                    if ( calculation.Helper.Carriers.Count == 0 ) // если так выходи и завершаем расчет!!! 
                    {
                        calculation.State |= CalculationStateTypes.CdlTail;
                    }
                    else
                    {
                        var cdDiagData = calculation.DiagDataList.FirstOrDefault( item =>
                            item is CdmDiagData data &&
                            ( data.DirectionName == enCdmDirectionName.Cdl || data.DirectionName == enCdmDirectionName.Cds ) ||
                            item.DataType == DataType.Cdl );

                        calculation.CdChange = calculation.Helper.Carriers.Exists( item => item.Change ) && cdDiagData != null;

                        foreach( var diagData in calculation.DiagDataList )
                            diagData.PassportSpeedDiapason = GetSpeedByTypeData( calculation.DataOutput.Diameter, diagData.DataType, calculation.Helper.Carriers );
                    }

                    calculation.State |= CalculationStateTypes.FindTypes;
                    calcCollection.Update(calculation);
                    var mes = $"Предварительные вычисления закончены, путь к данным: {calculation.SourcePath}";
                    Logger.Logger.Info(mes);
                }
                catch (Exception ex)
                {
                    Logger.Logger.Info(ex.Message);
                }
            }
        }

        //  Подсчет хешей во всей базе
        private void HashesTasks()
        {
            var calcCollectionHashes = buffCalcCollection
                .FindAll(calculation => calculation.State.HasFlag(CalculationStateTypes.FindTypes) &&
                                        !calculation.State.HasFlag(CalculationStateTypes.Hashed));
            if (calcCollectionHashes.Count == 0) return;

            foreach (var calculation in calcCollectionHashes)
            {
                if (!dataLoader.Hash(calculation))
                    continue;
                calculation.State |= CalculationStateTypes.Hashed;
                calcCollection.Update(calculation);
                var mes = $"Расчет хешей завершен, путь к данным: {calculation.SourcePath}";
                Logger.Logger.Info(mes);
            }
        }


        private void CalcSpeedLdi()
        {
            var calcCollectionSpeed = buffCalcCollection
                .FindAll(calculation =>
                    calculation.State.HasFlag(CalculationStateTypes.FindTypes |
                                              CalculationStateTypes.Hashed)
                    && !calculation.State.HasFlag(CalculationStateTypes.Speed));

            if (calcCollectionSpeed.Count == 0) return;

            var tasks = new Task[calcCollectionSpeed.Count];
            for (var i = 0; i < calcCollectionSpeed.Count; i++)
            {
                var index = i;
                tasks[i] = new Task(() =>
                {
                    diagDataLossTask.CalcSpeedLdi(UpdateAction, calcCollectionSpeed[index]);
                });
                tasks[i].Start();
            }
            Task.WaitAll(tasks);
        }
        
        //Расчет битых датчиков
        private void CalcHaltingSensorsTasks()
        {
            var calcCollectionHaltingSensors = buffCalcCollection
                .FindAll(calculation =>
                    calculation.State.HasFlag(CalculationStateTypes.FindTypes |
                                              CalculationStateTypes.Hashed |
                                              CalculationStateTypes.Speed)
                    && !calculation.State.HasFlag(CalculationStateTypes.Calculated));

            if (calcCollectionHaltingSensors.Count == 0) return;

            foreach (var calculation in calcCollectionHaltingSensors)
            {
                if (!Directory.Exists(calculation.SourcePath)) continue;

                var calcTaskContinue = true;
                
                var result = true;
                var tasksDiagData = new Task[calculation.DiagDataList.Count];
                var j = 0;
#if !DEBUG
                CSharpExcHandler.HandleSO(() =>
                {
#endif
                foreach (var diagData in calculation.DiagDataList)
                {
                    if (diagData.State) continue;
                    if (diagData.DataType == DataType.Nav) continue;
                    tasksDiagData[j] = new Task(() =>
                    {
                        try
                        {
                            if (calcTaskContinue)
                                calcTaskContinue = diagDataLossTask.Calc(UpdateAction, calculation, diagData);
                        }
                        catch (SEHException e)
                        {
                            calcTaskContinue = false;
                        }
                        catch (Exception e)
                        {
                            calcTaskContinue = false;
                            if (!Directory.Exists(calculation.SourcePath)) return;
                            Logger.Logger.Info("Ошибка в процессе расчета, расчет будет удален: " + e);
                            SharingEvents.SharingEvents.OnDeleteRow("Ошибка в процессе расчета, расчет будет удален: " + e, calculation.Id);
                            calcCollection.Delete(calculation.Id);
                            buffCalcCollection.Remove(
                                buffCalcCollection.FirstOrDefault(q => q.Id == calculation.Id));
                        }
                    });
                    tasksDiagData[j].Start();
                    j++;
                }
                Task.WaitAll(tasksDiagData.Where(item => item != null).ToArray());
#if !DEBUG
                });
#endif
                foreach (var diagData in calculation.DiagDataList)
                {
                    if (!diagData.State)
                        result = false;
                }
                if (result)
                {
                    calculation.State |= CalculationStateTypes.Calculated;
                    calcCollection.Update(calculation);
                }
                if (calcTaskContinue)
                {
                    var mes = $"Расчет ПДИ завершен, путь к данным: {calculation.SourcePath}";
                    Logger.Logger.Info(mes);
                }
            }
        }

        private void CalculatedCdlTail()
        {
            var calcCollectionCdlTail = buffCalcCollection
                .FindAll(calculation => calculation.State.HasFlag(CalculationStateTypes.FindTypes |
                                                                  CalculationStateTypes.Hashed |
                                                                  CalculationStateTypes.Speed |
                                                                  CalculationStateTypes.Calculated)
                                        && !calculation.State.HasFlag(CalculationStateTypes.CdlTail));

            if (calcCollectionCdlTail.Count == 0) return;

            foreach (var calculation in calcCollectionCdlTail)
            {
                if (!Directory.Exists(calculation.SourcePath)) continue;
                try
                {
                    var cdDiagData = calculation.DiagDataList.FirstOrDefault(item =>
                        item is CdmDiagData data && (data.DirectionName == enCdmDirectionName.Cdl || data.DirectionName == enCdmDirectionName.Cds) ||
                        item.DataType == DataType.Cdl);
                    
                    if (calculation.CdChange && cdDiagData != null)
                        diagDataLossTask.CheckCdTail(calculation, cdDiagData, UpdateAction);
                    else
                        calculation.State |= CalculationStateTypes.CdlTail;

                    calcCollection.Update(calculation);
                }
                catch (SEHException e)
                {
                    Logger.Logger.Info(e);
                }
            }
        }

        private void DefinitionOfRerun()
        {
            var calcCollectionDefinitionOfRerun = buffCalcCollection
                .FindAll(calculation => calculation.State.HasFlag(CalculationStateTypes.FindTypes |
                                                                  CalculationStateTypes.Hashed |
                                                                  CalculationStateTypes.Speed |
                                                                  CalculationStateTypes.Calculated |
                                                                  CalculationStateTypes.CdlTail)
                                        && !calculation.State.HasFlag(CalculationStateTypes.DefinitionOfRerun));
            if (calcCollectionDefinitionOfRerun.Count == 0) return;
            //  1. Критерий (общий для всех)

            const string criterion = "Суммарная площадь потерянной диагностической информации более 5% от подлежащей обследованию";
            foreach (var calculation in calcCollectionDefinitionOfRerun)
            {
                var defectoscope = "";
                var controllerHelper = new ControllerHelper();
                var dataTypes = controllerHelper.GroupDiagDataByType(calculation.DiagDataList);
                DiagDataLossTask.CalcAreas(calculation);

                foreach (var dataType in dataTypes)
                {
                    if (dataType.Key == enCdmDirectionName.None)
                    {
                        foreach (var noneType in dataType.Value)
                        {
                            if (noneType.DataType == DataType.Mpm)
                                defectoscope = "ПРН";
                            var area = Math.Round(noneType.AreaLdi, 3);
                            var surveyArea = Math.Round(Math.PI * calculation.DataOutput.Diameter / 1000 *
                                                                            (noneType.EndDist - noneType.StartDist), 3);
                            var percentLoss = (area / surveyArea) * 100;
                            if (!(percentLoss > 5)) continue;
                            calculation.RestartReport.Add(new RestartCriterion(noneType.DataType.ToString(), criterion));
                        }
                    }
                    else
                    {
                        var dataTypeFirst = dataType.Value.FirstOrDefault();
                        var surveyArea = Math.Round(Math.PI * calculation.DataOutput.Diameter / 1000 *
                                                    (dataTypeFirst.EndDist - dataTypeFirst.StartDist), 3);
                        var area = Math.Round(dataType.Value.Sum(q => q.AreaLdi), 3);
                        var percentLoss = (area / surveyArea) * 100;
                        if (!(percentLoss > 5)) continue;
                        calculation.RestartReport.Add(new RestartCriterion(dataType.Key.ToString(), criterion));
                    }
                }

                if (calculation.DiagDataList.Any(q => q.DataType != DataType.Mpm))
                {
                    if (calculation.Helper.Carriers.Count == 0)
                        defectoscope = "";//calculation.RestartReport.Add(new RestartCriterion("", "Идентификаторы не определены."));
                    else
                        defectoscope = calculation.Helper.Carriers.FirstOrDefault().Defectoscope;
                }
                //  2. Для ВИП ДКП количиество смежных сбойных датчиков 8 и более протяженностью более 1 км
                if (defectoscope.Contains("ДКП") || defectoscope.Contains("ДВУ"))
                {
                    //  WM
                    var wmDiagDataList = calculation.DiagDataList.Where(q => q.DataType == DataType.Wm);
                    diagDataLossTask.NearSensors(wmDiagDataList, calculation.RestartReport, 6);
                    //  CD
                    var cdDiagDataList = calculation.DiagDataList.Where(q => q.DataType == DataType.Cdc || q.DataType == DataType.Cdl || q.DataType == DataType.Cd360);
                    diagDataLossTask.NearSensors(cdDiagDataList, calculation.RestartReport, 3);
                }

                //  3. Для ВИП УСК.04 количество смежных сбойных датчиков 6 и более протяженностью от 1 км
                else if (defectoscope.Contains("УСК.04"))
                {
                    diagDataLossTask.NearSensors(calculation.DiagDataList, calculation.RestartReport, 6);
                }

                //  4 и 5. Для ВИП с ультрозвуковыми системами - наличие потерь по 4 и более смежными датчиками протяженностью от 1 км
                //         Для ВИП с магнитными системами - наличие потерь по 4-м и более смежыми блоками датчиков протяженностью от 1 км
                else if (defectoscope.Contains("ДКК") || 
                         defectoscope.Contains("ДКУ") ||
                         defectoscope.Contains("УСК.03") || 
                         defectoscope.Contains("УСК") || 
                         defectoscope.Contains("МСК") || 
                         defectoscope.Contains("ДМК") || 
                         defectoscope.Contains("ДМУ") ||
                         defectoscope.Contains("ДКМ"))
                {
                    //  WM
                    var wmDiagDataList = calculation.DiagDataList.Where(q => q.DataType == DataType.Wm);
                    diagDataLossTask.NearSensors(wmDiagDataList, calculation.RestartReport, 4);
                    //  CD
                    var cdDiagDataList = calculation.DiagDataList.Where(q => q.DataType == DataType.Cdc || q.DataType == DataType.Cdl || q.DataType == DataType.Cd360);
                    diagDataLossTask.NearSensors(cdDiagDataList, calculation.RestartReport, 3);
                    //  MFL / TFI
                    var mflDiagDataList = calculation.DiagDataList.Where(q => q.DataType == DataType.MflT1 || 
                                                                              q.DataType == DataType.MflT11 ||
                                                                              q.DataType == DataType.MflT2 ||
                                                                              q.DataType == DataType.MflT22 ||
                                                                              q.DataType == DataType.MflT3 ||
                                                                              q.DataType == DataType.MflT31 ||
                                                                              q.DataType == DataType.MflT32 ||
                                                                              q.DataType == DataType.MflT33 ||
                                                                              q.DataType == DataType.MflT34 ||
                                                                              q.DataType == DataType.TfiT4 ||
                                                                              q.DataType == DataType.TfiT41);
                    diagDataLossTask.NearSensors(mflDiagDataList, calculation.RestartReport, 4);
                }

                //  6. Для ВИП ПРН/ОПТ - отсутствие информации от 2 и более смежных каналов протяженностью от 1 км
                else if (defectoscope.Contains("ПРН") || defectoscope.Contains("ОПТ"))
                {
                    diagDataLossTask.NearSensors(calculation.DiagDataList, calculation.RestartReport, 2);
                }

                calculation.State |= CalculationStateTypes.DefinitionOfRerun;
                calcCollection.Update(calculation);
            }
            SendData(null);
        }

        //  Сохранение сalculation в БД
        public void UpdateAction(Calculation calculation)
        {
            try
            {
                calcCollection.Update(calculation);
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex);
            }
        }

        //  Возвращает массив для отображения в форме
        public List<Calculation> GetBdForm()
        {
            return buffCalcCollection;
        }

        // Получить Json по индексу
        public byte[] CalculationToJson( int id )
        {
            var collection = buffCalcCollection.FirstOrDefault(q => q.Id == id);
            var settings   = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var json       = JsonConvert.SerializeObject( collection, settings );

            return Encoding.UTF8.GetBytes( json );
        }

        // Получить Calculation по индексу
        public Calculation GetCalculation(int id)
        {
            return buffCalcCollection.FirstOrDefault(q => q.Id == id);
        }

        //  Удаление записи и отмена выполнения задачи по Id
        public string Canseled(int id)
        {
            dataLoader.CancellableId.Add(id);
            diagDataLossTask.CancellableId.Add(id);

            calcCollection.Delete(id);
            var calcDel = buffCalcCollection.FirstOrDefault(q => q.Id == id);
            buffCalcCollection.Remove(calcDel);
            //db.Shrink();
            return buffCalcCollection.FirstOrDefault(q => q.Id == id)?.SourcePath;
        }

        /// <summary>
        /// Проверка связи с сервером
        /// </summary>
        private HttpWebResponse GetConnection()
        {
            try
            {
                var address = ConfigurationManager.AppSettings["ServerAddress"] +
                              ConfigurationManager.AppSettings["AddressCheckingAccessResource"];
                var webRequest = WebRequest.Create(address);
                var httpWebResponse = (HttpWebResponse) webRequest.GetResponse();
                if (!connectionFlag)
                    Logger.Logger.Info("Базовое соединение восстановлено");
                connectionFlag = true;
                return httpWebResponse;
            }
            catch (Exception ex)
            {
                if (connectionFlag)
                    Logger.Logger.Info(ex);
                connectionFlag = false;
                return null;
            }
        }

        public bool TestConnection(string newAddress)
        {
            var address = "";
            try
            {
                address = newAddress + ConfigurationManager.AppSettings["AddressCheckingAccessResource"];
                var webRequest = WebRequest.Create(address);
                var httpWebResponse = (HttpWebResponse) webRequest.GetResponse();
                httpWebResponse.Close();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Logger.Info($"Сервер по адресу {address} не отвечает. Ошибка: {ex}");
                return false;
            }
        }

        public string GetAddressConnection()
        {
            return ConfigurationManager.AppSettings["ServerAddress"];
        }

        public void ChangeAddressConnection(string newAddress)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            foreach (XmlElement element in xmlDoc.DocumentElement)
            {
                if (element.Name.Equals("appSettings"))
                {
                    foreach (XmlNode node in element.ChildNodes)
                    {
                        if (node.Attributes[0].Value.Equals("ServerAddress"))
                        {
                            node.Attributes[1].Value = newAddress;
                        }
                    }
                }
            }

            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            ConfigurationManager.RefreshSection("appSettings");
            Logger.Logger.Info($"Адрес сервера изменен: {newAddress}");
        }

        public void Dispose()
        {
            db.Dispose();
        }

#region Отправка данных на сервер

        /// <summary>
        ///  Отправка на сервер Json collection
        /// </summary>
        private void SendData(object sender)
        {
            try
            {
                var condition = buffCalcCollection.FindAll(q => q.State == (CalculationStateTypes.FindTypes |
                                                                            CalculationStateTypes.Hashed |
                                                                            CalculationStateTypes.Speed |
                                                                            CalculationStateTypes.Calculated |
                                                                            CalculationStateTypes.CdlTail |
                                                                            CalculationStateTypes.DefinitionOfRerun)
                                                                && q.State != CalculationStateTypes
                                                                    .Transferred);

                if (condition.Count == 0) return;

                var httpWebResponse = GetConnection();
                if (httpWebResponse == null) return;
                var address = ConfigurationManager.AppSettings["ServerAddress"] +
                              ConfigurationManager.AppSettings["AddressUploadJsonLDIFiles"];
                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    httpWebResponse.Close();

                    var listBuf = new Dictionary<string, byte[]>();

                    foreach (var collection in buffCalcCollection)
                    {
                        if (collection.State == (CalculationStateTypes.Calculated |
                                                 CalculationStateTypes.Hashed |
                                                 CalculationStateTypes.Speed |
                                                 CalculationStateTypes.FindTypes |
                                                 CalculationStateTypes.CdlTail |
                                                 CalculationStateTypes.DefinitionOfRerun)
                            && collection.State != CalculationStateTypes.Transferred)
                        {
                            var nameFileJson = collection.GlobalId + Resources.FileExtensionJson;
                            var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto};
                            var json = JsonConvert.SerializeObject(collection, settings);
                            var jsonByte = Encoding.UTF8.GetBytes(json);

                            listBuf.Add(nameFileJson, jsonByte);
                        }
                    }

                    if (listBuf.Count > 0)
                    {
                        if (SetFile(address, listBuf))
                            foreach (var collection in buffCalcCollection)
                                if (collection.State == (CalculationStateTypes.Calculated |
                                                         CalculationStateTypes.Hashed |
                                                         CalculationStateTypes.Speed |
                                                         CalculationStateTypes.FindTypes |
                                                         CalculationStateTypes.CdlTail |
                                                         CalculationStateTypes.DefinitionOfRerun) &&
                                    collection.State != CalculationStateTypes.Transferred)
                                {
                                    collection.State |= CalculationStateTypes.Transferred;
                                    calcCollection.Update(collection);
                                    Logger.Logger.Info($"Данные отправлены, путь к данным: {collection.SourcePath}");
                                }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex);
            }
        }

        private bool SetFile(string serviceUrl, Dictionary<string, byte[]> listBuf)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    using (var content = new MultipartFormDataContent())
                    {
                        foreach (var bytese in listBuf)
                        {
                            var fileContent =
                                new ByteArrayContent(bytese.Value); //(System.IO.File.ReadAllBytes(fileName));
                            fileContent.Headers.ContentDisposition =
                                new ContentDispositionHeaderValue("attachment") {FileName = bytese.Key};
                            content.Add(fileContent);
                        }

                        var task = client.PostAsync(serviceUrl, content);
                        task.Wait();
                        return task.Result.StatusCode == HttpStatusCode.OK;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex);
                return false;
            }
        }

#endregion

#region Отправка и обновление Log файла

        /// <summary>
        /// Проверка времени Log файла
        /// </summary>
        private void CheckTimeSendLog(object myObject)
        {
            if (!System.IO.File.Exists(Logger.Logger.PathFileLog))
                System.IO.File.Create(Logger.Logger.PathFileLog);

            var dataCreation = System.IO.File.GetCreationTime(Logger.Logger.PathFileLog);
            var dataNow = DateTime.Now;
            var lastUpdate = dataNow - dataCreation;
            if (lastUpdate >= TimeSpan.FromDays(1))
            {
                if (SendLogData())
                {
                    // получили и заменили справочник
                    timerSendLogFile = new System.Threading.Timer(CheckUpdateTimeDirectory, null,
                        new TimeSpan(0, 24, 0, 0), new TimeSpan(0, 24, 0, 0));
                    return;
                }

                // не удалось получить справочник
                timerSendLogFile = new System.Threading.Timer(CheckUpdateTimeDirectory, null, new TimeSpan(0, 0, 2, 0),
                    new TimeSpan(0, 0, 2, 0));
            }
        }

        /// <summary>
        /// Отправка файла Log
        /// </summary>
        private bool SendLogData()
        {
            try
            {
                var httpWebResponse = GetConnection();
                if (HttpStatusCode.OK != httpWebResponse?.StatusCode) return false;
                httpWebResponse.Close();
                return System.IO.File.Exists(Logger.Logger.PathFileLog) && PostSendLogFiles();
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex);
                return false;
            }
        }

        /// <summary>
        /// Отправка файла Log по адресу Resources.UploadJsonLogFiles
        /// </summary>
        private bool PostSendLogFiles()
        {
            var nameFileJson = Guid.NewGuid() + Resources.FileExtensionLog;
            var jsonByte = System.IO.File.ReadAllBytes(Logger.Logger.PathFileLog);
            var dictionary = new Dictionary<string, byte[]> {{nameFileJson, jsonByte}};
            var address = ConfigurationManager.AppSettings["ServerAddress"] +
                          ConfigurationManager.AppSettings["AddressUploadLogFiles"];
            if (SetFile(address, dictionary))
            {
                System.IO.File.Delete(Logger.Logger.PathFileLog);
                Logger.Logger.Info($" Log файл {nameFileJson} отправлен");
                return true;
            }

            Logger.Logger.Info($"Log файл {nameFileJson} не отправлен");
            return false;
        }

#endregion

#region Обновление Справочника

        /// <summary>
        /// Проверка времени обновления справочника
        /// </summary>
        private void CheckUpdateTimeDirectory(object myObject)
        {
            if (System.IO.File.Exists(directoryDataModelFilePath))
            {
                var dataCreation = System.IO.File.GetLastWriteTime(directoryDataModelFilePath);
                var dataNow = DateTime.Now;
                var lastUpdate = dataNow - dataCreation;
                if (lastUpdate >= TimeSpan.FromDays(1))
                {
                    if (GetFileDirectoryDataBase())
                    {
                        // получили и заменили справочник
                        timerGetDirectoryDataModel = new System.Threading.Timer(CheckUpdateTimeDirectory, null,
                            new TimeSpan(0, 24, 0, 0), new TimeSpan(0, 24, 0, 0));
                        return;
                    }

                    // не удалось получить справочник
                    timerGetDirectoryDataModel = new System.Threading.Timer(CheckUpdateTimeDirectory, null,
                        new TimeSpan(0, 0, 2, 0), new TimeSpan(0, 0, 2, 0));
                }
            }
        }

        /// <summary>
        /// Получение информации о справочниках и программе
        /// </summary>
        public FileInfo[] GetInfo()
        {
#if DEBUG
            var pathClientApp =
                @"D:\Agent\Dev\Src\Agent\Diascan.Agent.ClientApp\bin\Debug\Diascan.Agent.ClientApp.exe"; // для теста*/
#else
            var pathClientApp = Application.StartupPath + @"\Diascan.Agent.ClientApp.exe";
#endif
            if (System.IO.File.Exists(directoryDataModelFilePath) && System.IO.File.Exists(pathClientApp))
            {
                var fileInfoClientApp = new FileInfo(pathClientApp);
                var fileInfoDirectoryDataModel = new FileInfo(directoryDataModelFilePath);

                return new FileInfo[2] {fileInfoClientApp, fileInfoDirectoryDataModel};
            }

            return null;
        }

        /// <summary>
        /// Получить справочник ИС "Стрежень"
        /// </summary>
        public bool GetFileDirectoryDataBase()
        {
            try
            {
                var httpWebResponse = GetConnection();
                if (httpWebResponse != null)
                {
                    if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                    {
                        httpWebResponse.Close();
                        var address = ConfigurationManager.AppSettings["ServerAddress"] +
                                      ConfigurationManager.AppSettings["AddressGetDirectoryDataBaseFile"];
                        var httpWebRequest = WebRequest.Create(address);
                        var streamContent =
                            new StreamContent(httpWebRequest.GetResponse().GetResponseStream() ??
                                              throw new InvalidOperationException());
                        var bufBytes = streamContent.ReadAsByteArrayAsync().Result;

                        using (var fileStream = new FileStream(directoryDataModelFilePath, System.IO.FileMode.Create, FileAccess.Write))
                        {
                            fileStream.Write(bufBytes, 0, bufBytes.Length);
                            fileStream.Close();
                        }
                        streamContent.Dispose();
                        Logger.Logger.Info("Справочник обновлен");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Logger.Info("Не удалось обновить справочник: " + ex);
                return false;
            }
        }

#endregion

        private Dictionary<int, SensorMediaIdentifier> GetSensorMediaIdentifier( Calculation calculation, ControllerHelper controllerHelper )
        {
            var carriers = new Dictionary<int, SensorMediaIdentifier>();
            using ( var dbSensorMediaIdentifier = new LiteDatabase( Application.StartupPath + @"\SensorMediaIdentifiers.db" ) )
            {
                var sensorMediaIdentifierCollection = dbSensorMediaIdentifier.GetCollection<SensorMediaIdentifier>( "SensorMediaIdentifier" ).FindAll();

                //  найти CarrierId
                var carriersId = controllerHelper.GetCarrierId( calculation.OmniFilePath );
                //  находим тип прибора для каждого кериера

                foreach( var sensorMediaIdentifier in sensorMediaIdentifierCollection )
                    foreach( var carrierId in carriersId )
                        if( sensorMediaIdentifier.Id == carrierId )
                            carriers.Add( carrierId, sensorMediaIdentifier );
            }

            return carriers;
        }
    }
}
