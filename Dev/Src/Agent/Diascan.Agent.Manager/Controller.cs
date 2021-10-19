using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Diascan.Agent.Manager.Properties;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;
using Diascan.Agent.ErrorHandler;
using Diascan.Agent.TaskManager;
using System.IO;
using Diascan.Agent.AnalysisManager;
using Diascan.Agent.FuzzySearch;
using Diascan.Agent.LiteDbAccess;
using Diascan.Agent.Types;
using Diascan.Agent.Types.ModelCalculationDiagData;
using DiCore.Lib.NDT.Types;
using Path = Diascan.Utils.IO.Path;
using FileInfo = Diascan.Utils.IO.FileInfo;

namespace Diascan.Agent.Manager
{
    public class Controller
    {
        private bool connectionFlag = true;
        private BindingList<HeaderSession> headersSession;
        private Guid currentGlobalId;
        private readonly AccessManager accessManager;

        private System.Threading.Timer timerMain;
        private System.Threading.Timer timerSendData;
        private System.Threading.Timer timerGetDirectoryDataModel;
        private System.Threading.Timer timerGetCarrierData;
        private System.Threading.Timer timerGetCarriers;
        private System.Threading.Timer timerSendLogFile;

        private readonly WorkManager workManager;
        private AnalysisManager.AnalysisManager analysisManager;
        private CancellationTokenSource cts;
        
        //  Инициализация контроллера
        public Controller()
        {
            accessManager   = new AccessManager();
            workManager     = new WorkManager(UpdateAction, Logger.Logger.Info, SharingEvents.SharingEvents.OnWarnMessage);
            analysisManager = new AnalysisManager.AnalysisManager(UpdateAction, Logger.Logger.Info);
            accessManager.DiascanAgentAccess.OpenConnection();
            headersSession  = new BindingList<HeaderSession>();

            // таймеры
            timerMain                  = new System.Threading.Timer(TimerMain, null, TimeSpan.Zero, new TimeSpan(0, 0, 0, 2));
            timerSendData              = new System.Threading.Timer(TimerSendCalc, null, TimeSpan.Zero, new TimeSpan(0, 0, 2, 0));
            timerGetDirectoryDataModel = new System.Threading.Timer(CheckUpdateTimeDirectory, null, TimeSpan.Zero, new TimeSpan(0, 24, 0, 0));
            timerGetCarrierData        = new System.Threading.Timer(CheckUpdateTimeDirectoryCarrierData, null, TimeSpan.Zero, new TimeSpan(0, 24, 0, 0));
            timerGetCarriers           = new System.Threading.Timer(CheckUpdateTimeDirectoryCarriers, null, TimeSpan.Zero, new TimeSpan(0, 24, 0, 0));
            timerSendLogFile           = new System.Threading.Timer(CheckTimeSendLog, null, TimeSpan.Zero, new TimeSpan(0, 24, 0, 0));
        }

        public BindingList<HeaderSession> Load()
        {
            var sessions = accessManager.DiascanAgentAccess.SessionAccess.GetAll();
            foreach (var session in sessions)
            {
                var headerSession = new HeaderSession(session, Logger.Logger.Info);
                headersSession.Add(headerSession);
            }
            return headersSession;
        }

        public void TransferNewAddress(string omniFilePath, ReferenceInputData referenceInputData, List<DataLocation> datasetLocation)
        {
            var path = Path.GetDirectoryName(omniFilePath);

            if (Path.GetFileName(path) != Path.GetFileNameWithoutExtension(omniFilePath))
            {
                var strMess = "Не удалось создать расчет. Данные для расчета не найдены";
                SharingEvents.SharingEvents.OnErrorMessage(strMess);
                Logger.Logger.Error(strMess + $@": {path}");
                return;
            }
            if (Directory.Exists(path))
                AddNewCalculation(omniFilePath, referenceInputData, datasetLocation);
        }

        private void AddNewCalculation(string omniFilePath, ReferenceInputData referenceInputData, List<DataLocation> datasetLocation)
        {
            try
            {
                referenceInputData.AccountUserName = Logger.Logger.Username;
                referenceInputData.ComputerName = SystemInformation.ComputerName;

                var session = new Session(Guid.NewGuid());
                foreach (var dataLocation in datasetLocation)
                {
                    session.Calculations.Add(new Calculation()
                    {
                        GlobalId = session.GlobalID,
                        DataOutput = referenceInputData,
                        Carriers = GetUniqueCarrierData(omniFilePath),
                        DataLocation = dataLocation
                    });
                }

                session.BaseFile = session.Calculations[0].DataLocation.FullPath;
                session.BasePath = session.Calculations[0].DataLocation.BaseDirectory;

                accessManager.DiascanAgentAccess.SessionAccess.Insert(session);
                var headerSession = new HeaderSession(session, Logger.Logger.Info);
                headersSession.Add(headerSession);

                var mes = $"(Добавление нового расчета, путь к данным: {session.Calculations[0].DataLocation.BaseDirectory}) \r\n" +
                          $"(Индификатор расчета: {session.GlobalID}) \r\n" +
                          $"(Название (код) прогона: {session.Calculations[0].DataOutput.WorkItemName}) \r\n" +
                          $"(Дефектоскоп: {session.Calculations[0].DataOutput.FlawDetector}) \r\n" +
                          $"(Заказчик: {session.Calculations[0].DataOutput.Contractor.Value}) \r\n" +
                          $"(Трубопровод: {session.Calculations[0].DataOutput.PipeLine.Value}) \r\n" +
                          $"(Участок: {session.Calculations[0].DataOutput.Route.Value}) \r\n" +
                          $"(Диамерт: {session.Calculations[0].DataOutput.Diameter}) \r\n" +
                          $"(Дата пропуска: {session.Calculations[0].DataOutput.DateWorkItem.ToShortDateString()}) \r\n" +
                          $"(Ответственный за пропуск: {session.Calculations[0].DataOutput.ResponsibleWorkItem}) \r\n" +
                          $"(Конец камеры пуска: {session.Calculations[0].DataOutput.TriggerChamber}) \r\n" +
                          $"(Начало камеры приема: {session.Calculations[0].DataOutput.ReceptionChamber}) \r\n" +
                          $"(Длина участка по ТЗ (м) (фактическая длина): {session.Calculations.Aggregate(string.Empty,(current, calculation)=> current + calculation.DataOutput.InspectionDirNameSectLengTechTask[calculation.DataLocation.InspectionDirName] + " - " + calculation.DataLocation.InspectionDirName + "; ")}) \r\n" +
                          $"(Список CarrierId из omni файла: {session.Calculations[0].DataOutput.OmniCarrierIds.Aggregate(string.Empty, (current, omniCarrierId) => current + omniCarrierId + "; ")}) \r\n" +
                          $"(Наличие приемной задвижки: {session.Calculations[0].DataOutput.GateValve}) \r\n";
                Logger.Logger.Info(mes);
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex);
                SharingEvents.SharingEvents.OnErrorMessage(ex.Message);
            }
        }

        private void TimerMain(object sender)
        {
            timerMain.Dispose();

            var headers = headersSession.Where(q => !q.Сompleted).ToArray();
            for (var i = 0; i < headers.Length; i++)
            {
                if (!Directory.Exists(headers[i].Path)) continue;
                var session = accessManager.DiascanAgentAccess.SessionAccess.GetCalculationByGlobalId(headers[i].GlobalId);
                if (session == null || session.Calculations.Aggregate(false, (current, calculation) => current | calculation.WorkState.HasFlag(enWorkState.Error)))
                    continue;

                currentGlobalId = session.GlobalID;
                cts             = new CancellationTokenSource();
#if !DEBUG
                AgentErrorHandler.TryCatch(() =>
                {
#endif
                try
                {
                    workManager.DoWork(session, cts);
                    DoAnalysis(session);
                }
                catch (Exception) when ((uint)Marshal.GetExceptionCode() == AgentErrorHandler.Err)
                {
                    Logger.Logger.Info($"{session.Calculations[0].DataOutput.WorkItemName}: Ошибка расчета: {AgentErrorHandler.Err}");
                }
#if !DEBUG
                });
#endif

                if (session.Calculations.Aggregate(false, (current, calculation) => current | calculation.WorkState.HasFlag(enWorkState.Error)))
                {
                    var path = session.BasePath;
                    var info = $"Ошибка в расчете. Расчет остановлен. Путь к данным: {path}";
                    SharingEvents.SharingEvents.OnWarnMessage(info);
                    Logger.Logger.Info(info);
                    break;
                }

                if (session.Calculations.Aggregate(false, (current, calculation) => current | (calculation.WorkState.HasFlag(enWorkState.Error)                     ||
                                                                                              (!calculation.State.HasFlag(enCalculationStateTypes.Analysis)           &&
                                                                                               !calculation.State.HasFlag(enCalculationStateTypes.HaltingSensors)))))

                {
                    currentGlobalId = Guid.Empty;
                    break;
                }
                else
                {
                    var url     = ConfigurationManager.AppSettings["ServerAddress"] + ConfigurationManager.AppSettings["AddressUploadJsonLDIFiles"];
                    var urlTest = ConfigurationManager.AppSettings["ServerAddress"] + ConfigurationManager.AppSettings["AddressCheckingAccessResource"];
                    if (WorkManager.TestConnection(urlTest))
                        workManager.SendCalculation(session, url);
                }

                currentGlobalId = Guid.Empty;
            }

            timerMain = new System.Threading.Timer(TimerMain, null, new TimeSpan(0, 0, 0, 2), new TimeSpan(0, 0, 2, 0));
        }

        private void StateErrorOrСompleted(List<Calculation> calculations)
        {
            var stateError = calculations.Aggregate(false, (current, calculation) => current | calculation.WorkState.HasFlag(enWorkState.Error));
        }

        private void DoAnalysis(Session session)
        {
            if (session.Calculations.Aggregate(false, (current, calculation) => current | (calculation.WorkState.HasFlag(enWorkState.Error) ||
                                                                                           !calculation.State.HasFlag(enCalculationStateTypes.HaltingSensors) ||
                                                                                           calculation.State.HasFlag(enCalculationStateTypes.Analysis)
                                                                                          )))
                return;

            var analysisCalculation = analysisManager.DoAnalysis(session);
        }

        //  Сохранение сalculation в БД
        public void UpdateAction(Session session)
        {
            try
            {
                var headerSession = headersSession.Where(q => q.GlobalId == session.GlobalID)?.FirstOrDefault();
                if (headerSession != null)
                {
                    headerSession.Change(session);
                }
                else
                    Logger.Logger.Info($"Элемент с таким GlobalId: {session.GlobalID} не найден!");

                accessManager.DiascanAgentAccess.SessionAccess.Update(session);
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex);
            }
        }
        
        private List<CarrierData> GetUniqueCarrierData(string omniFilePath)
        {
            accessManager.CarrierDataModelAccess.OpenConnection();
            var carries = accessManager.CarrierDataModelAccess.CarrierAccess.GetAll();
            var carrierData = TaskHelper.GetCarrierData(omniFilePath, carries.ToList());
            accessManager.CarrierDataModelAccess.CloseConnection();
            return carrierData;
        }
        
        //  Возвращает массив для отображения в форме
        public List<Session> GetBdForm()
        {
            return accessManager.DiascanAgentAccess.SessionAccess.GetAll().ToList();
        }

        // Получить Json по индексу
        public byte[] CalculationToJson(int id)
        {
            var collection = accessManager.DiascanAgentAccess.SessionAccess.GetCalculationById(id);
            var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead };
            var json = JsonConvert.SerializeObject( collection, jsonSettings);

            return Encoding.UTF8.GetBytes( json );
        }

        private void DoWorkStoped(Guid id)
        {
            if(currentGlobalId == id)
                cts?.Cancel();
        }

        //  Удаление записи и отмена выполнения задачи по Id
        public void Canseled(Guid id)
        {
            DoWorkStoped(id);
            accessManager.DiascanAgentAccess.SessionAccess.DeleteByGlobalId(id);
            var headerCalculation = headersSession.Where(q => q.GlobalId == id)?.FirstOrDefault();
            headersSession.Remove(headerCalculation);
        }


        public byte[] ExportToExcel(int id, string path)
        {
            var session = accessManager.DiascanAgentAccess.SessionAccess.GetCalculationById(id);
            var dictionaryAnalysisCalculation = analysisManager.DoAnalysis(session);
            return new ExportToExcel(Logger.Logger.Info).CreateExcelFile(dictionaryAnalysisCalculation, session.Calculations, out var result);
        }

        public DataModel[] GetAllDataModel()
        {
            using (var dataModelHelper = new DataModelHelper(accessManager))
            {
                return dataModelHelper.GetAllDataModel();
            }
        }

        public float[] GetAllDiameters()
        {
            using (var dataModelHelper = new DataModelHelper(accessManager))
            {
                return dataModelHelper.GetAllDiameters();
            }
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

        public void TimerSendCalc(object obj)
        {
            foreach (var headerSession in headersSession)
            {
                var session = accessManager.DiascanAgentAccess.SessionAccess.GetCalculationByGlobalId(headerSession.GlobalId);

                if (session.Calculations.Aggregate(false, (current, calculation) => current | ( !(calculation.State.HasFlag(enCalculationStateTypes.FindTypes)      &&
                                                                                                  calculation.State.HasFlag(enCalculationStateTypes.Hashe)          &&
                                                                                                  calculation.State.HasFlag(enCalculationStateTypes.HaltingSensors) &&
                                                                                                  calculation.State.HasFlag(enCalculationStateTypes.Analysis))      ||
                                                                                                calculation.State.HasFlag(enCalculationStateTypes.Sended)           ||
                                                                                                calculation.WorkState.HasFlag(enWorkState.Error)
                                                                                               ))) continue;

                var url = ConfigurationManager.AppSettings["ServerAddress"] +
                              ConfigurationManager.AppSettings["AddressUploadJsonLDIFiles"];
                    var urlTest = ConfigurationManager.AppSettings["ServerAddress"] +
                                  ConfigurationManager.AppSettings["AddressCheckingAccessResource"];

                    if (WorkManager.TestConnection(urlTest))
                        workManager.SendCalculation(session, url);
            }
        }

        private string FullDbFilePath(string dbName)
        {
            return string.Concat(Application.StartupPath, @"\", dbName);
        }

        #region Отправка и обновление Log файла

        /// <summary>
        /// Проверка времени Log файла
        /// </summary>
        private void CheckTimeSendLog(object myObject)
        {
            if (!System.IO.File.Exists(Logger.Logger.PathFile))
                System.IO.File.Create(Logger.Logger.PathFile);

            var dataCreation = System.IO.File.GetCreationTime(Logger.Logger.PathFile);
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
                return System.IO.File.Exists(Logger.Logger.PathFile) && PostSendLogFiles();
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
            var fileNameJson = Guid.NewGuid() + Resources.FileExtensionLog;
            var data = System.IO.File.ReadAllBytes(Logger.Logger.PathFile);
            var url = ConfigurationManager.AppSettings["ServerAddress"] + ConfigurationManager.AppSettings["AddressUploadLogFiles"];
            if (workManager.SetFile(url, fileNameJson, data))
            {
                System.IO.File.Delete(Logger.Logger.PathFile);
                Logger.Logger.Info($" Log файл {fileNameJson} отправлен");
                return true;
            }

            Logger.Logger.Info($"Log файл {fileNameJson} не отправлен");
            return false;
        }

#endregion

        #region Обновление Справочника

        /// <summary>
        /// Проверка времени обновления справочника
        /// </summary>
        private void CheckUpdateTimeDirectory(object myObject)
        {
            if (System.IO.File.Exists(FullDbFilePath(Resources.DataModel)))
            {
                var dataCreation = System.IO.File.GetLastWriteTime(FullDbFilePath(Resources.DataModel));
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
        /// Проверка времени обновления справочника
        /// </summary>
        private void CheckUpdateTimeDirectoryCarriers(object myObject)
        {
            if (System.IO.File.Exists(FullDbFilePath(Resources.Carriers)))
            {
                var dataCreation = System.IO.File.GetLastWriteTime(FullDbFilePath(Resources.Carriers));
                var dataNow = DateTime.Now;
                var lastUpdate = dataNow - dataCreation;
                if (lastUpdate >= TimeSpan.FromDays(1))
                {
                    if (GetCarriers())
                    {
                        // получили и заменили справочник
                        timerGetCarriers = new System.Threading.Timer(CheckUpdateTimeDirectoryCarriers, null,
                            new TimeSpan(0, 24, 0, 0), new TimeSpan(0, 24, 0, 0));
                        return;
                    }

                    // не удалось получить справочник
                    timerGetCarriers = new System.Threading.Timer(CheckUpdateTimeDirectoryCarriers, null,
                        new TimeSpan(0, 0, 2, 0), new TimeSpan(0, 0, 2, 0));
                }
            }
        }


        /// <summary>
        /// Проверка времени обновления справочника
        /// </summary>
        private void CheckUpdateTimeDirectoryCarrierData(object myObject)
        {
            if (System.IO.File.Exists(FullDbFilePath(Resources.CarrierData)))
            {
                var dataCreation = System.IO.File.GetLastWriteTime(FullDbFilePath(Resources.CarrierData));
                var dataNow = DateTime.Now;
                var lastUpdate = dataNow - dataCreation;
                if (lastUpdate >= TimeSpan.FromDays(1))
                {
                    if (GetCarrierData())
                    {
                        // получили и заменили справочник
                        timerGetCarrierData = new System.Threading.Timer(CheckUpdateTimeDirectoryCarrierData, null,
                            new TimeSpan(0, 24, 0, 0), new TimeSpan(0, 24, 0, 0));
                        return;
                    }

                    // не удалось получить справочник
                    timerGetCarrierData = new System.Threading.Timer(CheckUpdateTimeDirectoryCarrierData, null,
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
                @"D:\T\Agent\Dev\Src\Agent\Diascan.Agent.ClientApp\bin\Debug\Diascan.Agent.ClientApp.exe"; // для теста*/
#else
            var pathClientApp = Application.StartupPath + @"\Diascan.Agent.ClientApp.exe";
#endif
            if (System.IO.File.Exists(FullDbFilePath(Resources.DataModel)) && System.IO.File.Exists(pathClientApp))
            {
                var fileInfoClientApp          = new FileInfo(pathClientApp);
                var fileInfoDirectoryDataModel = new FileInfo(FullDbFilePath(Resources.DataModel));
                var fileInfoCarrierData        = new FileInfo(FullDbFilePath(Resources.CarrierData));
                var fileInfoCarriers           = new FileInfo(FullDbFilePath(Resources.Carriers));

                return new [] {fileInfoClientApp, fileInfoDirectoryDataModel, fileInfoCarrierData, fileInfoCarriers };
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

                        using (var fileStream = new FileStream(FullDbFilePath(Resources.DataModel), System.IO.FileMode.Create, FileAccess.Write))
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

        /// <summary>
        /// Получить справочник CarrierData
        /// </summary>
        public bool GetCarrierData()
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
                                      ConfigurationManager.AppSettings["AddressGetCarrierDataFile"];
                        var httpWebRequest = WebRequest.Create(address);
                        var streamContent =
                            new StreamContent(httpWebRequest.GetResponse().GetResponseStream() ??
                                              throw new InvalidOperationException());
                        var bufBytes = streamContent.ReadAsByteArrayAsync().Result;

                        using (var fileStream = new FileStream(FullDbFilePath(Resources.CarrierData), System.IO.FileMode.Create, FileAccess.Write))
                        {
                            fileStream.Write(bufBytes, 0, bufBytes.Length);
                            fileStream.Close();
                        }
                        streamContent.Dispose();
                        Logger.Logger.Info("Справочник CarrierData обновлен");
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

        /// <summary>
        /// Получить справочник Carriers
        /// </summary>
        public bool GetCarriers()
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
                                      ConfigurationManager.AppSettings["AddressGetCarriersFile"];
                        var httpWebRequest = WebRequest.Create(address);
                        var streamContent =
                            new StreamContent(httpWebRequest.GetResponse().GetResponseStream() ??
                                              throw new InvalidOperationException());
                        var bufBytes = streamContent.ReadAsByteArrayAsync().Result;

                        using (var fileStream = new FileStream(FullDbFilePath(Resources.Carriers), System.IO.FileMode.Create, FileAccess.Write))
                        {
                            fileStream.Write(bufBytes, 0, bufBytes.Length);
                            fileStream.Close();
                        }
                        streamContent.Dispose();
                        Logger.Logger.Info("Справочник Carriers обновлен");
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
    }
}
