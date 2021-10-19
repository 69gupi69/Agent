using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using DiCore.Lib.NDT.Carrier;
using Diascan.Agent.AnalysisManager;
using DiCore.Lib.Web;
using DiCore.Wrappers.Notification;
using Npgsql;
using Newtonsoft.Json;
using NpgsqlTypes;
using Staff.Wrapper;
using Diascan.Agent.DataBaseAgentAccess.Properties;
using Diascan.Agent.Types;
using LiteDB;
using Newtonsoft.Json.Linq;

namespace Diascan.Agent.DataBaseAgentAccess
{
    public class DataBaseAgentAccessManager
    {
        public string ConnectionString         { get; protected set; }
        public string ApiUrlStaffWrapper { get; protected set; }
        public string NotificationApiUrl       { get; protected set; }
        public string ApplicationCode { get; protected set; }
        public string SecurityKey              { get; protected set; }
        public string WebApiAgentUrl           { get; protected set; }
        public string Path                     { get; protected set; }
        public JsonSerializerSettings Settings { get; protected set; }
        internal const string DateFormat    = "yy-MM-dd";
        internal const string T             = "T";
        internal const string Colon         = ":";
        internal const string Hyphen        = "-";
        internal const string Title         = @"Рассылка Excel отчета ПО ""ПДИ"" ";
        internal const string Message       = "Данное сообщение создано автоматически. Пожалуйста, не отвечайте на это письмо.\n";
        internal const int    LimitKilobyte = 4197376;
        internal const string DirictoriName = "Files";

        public DataBaseAgentAccessManager( string connectionString, string filePath, string apiUrlStaffWrapper, string notificationApiUrl, string applicationCode, string securityKey, string webApiAgentUrl )
        {
            
            Path = AppDomain.CurrentDomain.BaseDirectory + DirictoriName;
            ConnectionString = connectionString;
            Logger.Logger.InitLogger( filePath );
            ApiUrlStaffWrapper = apiUrlStaffWrapper; // value="http://vds01-demsiis29:8555/api/v2/"
            NotificationApiUrl = notificationApiUrl; // value="http://vds01-demsiis29:8558/api/v1/"
            ApplicationCode    = applicationCode;    // value="Agent"
            SecurityKey        = securityKey;        // key="SmptNotificationSecurityKey" value="4e6f74696669636174696f6e"
            WebApiAgentUrl     = webApiAgentUrl;     // value="http://vds01-tetemp-13/api/Agent/"
            Settings           = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead };
        }

        /// <summary>
        /// Получить массив byte Excel файла
        /// </summary>
        /// <param name="json">Расчет ПДИ</param>
        /// <param name="fileName">Имя Excel файла</param>
        /// <param name="buff">byte массив Excel файла</param>
        /// <returns>true если Excel сформерован, иначе Excel НЕ сформирован</returns>
        private bool GetByteExcel( string json, out string fileName, out byte[] buff )
        {
            var flag = false;
            if ( !string.IsNullOrEmpty(json) )
            {
                var session = JsonConvert.DeserializeObject<Session>(json, Settings);
                //  расчет анализа для формирования экселя
                var analysisManger = new AnalysisManager.AnalysisManager(Logger.Logger.Info); // <-
                var analysisCalculation = analysisManger.DoAnalysis(session);
                foreach (var calculation in session.Calculations)
                    Logger.Logger.Info($"{calculation.DataOutput.WorkItemName}: Проверка перезапуска завершена");
                
                var shortDateFormat = DateTime.Now.ToString( DateFormat );
                var shortTimeFormat = DateTime.Now.ToString( T ).Replace( Colon, Hyphen );
                    fileName = $"{ session.Calculations[0].DataOutput.WorkItemName }_{ shortDateFormat }_{ shortTimeFormat }.xls";
                    buff = new ExportToExcel(Logger.Logger.Info).CreateExcelFile(analysisCalculation, session.Calculations, out flag );
            }
            else
            {
                fileName = string.Empty;
                buff = null;
            }

            return flag;
        }

        private List<FileParameter> СreatureListByteExcel(List<KeyValuePair<Guid, string>> jsons, out int countElementsAdded, out int kilobyteExcel)
        {
            var files = new List<FileParameter>();

            countElementsAdded = 0; //количество добавленных элементов
            kilobyteExcel = 0;

            foreach (var json in jsons)
            {
                if (GetByteExcel(json.Value, out var fileName, out var buff))
                {
                    kilobyteExcel = kilobyteExcel + buff.Length;
                    if (kilobyteExcel >= LimitKilobyte) break;

                    files.Add(new FileParameter(buff, fileName));
                    Logger.Logger.Info($" Файл {fileName} Excel сформирован ");
                }
                else
                {
                    Logger.Logger.Info($" Файл {fileName} Excel НЕ сформирован !!! ");
                }
                countElementsAdded++;
            }
            return files;
        }

        private void SendMessage(int kilobyteExcel,
                                 int countElementsAdded,
                                 string[] emailUser,
                                 FileParameter[] files,
                                 NotificationWrapper notificationWrapper,
                                 List<KeyValuePair<Guid, string>> jsons)

        {
            var html = false;
            if (kilobyteExcel >= LimitKilobyte)
            {
                var mes = Message + "Размер вложения превысел допустимый сервером предел. Перейдите по ссылке(ам) для скачивания:\n";
                var s = 0;
                foreach (var json in jsons)
                {
                    if (s >= countElementsAdded)
                        mes = mes + WebApiAgentUrl + $"ExcelFromDataBase/{json.Key}\n";
                    s++;
                }
                notificationWrapper.SendMessage(emailUser, new string[] { }, Title, mes, null, files);
            }
            else
            {
                notificationWrapper.SendMessage(emailUser, new string[] { }, Title, Message, null, files);
            }
        }


        /// <summary>
        /// Формеруем Excel'и и рассылаем всем пользователям,
        /// которые имеют рольи User в Staff
        /// </summary>
        /// <param name="jsons">Список Calculation</param>
        public bool MailDistribution( List<KeyValuePair<Guid, string>> jsons, string profile)
        {
            try
            {
                var files = СreatureListByteExcel(jsons, out var countElementsAdded, out var kilobyteExcel);

                Logger.Logger.Info($"Формирование запрос к ApiUrlStaffWrapper {ApiUrlStaffWrapper}, ApplicationCode {ApplicationCode}");
                var staffWrapper = new StaffWrapper(new StaffConnectionParameters(ApiUrlStaffWrapper, ApplicationCode));
                Logger.Logger.Info($"Забираем из ApiUrlStaffWrapper {ApiUrlStaffWrapper}, почтовые адреса с profile {profile}");
                var users = staffWrapper.GetUsersForPermission(profile);
                Logger.Logger.Info($"Количество полученых почтовых адресов пользователей {users.Length}");
                var emailUsers = new string[users.Length];

                Parallel.For(0, users.Length, j => { emailUsers[j] = users[j].Email; });

                Logger.Logger.Info($"Формирование запрос к NotificationApiUrl  {NotificationApiUrl}, ApplicationCode {ApplicationCode}, SecurityKey {SecurityKey}");
                var notificationWrapper = new NotificationWrapper( new NotificationWrapperParameters(NotificationApiUrl, ApplicationCode, SecurityKey));

                Logger.Logger.Info($"Количество файлов: {files.Count}");
                var checkEmailUsers = new List<string>();
                foreach (var emailUser in emailUsers)
                {
                    Logger.Logger.Info($"Почтовых адрес: {emailUser}");
                    if (emailUser == String.Empty || emailUser == "" || emailUser == null)
                        continue;
                    
                    checkEmailUsers.Add(emailUser);
                }

                SendMessage(kilobyteExcel, countElementsAdded, checkEmailUsers.ToArray(), files.ToArray(), notificationWrapper, jsons);

                Logger.Logger.Info("Excel'и файлы отправлены");
                return true;
            }
            catch (Exception e)
            {
                Logger.Logger.Info($@"Excel'и файлы не отправлены n\ ERROR:n\{e.Message}");
                return false;
            }
        }


        /// <summary>
        /// Формеруем Excel'и и рассылаем выбранным пользователям,
        /// которые имеют рольи User в Staff
        /// </summary>
        /// <param name="jsons">Список расчетов</param>
        /// <param name="idUsersEmail">Список Id пользователей</param>
        /// <returns></returns>
        public bool Mailing( List<KeyValuePair<Guid, string>> jsons, List<Guid> idUsersEmail )
        {
            try
            {
                var files = СreatureListByteExcel(jsons, out var countElementsAdded, out var kilobyteExcel);

                var staffWrapper = new StaffWrapper( new StaffConnectionParameters(ApiUrlStaffWrapper, ApplicationCode));
                var users        = staffWrapper.GetUsersForPermission(Resource.ProfileUser);
                var emailUser    = new string[idUsersEmail.Count];

                Parallel.For(0, users.Length, j =>
                {
                    foreach (var idUserEmail in idUsersEmail)
                        if (users[j].Id == idUserEmail)
                            emailUser[j] = users[j].Email;
                });

                var notificationWrapper = new NotificationWrapper( new NotificationWrapperParameters( NotificationApiUrl, ApplicationCode, SecurityKey ) );

                SendMessage(kilobyteExcel, countElementsAdded, emailUser, files.ToArray(), notificationWrapper, jsons);

                Logger.Logger.Info( "Excel'и файлы отправлены" );
                return true;
            }
            catch (Exception e)
            {
                Logger.Logger.Info($@"Excel'и файлы не отправлены n\ ERROR:n\{e.Message}");
                return false;
            }
        }


        private void InsertJson( List<KeyValuePair<Guid, string>> jsons )
        {
            try
            {
                using ( var connection = new NpgsqlConnection( ConnectionString ) )
                {
                    connection.Open();
                    Logger.Logger.Info( $"Соединение с БД: ОТКРЫТО" );

                    using ( var writer = connection.BeginBinaryImport("COPY data.\"Sessions\" (\"Id\", \"Item\",\"ContractorId\", \"PipeLineId\",\"RouteId\", \"DateWorkItem\", \"StartDateDataCalculation\") FROM STDIN (FORMAT BINARY)") )
                    {
                        var workItemName = String.Empty;
                        foreach (var item in jsons)
                        {
                            var Session = JsonConvert.DeserializeObject<Session>( item.Value, Settings );
                            writer.StartRow();
                            writer.Write(Session.GlobalID                                 , NpgsqlDbType.Uuid);
                            writer.Write(item.Value                                       , NpgsqlDbType.Jsonb);
                            writer.Write(Session.Calculations[0].DataOutput.Contractor.Key, NpgsqlDbType.Uuid); // Контракторы
                            writer.Write(Session.Calculations[0].DataOutput.PipeLine.Key  , NpgsqlDbType.Uuid); // Трубопроводы
                            writer.Write(Session.Calculations[0].DataOutput.Route.Key     , NpgsqlDbType.Uuid); // Участок
                            writer.Write(Session.Calculations[0].DataOutput.DateWorkItem  , NpgsqlDbType.Date); // Дата пропуска
                            writer.Write(Session.StartDateDataCalculation                 , NpgsqlDbType.Date); // Дата старта расчета данных
                            workItemName = Session.Calculations[0].DataOutput.WorkItemName;
                        }
                        writer.Complete();
                        Logger.Logger.Info($"Json:{workItemName}, добавлен в базу");
                    }

                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                        Logger.Logger.Info( $"Соединение с БД: ЗАКРЫТО" );
                    }
                }
            }
            catch( Exception e )
            {
                Logger.Logger.Info( e.Message );
            }
        }


        /// <summary>
        /// Получить количество записей Json'ов из БД
        /// </summary>
        /// <returns>Количество записей</returns>
        public int GetCountJsonFromDataBase()
        {
            try
            {
                var dataResult = Int32.MinValue;
                using( var connection = new NpgsqlConnection( ConnectionString ) )
                {
                    connection.Open();
                    using( var reader = new NpgsqlCommand( $"SELECT count(*) FROM data.\"Sessions\"", connection ).ExecuteReader() )
                        while( reader.Read() )
                            Int32.TryParse( reader[0].ToString(), out dataResult );
                    connection.Close();
                }
                return dataResult;
            }
            catch( Exception e )
            {
                Logger.Logger.Info( e.Message );
                return Int32.MinValue;
            }
        }

        /// <summary>
        /// Получить количество записей Json'ов из БД
        /// </summary>
        /// <returns>Количество записей</returns>
        public int GetCountJsonFromDataBase(Guid? contractorId, Guid? pipelineId, Guid? routeId)
        {
            var w = new List<string>(3);

            if (contractorId.HasValue) w.Add($"\"ContractorId\" = '{contractorId}'");
            if (pipelineId.HasValue) w.Add($"\"PipeLineId\" = '{pipelineId}'");
            if (routeId.HasValue) w.Add($"\"RouteId\" = '{routeId}'");

            var whereStr = w.Any() ? $" WHERE {string.Join(" AND ", w)}" : "";

            try
            {
                var dataResult = Int32.MinValue;
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (var reader = new NpgsqlCommand($"SELECT count(*) FROM data.\"Sessions\" {whereStr}", connection).ExecuteReader())
                        while (reader.Read())
                            Int32.TryParse(reader[0].ToString(), out dataResult);
                    connection.Close();
                }
                return dataResult;
            }
            catch (Exception e)
            {
                Logger.Logger.Info(e.Message);
                return Int32.MinValue;
            }
        }

        /// <summary>
        /// Получить заголовки Json'ов
        /// </summary>
        /// <param name="pagenumber">Номер страницы</param>
        /// <param name="pageSize">Размер страницы</param>
        /// <returns></returns>
        public List<SessionUniHead> GetJsonsHeads(int pagenumber, int pageSize)
        {
            try
            {
                var dataResult = new List<SessionUniHead>();
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var i = pageSize * (pagenumber - 1);
                    i = i <= 0 ? 0 : i;
                    var сommand = new NpgsqlCommand($"SELECT \"Id\", (jsonb_array_elements((\"Item\"->'Calculations')::jsonb)->'DataOutput')->'WorkItemName', (jsonb_array_elements((\"Item\"->'Calculations')::jsonb)->'DataOutput')->'AccountUserName', (jsonb_array_elements((\"Item\"->'Calculations')::jsonb)->'DataOutput')->'ComputerName', \"ContractorId\", \"PipeLineId\", \"RouteId\", \"DateWorkItem\", \"StartDateDataCalculation\" FROM data.\"Sessions\" ORDER BY \"Id\" ASC OFFSET {i} LIMIT {pageSize}", connection);
                    connection.Open();
                    using (var reader = сommand.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            dataResult.Add(new SessionUniHead()
                            {
                                Id              = ( Guid )reader[0],
                                Name    = reader[1].ToString(),
                                AccountUserName = reader[2].ToString(),
                                ComputerName    = reader[3].ToString(),
                                ContractorId    = ( Guid )reader[4],
                                PipeLineId      = ( Guid )reader[5],
                                RouteId         = ( Guid )reader[6],
                                DateWorkItem    = ( DateTime )reader[7],
                                StartDateDataCalculation = (DateTime)reader[8]
                            });
                        }
                    }
                    connection.Close();
                }
                return dataResult;
            }
            catch (Exception e)
            {
                Logger.Logger.Info(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Получить заголовки Json'ов по имени расчета 
        /// </summary>
        /// <param name="WorkItemName">Имя расчета</param>
        /// <returns></returns>
        public List<SessionUniHead> GetJsonsHeads(string workItemName)
        {
            try
            {
                var dataResult = new List<SessionUniHead>();
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var сommand = new NpgsqlCommand($"SELECT x.* FROM ( SELECT \"Id\", (jsonb_array_elements((\"Item\"->'Calculations')::jsonb)->'DataOutput')->'WorkItemName' AS WorkItemName, (jsonb_array_elements((\"Item\"->'Calculations')::jsonb)->'DataOutput')->'AccountUserName', (jsonb_array_elements((\"Item\"->'Calculations')::jsonb)->'DataOutput')->'ComputerName', \"ContractorId\", \"PipeLineId\", \"RouteId\", \"DateWorkItem\", \"StartDateDataCalculation\" FROM data.\"Sessions\") x WHERE x.WorkItemName = '\"{workItemName}\"'", connection);
                    connection.Open();
                    using (var reader = сommand.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            dataResult.Add(new SessionUniHead()
                            {
                                Id = (Guid)reader[0],
                                Name = reader[1].ToString(),
                                AccountUserName = reader[2].ToString(),
                                ComputerName = reader[3].ToString(),
                                ContractorId = (Guid)reader[4],
                                PipeLineId = (Guid)reader[5],
                                RouteId = (Guid)reader[6],
                                DateWorkItem = (DateTime)reader[7],
                                StartDateDataCalculation = (DateTime)reader[8]
                            });
                        }
                    }
                    connection.Close();
                }
                return dataResult;
            }
            catch (Exception e)
            {
                Logger.Logger.Info(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Получить заголовок Json по Guid Id
        /// </summary>
        /// <param name="Id">Guid</param>
        /// <returns></returns>
        public List<SessionUniHead> GetJsonsHead(Guid Id)
        {
            try
            {
                var dataResult = new List<SessionUniHead>();
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var сommand = new NpgsqlCommand($"SELECT \"Id\", (jsonb_array_elements((\"Item\"->'Calculations')::jsonb)->'DataOutput')->'WorkItemName',  (jsonb_array_elements((\"Item\"->'Calculations')::jsonb)->'DataOutput')->'AccountUserName', (jsonb_array_elements((\"Item\"->'Calculations')::jsonb)->'DataOutput')->'ComputerName', \"ContractorId\", \"PipeLineId\", \"RouteId\", \"DateWorkItem\", \"StartDateDataCalculation\" FROM data.\"Sessions\" WHERE \"Id\" = '{Id}'", connection);
                    connection.Open();
                    using (var reader = сommand.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            dataResult.Add(new SessionUniHead()
                            {
                                Id = (Guid)reader[0],
                                Name = reader[1].ToString(),
                                AccountUserName = reader[2].ToString(),
                                ComputerName = reader[3].ToString(),
                                ContractorId = (Guid)reader[4],
                                PipeLineId = (Guid)reader[5],
                                RouteId = (Guid)reader[6],
                                DateWorkItem = (DateTime)reader[7],
                                StartDateDataCalculation = (DateTime)reader[8],
                            });
                        }
                    }
                    connection.Close();
                }
                return dataResult;
            }
            catch (Exception e)
            {
                Logger.Logger.Info(e.Message);
                return null;
            }
        }

        #region UpdateCarrierData
        /// <summary>
        /// Поиск тип класса
        /// </summary>
        /// <param name="property">Свойство класса</param>
        /// <returns></returns>
        private NpgsqlDbType SearchType(PropertyInfo property)
        {
            switch (property.PropertyType.Name)
            {
                case "DataTypesExt":
                case "Int":
                case "Int32":
                case "Int64":
                    return NpgsqlDbType.Integer;
                    break;
                case "Double":
                    return NpgsqlDbType.Double;
                    break;
                case "String":
                    return NpgsqlDbType.Text;
                    break;
                case "Boolean":
                    return NpgsqlDbType.Boolean;
                    break;
                default:
                    throw new Exception("Определить тип данных не удалось");
            }
        }

        /// <summary>
        /// Обновить запись в таблице carrierdata
        /// </summary>
        /// <param name="carriersData"> CarrierData </param>
        /// <returns></returns>
        public bool UpdateCarrierData(CarrierData carriersData)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    Logger.Logger.Info($"Соединение с БД: [{ConnectionString}] открыто");
                    var sql = $"UPDATE data.\"carrierdata\"" +
                              $" SET \"diagtype\" = :Type," +
                              $" \"defectoscope\" = :Defectoscope," +
                              $" \"carrierdiameter\" = :CarrierDiameter," +
                              $" \"sensorcount\" = :Sensorcount," +
                              $" \"speedmin\" = :SpeedMin," +
                              $" \"speedmax\" = :SpeedMax," +
                              $" \"cdchange\" = :Change," +
                              $" \"numbersensorsblock\" = :NumberSensorsBlock" +
                              $"  WHERE \"id\" = :id";
                    using (var npgSqlCommand = new NpgsqlCommand(sql, connection))
                    {
                        var carriersDataType       = carriersData.GetType();
                        var carriersDataProperties = carriersDataType.GetProperties();
                        foreach (var Propertie in carriersDataProperties)
                            npgSqlCommand.Parameters.Add(new NpgsqlParameter()
                            {
                                ParameterName = $"@{Propertie.Name}",
                                Value         = Propertie.Name == "Type"? (int)(DataTypesExt)Propertie.GetValue(carriersData) : Propertie.GetValue(carriersData),
                                NpgsqlDbType  = SearchType(Propertie)
                            });

                        npgSqlCommand.ExecuteNonQuery();
                        npgSqlCommand.Cancel();
                        npgSqlCommand.Dispose();
                    }
                    Logger.Logger.Info($"CarrierData ID: {carriersData.Id}; Name:{carriersData.Defectoscope}, добавлен в базу");

                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                        Logger.Logger.Info($"Соединение с БД: [{ConnectionString}] закрыто");
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Logger.Info(e.Message);
                return false;
            }
        }
        #endregion

        #region InsertCarrierData
        /// <summary>
        /// Добавить список CarrierData в БД
        /// </summary>
        /// <param name="carriersData">список CarrierData</param>
        /// <returns></returns>
        public bool InsertCarrierData(IEnumerable<CarrierData> carriersData)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    Logger.Logger.Info($"Соединение с БД: [{ConnectionString}] открыто");

                    using (var writer = connection.BeginBinaryImport("COPY data.\"carrierdata\" (\"diagtype\", \"defectoscope\", \"carrierdiameter\", \"sensorcount\", \"speedmin\", \"speedmax\", \"cdchange\", \"id\", \"numbersensorsblock\") FROM STDIN (FORMAT BINARY)"))
                    {
                        foreach (var carrierData in carriersData)
                        {
                            writer. StartRow();
                            writer.Write((int)carrierData.Type,          NpgsqlDbType.Integer);
                            writer.Write(carrierData.Defectoscope,       NpgsqlDbType.Varchar);
                            writer.Write(carrierData.CarrierDiameter,    NpgsqlDbType.Integer);
                            writer.Write(carrierData.Sensorcount,        NpgsqlDbType.Integer);
                            writer.Write(carrierData.SpeedMin,           NpgsqlDbType.Double);
                            writer.Write(carrierData.SpeedMax,           NpgsqlDbType.Double);
                            writer.Write(carrierData.Change,             NpgsqlDbType.Boolean);
                            writer.Write(carrierData.Id,                 NpgsqlDbType.Integer);
                            writer.Write(carrierData.NumberSensorsBlock, NpgsqlDbType.Integer);
                            Logger.Logger.Info($"CarrierData ID: {carrierData.Id}; Name:{carrierData.Defectoscope}, добавлен в базу");
                        }
                        writer.Complete();
                    }

                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                        Logger.Logger.Info($"Соединение с БД: [{ConnectionString}] закрыто");
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Logger.Info(e.Message);
                return false;
            }
        }
        #endregion

        #region GetCarrierData
        /// <summary>
        /// Получить все CarrierData из БД
        /// </summary>
        /// <returns></returns>
        public List<CarrierData> GetAllCarrierData()
        {
            try
            {
                var dataResult = new List<CarrierData>();
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var сommand = new NpgsqlCommand($"SELECT diagtype, defectoscope, carrierdiameter, sensorcount, speedmin, speedmax, cdchange, id, numbersensorsblock FROM data.\"carrierdata\"; ", connection);
                    connection.Open();
                    using (var reader = сommand.ExecuteReader())
                        while (reader.Read())
                            dataResult.Add(new CarrierData()
                            {
                                Type = (DataTypesExt)(int)reader[0],
                                Defectoscope = reader[1].ToString(),
                                CarrierDiameter = (int)reader[2],
                                Sensorcount = (int)reader[3],
                                SpeedMin = (double)reader[4],
                                SpeedMax = (double)reader[5],
                                Change = (bool)reader[6],
                                Id = (int)reader[7],
                                NumberSensorsBlock = (int)reader[8],
                            });
                    connection.Close();
                }
                return dataResult;
            }
            catch (Exception e)
            {
                Logger.Logger.Info(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Получить страницу CarrierData
        /// </summary>
        /// <param name="pagenumber">Номер страницы</param>
        /// <param name="pageSize">Размер страницы</param>
        /// <returns></returns>
        public List<CarrierData> GetCarrierData(int pagenumber, int pageSize)
        {
            try
            {
                var dataResult = new List<CarrierData>(pageSize);
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var i = pageSize * (pagenumber - 1);
                    i = i <= 0 ? 0 : i;
                    var сommand = new NpgsqlCommand($"SELECT diagtype, defectoscope, carrierdiameter, sensorcount, speedmin, speedmax, cdchange, id, numbersensorsblock  FROM data.\"carrierdata\" ORDER BY \"id\" ASC OFFSET {i} LIMIT {pageSize}", connection);
                    connection.Open();
                    using (var reader = сommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dataResult.Add(new CarrierData()
                            {
                                Type = (DataTypesExt)(int)reader[0],
                                Defectoscope = reader[1].ToString(),
                                CarrierDiameter = (int)reader[2],
                                Sensorcount = (int)reader[3],
                                SpeedMin = (double)reader[4],
                                SpeedMax = (double)reader[5],
                                Change = (bool)reader[6],
                                Id = (int)reader[7],
                                NumberSensorsBlock = (int)reader[8],
                            });
                        }
                    }
                    connection.Close();
                }
                return dataResult;
            }
            catch (Exception e)
            {
                Logger.Logger.Info(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Получить CarrierData по Id
        /// </summary>
        /// <param name="Id">Guid</param>
        /// <returns></returns>
        public CarrierData GetCarrierDataId(int Id)
        {
            try
            {
                var dataResult = new CarrierData();
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var сommand = new NpgsqlCommand($"SELECT diagtype, defectoscope, carrierdiameter, sensorcount, speedmin, speedmax, cdchange, id, numbersensorsblock FROM data.\"carrierdata\" WHERE \"id\" = '{Id}'", connection);
                    connection.Open();
                    using (var reader = сommand.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            dataResult = new CarrierData()
                            {
                                Type = (DataTypesExt)(int)reader[0],
                                Defectoscope = reader[1].ToString(),
                                CarrierDiameter = (int)reader[2],
                                Sensorcount = (int)reader[3],
                                SpeedMin = (double)reader[4],
                                SpeedMax = (double)reader[5],
                                Change = (bool)reader[6],
                                Id = (int)reader[7],
                                NumberSensorsBlock = (int)reader[8],
                            };
                        }
                    }
                    connection.Close();
                }
                return dataResult;
            }
            catch (Exception e)
            {
                Logger.Logger.Info(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Получить количество записей CarrierData из БД
        /// </summary>
        /// <returns>Количество записей</returns>
        public int GetCountCarrierDataFromDataBase()
        {
            try
            {
                var dataResult = Int32.MinValue;
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (var reader = new NpgsqlCommand($"SELECT count(*) FROM data.\"carrierdata\"", connection).ExecuteReader())
                        while (reader.Read())
                            Int32.TryParse(reader[0].ToString(), out dataResult);
                    connection.Close();
                }
                return dataResult;
            }
            catch (Exception e)
            {
                Logger.Logger.Info(e.Message);
                return Int32.MinValue;
            }
        }
        #endregion

        #region DeleteCarrierData
        /// <summary>
        /// Удалить CarrierData по id из БД
        /// </summary>
        /// <param name="id">Уникальный id CarrierData</param>
        /// <returns></returns>
        public bool GetDeleteCarrierData(int id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var сommand = new NpgsqlCommand($"DELETE FROM data.\"carrierdata\" WHERE \"id\" = '{id}'", connection);
                    connection.Open();
                    var result = сommand.ExecuteNonQuery();
                    if (Convert.ToBoolean(result))
                        Logger.Logger.Info($"Данные успешно удалены!{id}");
                    else
                        Logger.Logger.Info($"Данные не удалены!{id}");
                    connection.Close();
                }
                return true;
            }
            catch (NpgsqlException e)
            {
                Logger.Logger.Info(e.Message);
                return false;
            }
        }

        public bool DeleteCarrierData(NpgsqlConnection connection)
        {
            Logger.Logger.Info($"Очистка таблицы");
            var sqlDELETE = "DELETE FROM data.carrierdata;";
            var command = new NpgsqlCommand(sqlDELETE, connection);
            command.ExecuteNonQuery();
            return true;
        }

        /// <summary>
        /// Удалить все CarrierData из БД
        /// </summary>
        /// <returns>Результат удаления true / false</returns>
        public bool GetDeleteAllCarrierData()
        {
            try
            {
                var res = false;
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    res = DeleteCarrierData(connection);
                    connection.Close();
                }
                return res;
            }
            catch (NpgsqlException e)
            {
                Logger.Logger.Info(e.Message);
                return false;
            }
        }
        #endregion

        private void InsertCarrierData(LiteCollection<CarrierData> contractorsTebel, List<CarrierData> сarrierData)
        {
            try
            {
                var agentContractors = new CarrierData[сarrierData.Count];
                var i = 0;
                foreach (var contractor in сarrierData)
                {
                    agentContractors[i] = contractor;
                    i++;
                }
                contractorsTebel.Insert(agentContractors);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Создание БД CarrierData
        /// </summary>
        /// <param name="UrlPath"></param>
        /// <returns></returns>
        public bool CreateCarrierData(string nameFileDB)
        {
            Task.Run(() => { Logger.Logger.Info(" Начало создания CarrierData "); });
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
            else if (File.Exists(nameFileDB))
            {
                File.Delete(nameFileDB);
            }

            var data = GetAllCarrierData();
            if (data != null || data.Count == 0)
            {
                Task.Run(() => { Logger.Logger.Info("Данные с DBAgent получены."); });

                using (var db = new LiteDatabase(nameFileDB))
                {
                    var сarrierDataTebel = db.GetCollection<CarrierData>("CarrierData");

                    Task.WaitAll(Task.Run(() => { InsertCarrierData(сarrierDataTebel, data); }));
                }
            }
            else
            {
                Task.Run(() => { Logger.Logger.Info("Данные с DBAgent получить не удалось!"); });
                return false;
            }

            return true;
        }

        /// <summary>
        /// Создание БД Carriers
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool CreateCarriers(string path)
        {
            Logger.Logger.Info("Начало создания Carriers");

            if (File.Exists(path))
                File.Delete(path);

            var loader   = new Loader();
            var res      = false;
            var carriers = loader.GetAllCarriers();

            Logger.Logger.Info($@" carriers = {carriers.Count()}");

            if (carriers == null || carriers.Count() == 0)
            {
                Logger.Logger.Info("Данные с sds01-pepgsql03 получить не удалось!");
            }
            else 
            {
                using (var localDb = new LiteDatabase(path))
                {
                    var carrierCollection = localDb.GetCollection<CarrierDto>();
                    foreach (var carrier in carriers)
                        carrierCollection.Insert(carrier);
                    
                    Logger.Logger.Info($@"carrierCollection = {carrierCollection.Name} / {carrierCollection.Count()}");
                }
                res = true;
            }

            return res;
        }

        /// <summary>
        /// Получить Json файл по id из БД
        /// </summary>
        /// <param name="id">Уникальный номер Json'a</param>
        /// <returns></returns>
        public string GetJson( Guid id )
        {
            try
            {
                var dataResult = string.Empty;
                using( var connection = new NpgsqlConnection( ConnectionString ) )
                {
                    var сommand = new NpgsqlCommand( $"SELECT \"Item\" FROM data.\"Sessions\" WHERE \"Id\" = '{id}'", connection );
                    connection.Open();
                    using( var reader = сommand.ExecuteReader() )
                        while (reader.Read())
                        {
                            var json = reader[0].ToString();
                            json = json.Replace("ModelDB", "Types");
                            json = json.Replace("EndDist", "StopDist");
                            dataResult = json;
                        }

                    
                    connection.Close();
                }
                return dataResult;
            }
            catch( Exception e )
            {
                Logger.Logger.Info( e.Message );
                return null;
            }
        }


        /// <summary>
        /// Получить Json файл по id из БД
        /// </summary>
        /// <param name="listIGuids">Уникальный номер Расчетов</param>
        /// <returns></returns>
        public List<KeyValuePair<Guid, string>> GetJsons( Guid[] listIGuids )
        {
            try
            {
                var arr = new List<string>(listIGuids.Length);
                arr.AddRange(listIGuids.Select(t => $"'{t}'"));
                var dataResult = new List<KeyValuePair<Guid, string>>();
                using( var connection = new NpgsqlConnection( ConnectionString ) )
                {
                    var сommand = new NpgsqlCommand( $"SELECT \"Item\" FROM data.\"Sessions\" WHERE \"Id\"  IN ({string.Join(", ", arr)})", connection );
                    connection.Open();
                    using (var reader = сommand.ExecuteReader())
                    {
                        var i = 0;
                        while (reader.Read())
                        {
                            var json = reader[0].ToString();
                            json = json.Replace("ModelDB", "Types");
                            json = json.Replace("EndDist", "StopDist");

                            dataResult.Add( new KeyValuePair<Guid, string>( listIGuids[i], json) ); 
                            i++;
                        }
                    }
                    connection.Close();
                }
                return dataResult;
            }
            catch( Exception e )
            {
                Logger.Logger.Info( e.Message );
                return null;
            }
        }


        /// <summary>
        /// Удалить Json файл по id из БД
        /// </summary>
        /// <param name="id">Уникальный номер Json'a</param>
        /// <returns></returns>
        public bool GetDeleteJson( Guid id )
        {
            try
            {
                using( var connection = new NpgsqlConnection( ConnectionString ) )
                {
                    var сommand = new NpgsqlCommand( $"DELETE FROM data.\"Sessions\" WHERE \"Id\" = '{id}'", connection );
                    connection.Open();
                    var result = сommand.ExecuteNonQuery();
                    if ( Convert.ToBoolean(result) )
                        Logger.Logger.Info( "Данные успешно удалены!" );
                    else
                        Logger.Logger.Info( "Данные не удалены!" );
                    connection.Close();
                }
                return true;
            }
            catch( NpgsqlException e )
            {
                Logger.Logger.Info( e.Message );
                return false;
            }
        }


        public bool GetExcel( Guid id, out byte[] buffer, out string nameFileDB )
        {
            var json = GetJson( id );
            if( string.IsNullOrEmpty( json ) || !GetByteExcel( json, out nameFileDB, out buffer ) )
            {
                nameFileDB = String.Empty;
                Logger.Logger.Info( $" Файл {nameFileDB} Excel НЕ сформирован !!! " );
                buffer = null;
                return false;
            }
            else
            {
                Logger.Logger.Info( $" Файл {nameFileDB} Excel сформирован " );
                return true;
            }
        }


        public bool CheckReportByGUID(List<KeyValuePair<Guid, string>> jsons)
        {
            var check = false;
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    foreach (var json in jsons)
                    {
                        var сommand = new NpgsqlCommand($"SELECT \"Item\" FROM data.\"Sessions\" WHERE \"Id\" = '{json.Key.ToString()}'", connection);
                        connection.Open();
                        using (var reader = сommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                check = true;
                            }
                            else
                            {
                                check = false;
                                connection.Close();
                                break;
                            }
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Logger.Info(e.Message);
                return false;
            }
            return check;
        }


        public void DataAgentInsertJsonLdiFiles(List<KeyValuePair<Guid, string>> jsons)
        {
            try
            {
                if (!CheckReportByGUID(jsons))
                {
                    MailDistribution(jsons, Resource.ProfileUser);
                    InsertJson(jsons);
                }
            }
            catch ( Exception e )
            {
                Logger.Logger.Info( e.Message );
            }
        }


        public void DataAgentInsertLogFiles( List<KeyValuePair<Guid, string>> logs )
        {
            try
            {
                using( var connection = new NpgsqlConnection( ConnectionString ) )
                {
                    connection.Open();
                    using( var writer = connection.BeginBinaryImport( "COPY data.\"LogFiles\" (\"Id\", \"Item\", \"DateWorkItem\") FROM STDIN (FORMAT BINARY)" ) )
                    {
                        foreach( var log in logs )
                        {
                            writer.StartRow();
                            writer.Write( log.Key, NpgsqlDbType.Uuid );
                            writer.Write( log.Value, NpgsqlDbType.Text );
                            writer.Write( DateTime.Now, NpgsqlDbType.Date ); // Дата пропуска
                        }
                        writer.Complete();
                        writer.Close();
                    }

                    if( connection.State == ConnectionState.Open )
                        connection.Close();
                }
            }
            catch ( Exception e )
            {
                Logger.Logger.Info( e.Message );
            }
        }

        public List<JObject> GetUsers()
        {
            var StaffWebClient = new SimpleWebClient(ApiUrlStaffWrapper.Trim());
            var usersListByPermission = StaffWebClient.Get<List<JObject>>($"auth/applications/{ApplicationCode}/permissions/{Resource.ProfileUser}/users");//Full");
            return usersListByPermission;
        }

        /// <summary>
        /// Получение ОСТа по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор ОСТа</param>
        /// <returns>ОСТ</returns>
        public JObject GetStaffContractor(Guid id)
        {
            var StaffWebClient = new SimpleWebClient(ApiUrlStaffWrapper.Trim());
            return JObject.FromObject(StaffWebClient.Get<object>($"affiliatedcompanies/?Id={id.ToString()}"));
        }
    }
}
