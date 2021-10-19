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
using Diascan.NDT.Enums;
using Path = Diascan.Utils.IO.Path;
using FileInfo = Diascan.Utils.IO.FileInfo;

namespace Diascan.Agent.Manager
{
    public class Controller
    {
        private bool connectionFlag = true;
        private BindingList<HeaderCalculation> headersCalculation;
        private Guid currentGlobalId;
        private readonly AccessManager accessManager;

        private System.Threading.Timer timerMain;
        private System.Threading.Timer timerSendData;
        private System.Threading.Timer timerGetDirectoryDataModel;
        private System.Threading.Timer timerGetCarrierData;
        private System.Threading.Timer timerSendLogFile;

        private readonly WorkManager workManager;
        private AnalysisManager.AnalysisManager analysisManager;
        private CancellationTokenSource cts;
        
        //  Инициализация контроллера
        public Controller()
        {
            accessManager = new AccessManager();
            workManager = new WorkManager(UpdateAction, Logger.Logger.Info, SharingEvents.SharingEvents.OnWarnMessage);
            analysisManager = new AnalysisManager.AnalysisManager(UpdateAction, Logger.Logger.Info);
            accessManager.DiascanAgentAccess.OpenConnection();
            headersCalculation = new BindingList<HeaderCalculation>();

            // таймеры
            timerMain = new System.Threading.Timer(TimerMain, null, TimeSpan.Zero, new TimeSpan(0, 0, 0, 2));
            timerSendData = new System.Threading.Timer(TimerSendCalc, null, TimeSpan.Zero, new TimeSpan(0, 0, 2, 0));
            timerGetDirectoryDataModel = new System.Threading.Timer(CheckUpdateTimeDirectory, null, TimeSpan.Zero, new TimeSpan(0, 24, 0, 0));
            timerGetCarrierData = new System.Threading.Timer(CheckUpdateTimeDirectoryCarrierData, null, TimeSpan.Zero, new TimeSpan(0, 24, 0, 0));
            timerSendLogFile = new System.Threading.Timer(CheckTimeSendLog, null, TimeSpan.Zero, new TimeSpan(0, 24, 0, 0));
        }

        public BindingList<HeaderCalculation> Load()
        {
            var calculations = accessManager.DiascanAgentAccess.CalculationAccess.GetAll();
            foreach (var calculation in calculations)
            {
                var headerCalculation = new HeaderCalculation(calculation, Logger.Logger.Info);
                headersCalculation.Add(headerCalculation);
            }
            return headersCalculation;
        }

        public void TransferNewAddress(string omniFilePath, ReferenceInputData referenceInputData )
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
                AddNewCalculation(omniFilePath, referenceInputData);
        }

        private void AddNewCalculation(string omniFilePath, ReferenceInputData referenceInputData)
        {
            var path = Path.GetDirectoryName(omniFilePath);
            try
            {
                referenceInputData.AccountUserName = Logger.Logger.Username;
                referenceInputData.ComputerName = SystemInformation.ComputerName;

                var calc = new Calculation(path)
                {
                    GlobalId = Guid.NewGuid(),
                    TimeAddCalculation = DateTime.Now,
                    OmniFilePath = omniFilePath,
                    DataOutput = referenceInputData,
                    Carriers = GetUniqueCarrierData(omniFilePath)
                };

                accessManager.DiascanAgentAccess.CalculationAccess.Insert(calc);
                var headerCalculation = new HeaderCalculation(calc, Logger.Logger.Info);
                headersCalculation.Add(headerCalculation);
                var mes = $"(Добавление нового расчета, путь к данным: {path}) " +
                          $"(Индификатор расчета: {calc.GlobalId}) " +
                          $"(Название (код) прогона: {calc.DataOutput.WorkItemName}) " +
                          $"(Дефектоскоп: {calc.DataOutput.FlawDetector}) " +
                          $"(Заказчик: {calc.DataOutput.Contractor.Value}) " +
                          $"(Трубопровод: {calc.DataOutput.PipeLine.Value}) " +
                          $"(Участок: {calc.DataOutput.Route.Value}) " +
                          $"(Диамерт: {calc.DataOutput.Diameter}) " +
                          $"(Дата пропуска: {calc.DataOutput.DateWorkItem}) " +
                          $"(Ответственный за пропуск: {calc.DataOutput.ResponsibleWorkItem}) " +
                          $"(Конец камеры пуска: {calc.DataOutput.TriggerChamber}) " +
                          $"(Начало камеры приема: {calc.DataOutput.ReceptionChamber})"+
                          $"(Длина участка по ТЗ (м) (фактическая длина): {calc.DataOutput.PlotLengthTechSpec})"+
                          $"(Наличие приемной задвижки: {calc.DataOutput.GateValve})";
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

            var headers = headersCalculation.Where(q => !q.Сompleted).ToArray();
            for (var i = 0; i < headers.Length; i++)
            {
                if (!Directory.Exists(headers[i].Path)) continue;
                var calculation = accessManager.DiascanAgentAccess.CalculationAccess.GetCalculationByGlobalId(headers[i].GlobalId);

                currentGlobalId = calculation.GlobalId;
                cts = new CancellationTokenSource();
#if !DEBUG
                AgentErrorHandler.TryCatch(() =>
                {
#endif
                try
                {
                    workManager.DoWork(calculation, cts);
                    DoAnalysis(calculation);
                }
                catch (Exception) when ((uint)Marshal.GetExceptionCode() == AgentErrorHandler.Err)
                {
                    Logger.Logger.Info(
                        $"{calculation.DataOutput.WorkItemName}: Ошибка расчета: {AgentErrorHandler.Err}");
                }

#if !DEBUG
                });
#endif

                if (calculation.WorkState.HasFlag(enWorkState.Error))
                {
                    var path = calculation.SourcePath;
                    var info = $"Отмена расчета, путь к данным: {path} ";
                    SharingEvents.SharingEvents.OnWarnMessage(info);
                    Logger.Logger.Info(info);
                }

                var url = ConfigurationManager.AppSettings["ServerAddress"] +
                          ConfigurationManager.AppSettings["AddressUploadJsonLDIFiles"];
                var urlTest = ConfigurationManager.AppSettings["ServerAddress"] +
                              ConfigurationManager.AppSettings["AddressCheckingAccessResource"];
                if (WorkManager.TestConnection(urlTest))
                    workManager.SendCalculation(calculation, url);
                currentGlobalId = Guid.Empty;
            }

            timerMain = new System.Threading.Timer(TimerMain, null, new TimeSpan(0, 0, 0, 2), new TimeSpan(0, 0, 0, 2));
        }

        private void DoAnalysis(Calculation calculation)
        {
            if ( calculation.WorkState.HasFlag(enWorkState.Error)                  ||
                !calculation.State.HasFlag(enCalculationStateTypes.HaltingSensors) ||
                 calculation.State.HasFlag(enCalculationStateTypes.Analysis)) return;

            var analysisCalculation = analysisManager.DoAnalysis(calculation);
        }

        //  Сохранение сalculation в БД
        public void UpdateAction(Calculation calculation)
        {
            try
            {
                var headerCalculation = headersCalculation.Where(q => q.GlobalId == calculation.GlobalId)?.FirstOrDefault();
                if (headerCalculation != null)
                {
                    headerCalculation.Change(calculation);
                }
                else
                    Logger.Logger.Info($"Элемент с таким GlobalId: {calculation.GlobalId} не найден!");

                accessManager.DiascanAgentAccess.CalculationAccess.Update(calculation);
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
        public List<Calculation> GetBdForm()
        {
            return accessManager.DiascanAgentAccess.CalculationAccess.GetAll().ToList();
        }

        // Получить Json по индексу
        public byte[] CalculationToJson(int id)
        {
            var collection = accessManager.DiascanAgentAccess.CalculationAccess.GetCalculationById(id);
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
            accessManager.DiascanAgentAccess.CalculationAccess.DeleteByGlobalId(id);
            var headerCalculation = headersCalculation.Where(q => q.GlobalId == id)?.FirstOrDefault();
            headersCalculation.Remove(headerCalculation);
        }


        public byte[] ExportToExcel(int id, string path)
        {
            var calculation = accessManager.DiascanAgentAccess.CalculationAccess.GetCalculationById(id);
            var analysisCalculation = analysisManager.DoAnalysis(calculation);
            return new ExportToExcel(Logger.Logger.Info).CreateExcelFile(analysisCalculation, calculation, out var result);
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
            foreach (var headerCalculation in headersCalculation)
            {
                var calc = accessManager.DiascanAgentAccess.CalculationAccess.GetCalculationByGlobalId(headerCalculation.GlobalId);
                if (!(calc.State.HasFlag(enCalculationStateTypes.FindTypes) &&
                      calc.State.HasFlag(enCalculationStateTypes.Hashe) &&
                      calc.State.HasFlag(enCalculationStateTypes.HaltingSensors)&&
                      calc.State.HasFlag(enCalculationStateTypes.Analysis))||
                    calc.State.HasFlag(enCalculationStateTypes.Sended) || calc.WorkState.HasFlag(enWorkState.Error)) continue; 

                 var url = ConfigurationManager.AppSettings["ServerAddress"] +
                          ConfigurationManager.AppSettings["AddressUploadJsonLDIFiles"];
                var urlTest = ConfigurationManager.AppSettings["ServerAddress"] +
                              ConfigurationManager.AppSettings["AddressCheckingAccessResource"];

                if (WorkManager.TestConnection(urlTest))
                    workManager.SendCalculation(calc, url);
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
                var fileInfoClientApp = new FileInfo(pathClientApp);
                var fileInfoDirectoryDataModel = new FileInfo(FullDbFilePath(Resources.DataModel));
                var fileInfoCarrierData = new FileInfo(FullDbFilePath(Resources.CarrierData));

                return new [] {fileInfoClientApp, fileInfoDirectoryDataModel, fileInfoCarrierData};
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

        #endregion
    }
}
