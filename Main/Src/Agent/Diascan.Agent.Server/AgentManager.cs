using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using Diascan.Agent.ModelDB;
using Diascan.NDT.Enums;

namespace Diascan.Agent.Manager
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class AgentManager
    {
        private Controller controller;
        
        public AgentManager()
        {
            Logger.Logger.InitLogger(Application.StartupPath);
            Logger.Logger.Info("Инициализация Diascan.Agent.Service");
            var license = new Aspose.Cells.License();
            license.SetLicense(new MemoryStream(Properties.Resources.Aspose_Total));
            controller = new Controller();

            //  Объявление событий
            SharingEvents.SharingEvents.SentPdi += SentPDI;
            SharingEvents.SharingEvents.GetPdiCustomers += GetPdiCustomers;
            SharingEvents.SharingEvents.GetPdiPipelines += GetPdiPipelines;
            SharingEvents.SharingEvents.GetPdiRoutes += GetPdiRoutes;
            SharingEvents.SharingEvents.GetPdiDiameter += GetPdiDiameter;
            SharingEvents.SharingEvents.GetCalculations += GetCalculations;
            SharingEvents.SharingEvents.Canseled += Canseled;
            SharingEvents.SharingEvents.ExportToExcel += ExportToExcel;
            SharingEvents.SharingEvents.GetAddressConnection += GetAddressConnection;
            SharingEvents.SharingEvents.ChangeAddressConnection += ChangeAddressConnection;
            SharingEvents.SharingEvents.TestConnection += TestConnection;
            SharingEvents.SharingEvents.DirectoryUpdate += DirectoryUpdate;
            SharingEvents.SharingEvents.GetInfo += GetInfo;
            SharingEvents.SharingEvents.ExportToJeson += ExportToJeson;
        }

        //  Новый расчет
        public void SentPDI(string omniFilePath, ReferenceInputData referenceInputData)
        {
            Task.Run(() =>
            {
                controller.TransferNewAddress(omniFilePath, referenceInputData);
            });
        }
        
        //  Отмена расчета
        public Task Canseled(int id)
        {
            try
            {
                return Task.Run(() =>
                {
                    Logger.Logger.Info($"Отмена расчета, путь к данным: {controller.Canseled(id)}");
                });
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw new FaultException(ex.Message);
            }
        }

        // Обновление справочника
        public Task DirectoryUpdate()
        {
            try
            {
                return Task.Run( () =>
                {
                    Logger.Logger.Info( $"Обновление справочника : {controller.GetFileDirectoryDataBase()}" );
                } );
            }
            catch( Exception ex )
            {
                Logger.Logger.Info( ex.Message );
                throw new FaultException( ex.Message );
            }
        }


        // Получение информации по программе и справочнику
        public Task<FileInfo[]> GetInfo()
        {
            try
            {
                return Task.Run( () => controller.GetInfo());
            }
            catch( Exception ex )
            {
                Logger.Logger.Info( ex.Message );
                throw new FaultException( ex.Message );
            }
        }


        //  Экспорт результата в Json
        public Task ExportToJeson( int id, string path )
        {
            try
            {
                return Task.Run( () =>
                {
                    Logger.Logger.Info( "Создание файла Json'на " );

                    System.IO.File.WriteAllBytes( $"{path}",  controller.CalculationToJson( id ) );
                } );
            }
            catch( Exception ex )
            {
                Logger.Logger.Info( ex.Message );
                throw new FaultException( ex.Message );
            }
        }


        //  Экспорт результата в Excel
        public Task ExportToExcel(int id, string path)
        {
            try
            {
                return Task.Run(() =>
                {
                    Logger.Logger.Info("Создание файла отчета...");
                    System.IO.File.WriteAllBytes($"{path}", new ExportToExcel().CreateExcelFile(controller.GetCalculation(id), out var result));
                });
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw new FaultException(ex.Message);
            }
        }

        //  получить справочные данные Customers
        public Task<Dictionary<Guid, string>> GetPdiCustomers()
        {
            try
            {
                var dataModelHelper = new DataModelHelper(Application.StartupPath);
                return Task.Run(() =>
                {
                    var customers = dataModelHelper.GetAllCustomers();
                    dataModelHelper.Dispose();
                    return customers;
                });
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw new FaultException(ex.Message);
            }
        }

        //  получить справочные данные Pipelines
        public Task<Dictionary<Guid, string>> GetPdiPipelines(Guid customerGuid)
        {
            try
            {
                var dataModelHelper = new DataModelHelper(Application.StartupPath);
                return Task.Run(() =>
                {
                    var pipelines = dataModelHelper.GetPipelines(customerGuid);
                    dataModelHelper.Dispose();
                    return pipelines;
                });
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw new FaultException(ex.Message);
            }
        }

        //  получить справочные данные Routes
        public Task<Dictionary<Guid, string>> GetPdiRoutes(Guid customerGuid, Guid pipelineGuid)
        {
            try
            {
                var dataModelHelper = new DataModelHelper(Application.StartupPath);
                return Task.Run(() =>
                {
                    var routes = dataModelHelper.GetRoutes(customerGuid, pipelineGuid);
                    dataModelHelper.Dispose();
                    return routes;
                });
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw new FaultException(ex.Message);
            }
        }
        //  получить справочные данные Diameter
        public Task<float> GetPdiDiameter( Guid routeGuidGuid )
        {
            try
            {
                var dataModelHelper = new DataModelHelper(Application.StartupPath);

                return Task.Run(() =>
                {
                    var diameter = dataModelHelper.GetDiameter( routeGuidGuid );
                    dataModelHelper.Dispose();
                    return diameter;
                });
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw new FaultException(ex.Message);
            }
        }
        //  протестировать соединение
        public Task<bool> TestConnection(string newAddress)
        {
            try
            {
                return Task.Run(() => controller.TestConnection(newAddress));
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw new FaultException(ex.Message);
            }
        }
        //  Получить адрес подключения к серверу
        public Task<string> GetAddressConnection()
        {
            try
            {
                return Task.Run(() => controller.GetAddressConnection());
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw new FaultException(ex.Message);
            }
        }
        //  изменить адрес подключения
        public Task ChangeAddressConnection(string newAddress)
        {
            try
            {
                return Task.Run(() => controller.ChangeAddressConnection(newAddress));
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw new FaultException(ex.Message);
            }
        }

        //  Передать данные на форму
        public Task<TableRowData[]> GetCalculations()
        {
            try
            {
                return Task.Run(() =>
                {
                    var calculations = controller.GetBdForm();
                    var newTableRowData = new TableRowData[calculations.Count];

                    for (var i = 0; i < calculations.Count; i++)
                    {
                        var state = ProgressPreliminaryCalculations(calculations[i]) +
                                    ProgressNavigation(calculations[i]) +
                                    ProgressPdiCalculations(calculations[i]) +
                                    ProgressCdlTail(calculations[i]);
                       
                        newTableRowData[i] = new TableRowData
                                             {
                                                 Id       = calculations[i].Id,
                                                 RunCode = calculations[i].DataOutput.WorkItemName,
                                                 Path     = calculations[i].SourcePath,
                                                 State  = state,
                                                 Status = StatusOfCalculate(calculations[i]),
                                                 Restart = calculations[i].RestartReport.Count != 0,
                                                 DateTime = calculations[i].TimeAddCalculation
                                             };
                    }
                    return newTableRowData;
                });
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw new FaultException(ex.Message);
            }
        }

        private string ProgressPreliminaryCalculations(Calculation calculation)
        {
            return $"Предварительные вычисления: {calculation.Helper.ProgressHashes}";
        }

        private string ProgressNavigation(Calculation calculation)
        {
            if (calculation.NavigationInfo.NavigationState.HasFlag(NavigationStateTypes.NavigationData))
            {
                return $"{Environment.NewLine}Расчет навигаци: {Math.Round(calculation.Helper.ProgressNavData * 100, 2)}%";
            }
            else
                return "";
        }

        private string ProgressPdiCalculations(Calculation calculation)
        {
            double persentPdi = 0;
            var count = 0;
            foreach (var diagData in calculation.DiagDataList)
            {
                if (diagData.DataType == DataType.Nav) continue;
                var distStop = diagData.ProcessedDist < 0 ? 0 : diagData.ProcessedDist;
                persentPdi += distStop / diagData.MaxDistance * 100;
                count++;
            }

            if (count == 0) return "";
            persentPdi = persentPdi / count;
            var str = double.IsNaN(persentPdi) ? "0" : Math.Round(persentPdi, 2).ToString();
            return $"{Environment.NewLine}Поиск ПДИ: {str}%";
        }

        private string ProgressCdlTail(Calculation calculation)
        {
            return calculation.Helper.ProgressCdlTail == null
                ? ""
                : $"{Environment.NewLine}Определение типа CD: {calculation.Helper.ProgressCdlTail}";
        }

        private string StatusOfCalculate(Calculation calculation)
        {
            return (calculation.State == (CalculationStateTypes.FindTypes |
                                         CalculationStateTypes.Calculated |
                                         CalculationStateTypes.Hashed |
                                         CalculationStateTypes.Speed |
                                         CalculationStateTypes.CdlTail |
                                         CalculationStateTypes.DefinitionOfRerun |
                                         CalculationStateTypes.Transferred) &&
                                         calculation.NavigationInfo.NavigationState.HasFlag(NavigationStateTypes.CalcNavigation)) 
                ? "Отправлено" : "Не отправлено";
        }
    }
}
