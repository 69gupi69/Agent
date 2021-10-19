using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using Diascan.Agent.AnalysisManager;
using Diascan.Agent.FuzzySearch;
using Diascan.Agent.LiteDbAccess;
using Diascan.Agent.SharingEvents;
using Diascan.Agent.Types;
using Diascan.NDT.Enums;
using Diascan.Utils.IO;
using DiCore.Lib.NDT.DataProviders.NAV;
using DiCore.Lib.NDT.Types;

namespace Diascan.Agent.Manager
{
    public class AgentManager
    {
        private readonly Controller controller;

        public AgentManager()
        {
            Logger.Logger.InitLogger(Application.StartupPath);
            controller = new Controller();
            InitEvents();
        }

        //  Новый расчет
        public void SentPdi(string omniFilePath, ReferenceInputData referenceInputData, List<DataLocation> datasetLocation)
        {
            controller.TransferNewAddress(omniFilePath, referenceInputData,  datasetLocation);
        }
        
        //  Отмена расчета
        public void Canseled(Guid id)
        {
            try
            {
                controller.Canseled(id);
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw;
            }
        }

        // Обновление справочника
        public void DirectoryUpdate()
        {
            try
            {
                Logger.Logger.Info($"Обновление справочника : {controller.GetFileDirectoryDataBase()}");
            }
            catch(Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw;
            }
        }

        // Обновление справочника GetCarrierData
        public void DirectoryCarrierDataUpdate()
        {
            try
            {
                Logger.Logger.Info($"Обновление справочника CarrierData : {controller.GetCarrierData()}");
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw;
            }
        }

        // Обновление справочника GetCarriers
        public void DirectoryCarriersUpdate()
        {
            try
            {
                Logger.Logger.Info($"Обновление справочника Carriers : {controller.GetCarriers()}");
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw;
            }
        }


        // Получение информации по программе и справочнику
        public FileInfo[] GetInfo()
        {
            try
            {
                return controller.GetInfo();
            }
            catch(Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw;
            }
        }


        //  Экспорт результата в Json
        public void ExportToJeson(int id, string path)
        {
            try
            {
                Logger.Logger.Info( "Создание файла Json'на ");
                System.IO.File.WriteAllBytes($"{path}", controller.CalculationToJson(id));
            }
            catch(Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw;
            }
        }


        //  Экспорт результата в Excel
        public void ExportToExcel(int id, string path)
        {
            try
            {
                Logger.Logger.Info("Создание файла отчета...");
                System.IO.File.WriteAllBytes($"{path}", controller.ExportToExcel(id, path));
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw;
            }
        }

        //  получить справочные данные Customers
        public DataModel[] GetAllDataModel()
        {
            try
            {
                return controller.GetAllDataModel();
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw;
            }
        }

        //  получить все диаметры труб
        public float[] GetAllDiameters()
        {
            try
            {
                return controller.GetAllDiameters();
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw;
            }
        }

        //  протестировать соединение
        public bool TestConnection(string newAddress)
        {
            try
            {
                return controller.TestConnection(newAddress);
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw;
            }
        }
        //  Получить адрес подключения к серверу
        public string GetAddressConnection()
        {
            try
            {
                return controller.GetAddressConnection();
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw;
            }
        }
        //  изменить адрес подключения
        public void ChangeAddressConnection(string newAddress)
        {
            try
            {
                controller.ChangeAddressConnection(newAddress);
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
                throw;
            }
        }

        public Task<BindingList<HeaderSession>> FormLoad()
        {
            try
            {
                return Task.Run(() => controller.Load());
            }
            catch (Exception e)
            {
                Logger.Logger.Info(e.Message);
                throw;
            }
        }

        //  Объявление событий
        private void InitEvents()
        {
            //  Объявление событий
            SharingEvents.SharingEvents.SentPdi += SentPdi;
            SharingEvents.SharingEvents.GetAllDataModel += GetAllDataModel;
            SharingEvents.SharingEvents.GetAllDiameters += GetAllDiameters;
            SharingEvents.SharingEvents.Canseled += Canseled;
            SharingEvents.SharingEvents.ExportToExcel += ExportToExcel;
            SharingEvents.SharingEvents.GetAddressConnection += GetAddressConnection;
            SharingEvents.SharingEvents.ChangeAddressConnection += ChangeAddressConnection;
            SharingEvents.SharingEvents.TestConnection += TestConnection;
            SharingEvents.SharingEvents.DirectoryUpdate += DirectoryUpdate;
            SharingEvents.SharingEvents.DirectoryCarrierDataUpdate += DirectoryCarrierDataUpdate;
            SharingEvents.SharingEvents.DirectoryCarriersUpdate += DirectoryCarriersUpdate;
            SharingEvents.SharingEvents.GetInfo += GetInfo;
            SharingEvents.SharingEvents.ExportToJeson += ExportToJeson;
            SharingEvents.SharingEvents.FormLoad += FormLoad;
        }
    }
}
