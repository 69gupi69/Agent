using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Diascan.Agent.FuzzySearch;
using Diascan.Agent.Types;
using Diascan.Utils.IO;

namespace Diascan.Agent.SharingEvents
{
    public static class SharingEvents
    {
        public delegate DataModel[] GetAllTableHandler();
        public static event GetAllTableHandler GetAllDataModel;

        public delegate float[] GetAllDiameterHandler();
        public static event GetAllDiameterHandler GetAllDiameters;

        //  Данные с формы паспорта
        public delegate void SentPdiHandler(string omniFileName, ReferenceInputData referenceInputData);
        public static event SentPdiHandler SentPdi;

        //  Получить данные для формы
        public delegate HeaderCalculation[] GetCalculationsHandler();
        public static event GetCalculationsHandler GetCalculations;

        //  Отмена расчета
        public delegate void CanseledHandler(Guid globalId);
        public static event CanseledHandler Canseled;

        //  Экспорт результата в Excel
        public delegate void ExportToExcelHandler(int id, string path);
        public static event ExportToExcelHandler ExportToExcel;

        //  Получить адрес подключения к серверу
        public delegate string GetAddressConnectionHandler();
        public static event GetAddressConnectionHandler GetAddressConnection;

        //  изменить адрес подключения
        public delegate void ChangeAddressConnectionHandler(string newAddress);
        public static event ChangeAddressConnectionHandler ChangeAddressConnection;

        //  протестировать соединение
        public delegate bool TestConnectionHandler(string newAddress);
        public static event TestConnectionHandler TestConnection;

        //  сообщения из расчетов об предупреждение
        public delegate void WarnMessageHandler(string message);
        public static event WarnMessageHandler WarnMessage;

        //  сообщения из расчетов об ошибке
        public delegate void ErrorMessageHandler(string message);
        public static event ErrorMessageHandler ErrorMessage;

        //  удалить строку на форме из расчетов
        public delegate void DeleteRowHandler(string message, Guid globalId);
        public static event DeleteRowHandler DeleteRow;

        //  обновить справочник
        public delegate void DirectoryUpdateHandler();
        public static event DirectoryUpdateHandler DirectoryUpdate;

        //  обновить справочник CarrierData
        public delegate void DirectoryCarrierDataUpdateHandler();
        public static event DirectoryCarrierDataUpdateHandler DirectoryCarrierDataUpdate;

        // получение информации по программе и справочнику
        public delegate FileInfo[] GetInfoHandler();
        public static event GetInfoHandler GetInfo;

        //  экспорт результата в Json
        public delegate void ExportToJesonHandler(int id, string path);
        public static event ExportToJesonHandler ExportToJeson;

        //  загрузка формы
        public delegate Task<BindingList<HeaderCalculation>> FormLoadHandler();
        public static event FormLoadHandler FormLoad;

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void OnSentPdi(string omnifilename, ReferenceInputData referenceInputData)
        {
            SentPdi?.Invoke(omnifilename, referenceInputData);
        }

        public static HeaderCalculation[] OnGetCalculation()
        {
            return GetCalculations?.Invoke();
        }

        public static void OnCanseled(Guid globalId)
        {
            Canseled?.Invoke(globalId);
        }

        public static void OnExportToExcel(int id, string path)
        {
            ExportToExcel?.Invoke(id, path);
        }

        public static string OnGetAddressConnection()
        {
            return GetAddressConnection?.Invoke();
        }

        public static void OnChangeAddressConnection(string newaddress)
        {
            ChangeAddressConnection?.Invoke(newaddress);
        }

        public static bool? OnTestConnection(string newaddress)
        {
            return TestConnection?.Invoke(newaddress);
        }

        public static void OnWarnMessage(string message)
        {
            WarnMessage?.Invoke(message);
        }

        public static void OnErrorMessage(string message)
        {
            ErrorMessage?.Invoke(message);
        }

        public static void OnDeleteRow(string message, Guid globalId)
        {
            DeleteRow?.Invoke(message, globalId);
        }

        public static void OnDirectoryUpdate()
        {
            DirectoryUpdate?.Invoke();
        }

        public static void OnDirectoryCarrierDataUpdate()
        {
            DirectoryCarrierDataUpdate?.Invoke();
        }
        public static FileInfo[] OnGetInfo()
        {
            return GetInfo?.Invoke();
        }

        public static void OnExportToJeson(int id, string path)
        {
            ExportToJeson?.Invoke(id, path);
        }

        public static DataModel[] OnGetAllDataModel()
        {
            return GetAllDataModel?.Invoke();
        }

        public static float[] OnGetAllDiameters()
        {
            return GetAllDiameters?.Invoke();
        }

        public static Task<BindingList<HeaderCalculation>> OnFormLoad()
        {
            return FormLoad?.Invoke();
        }
    }
}

