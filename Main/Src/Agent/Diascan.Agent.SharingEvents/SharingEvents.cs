using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diascan.Agent.ModelDB;

namespace Diascan.Agent.SharingEvents
{
    public static class SharingEvents
    {
        //  Данные с формы паспорта
        public delegate void SentPdiHandler(string omniFileName, ReferenceInputData referenceInputData);
        public static event SentPdiHandler SentPdi;

        //  Получить справочные данные Customers
        public delegate Task<Dictionary<Guid, string>> GetPdiCustomersHandler();
        public static event GetPdiCustomersHandler GetPdiCustomers;

        //  Получить справочные данные Pipelines
        public delegate Task<Dictionary<Guid, string>> GetPdiPipelinesHandler(Guid customerGuid);
        public static event GetPdiPipelinesHandler GetPdiPipelines;

        //  получить справочные данные Routes
        public delegate Task<Dictionary<Guid, string>> GetPdiRoutesHandler(Guid customerGuid, Guid pipelineGuid);
        public static event GetPdiRoutesHandler GetPdiRoutes;

        //  получить справочные данные Diameter
        public delegate Task<float> GetPdiDiameterHandler(Guid routeGuidGuid);
        public static event GetPdiDiameterHandler GetPdiDiameter;

        //  Получить данные для формы
        public delegate Task<TableRowData[]> GetCalculationsHandler();
        public static event GetCalculationsHandler GetCalculations;

        //  Отмена расчета
        public delegate Task CanseledHandler(int id);
        public static event CanseledHandler Canseled;

        //  Экспорт результата в Excel
        public delegate Task ExportToExcelHandler(int id, string path);
        public static event ExportToExcelHandler ExportToExcel;

        //  Получить адрес подключения к серверу
        public delegate Task<string> GetAddressConnectionHandler();
        public static event GetAddressConnectionHandler GetAddressConnection;

        //  изменить адрес подключения
        public delegate Task ChangeAddressConnectionHandler(string newAddress);
        public static event ChangeAddressConnectionHandler ChangeAddressConnection;

        //  протестировать соединение
        public delegate Task<bool> TestConnectionHandler(string newAddress);
        public static event TestConnectionHandler TestConnection;

        //  сообщения из расчетов
        public delegate Task WarnMessageHandler(string message);
        public static event WarnMessageHandler WarnMessage;

        //  удалить строку на форме из расчетов
        public delegate Task DeleteRowHandler(string message, int cancelElementId);
        public static event DeleteRowHandler DeleteRow;

        //  обновить справочник
        public delegate Task DirectoryUpdateHandler();
        public static event DirectoryUpdateHandler DirectoryUpdate;

        // получение информации по программе и справочнику
        public delegate Task<FileInfo[]> GetInfoHandler();
        public static event GetInfoHandler GetInfo;

        //  экспорт результата в Json
        public delegate Task ExportToJesonHandler(int id, string path);
        public static event ExportToJesonHandler ExportToJeson;


        public static void OnSentPdi(string omnifilename, ReferenceInputData referenceInputData)
        {
            SentPdi?.Invoke(omnifilename, referenceInputData);
        }

        public static Task<Dictionary<Guid, string>> OnGetPdiCustomers()
        {
            return GetPdiCustomers?.Invoke();
        }

        public static Task<Dictionary<Guid, string>> OnGetPdiPipelines(Guid customerguid)
        {
            return GetPdiPipelines?.Invoke(customerguid);
        }

        public static Task<Dictionary<Guid, string>> OnGetPdiRoutes(Guid customerguid, Guid pipelineguid)
        {
            return GetPdiRoutes?.Invoke(customerguid, pipelineguid);
        }

        public static Task<float> OnGetPdiDiameter(Guid routeguidguid)
        {
            return GetPdiDiameter?.Invoke(routeguidguid);
        }

        public static Task<TableRowData[]> OnGetCalculation()
        {
            return GetCalculations?.Invoke();
        }

        public static Task OnCanseled(int id)
        {
            return Canseled?.Invoke(id);
        }

        public static Task OnExportToExcel(int id, string path)
        {
            return ExportToExcel?.Invoke(id, path);
        }

        public static Task<string> OnGetAddressConnection()
        {
            return GetAddressConnection?.Invoke();
        }

        public static Task OnChangeAddressConnection(string newaddress)
        {
            return ChangeAddressConnection?.Invoke(newaddress);
        }

        public static Task<bool> OnTestConnection(string newaddress)
        {
            return TestConnection?.Invoke(newaddress);
        }

        public static Task OnWarnMessage(string message)
        {
            return WarnMessage?.Invoke(message);
        }

        public static Task OnDeleteRow(string message, int cancelelementid)
        {
            return DeleteRow?.Invoke(message, cancelelementid);
        }

        public static Task OnDirectoryUpdate()
        {
            return DirectoryUpdate?.Invoke();
        }

        public static Task<FileInfo[]> OnGetInfo()
        {
            return GetInfo?.Invoke();
        }

        public static Task OnExportToJeson(int id, string path)
        {
            return ExportToJeson?.Invoke(id, path);
        }
    }
}

