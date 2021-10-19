using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Globalization;
using System.IO;
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
    public class AgentDataJson
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

        public AgentDataJson( string connectionString, string filePath, string apiUrlStaffWrapper, string notificationApiUrl, string applicationCode, string securityKey, string webApiAgentUrl )
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
                var calculation = JsonConvert.DeserializeObject<Calculation>(json, Settings);
                //  расчет анализа для формирования экселя
                var analysisManger = new AnalysisManager.AnalysisManager(Logger.Logger.Info); // <-
                var analysisCalculation = analysisManger.DoAnalysis(calculation);
                Logger.Logger.Info($"{calculation.DataOutput.WorkItemName}: Проверка перезапуска завершена");
                
                var shortDateFormat = DateTime.Now.ToString( DateFormat );
                var shortTimeFormat = DateTime.Now.ToString( T ).Replace( Colon, Hyphen );
                    fileName = $"{ calculation.DataOutput.WorkItemName }_{ shortDateFormat }_{ shortTimeFormat }.xls";
                    buff = new ExportToExcel(Logger.Logger.Info).CreateExcelFile(analysisCalculation, calculation, out flag );
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

                    using ( var writer = connection.BeginBinaryImport( "COPY data.\"Calculations\" (\"Id\", \"Item\",\"ContractorId\", \"PipeLineId\",\"RouteId\", \"DateWorkItem\") FROM STDIN (FORMAT BINARY)" ) )
                    {
                        var workItemName = String.Empty;
                        foreach (var item in jsons)
                        {
                            var calculation = JsonConvert.DeserializeObject<Calculation>( item.Value, Settings );
                            var timeСheck = DateTime.TryParseExact( calculation.DataOutput.DateWorkItem, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date );
                            Logger.Logger.Info( $"Дата преоборазовалась? {timeСheck}" );
                            if ( timeСheck )
                            {

                                writer.StartRow();
                                writer.Write( calculation.GlobalId, NpgsqlDbType.Uuid  );
                                writer.Write( item.Value,           NpgsqlDbType.Jsonb );
                                writer.Write( calculation.DataOutput.Contractor.Key, NpgsqlDbType.Uuid ); // Контракторы
                                writer.Write( calculation.DataOutput.PipeLine.Key,   NpgsqlDbType.Uuid ); // Трубопроводы
                                writer.Write( calculation.DataOutput.Route.Key,      NpgsqlDbType.Uuid ); // Участок
                                writer.Write( date, NpgsqlDbType.Date ); // Дата пропуска
                            }
                            workItemName = calculation.DataOutput.WorkItemName;
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
                    using( var reader = new NpgsqlCommand( $"SELECT count(*) FROM data.\"Calculations\"", connection ).ExecuteReader() )
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
                    using (var reader = new NpgsqlCommand($"SELECT count(*) FROM data.\"Calculations\" {whereStr}", connection).ExecuteReader())
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
        public List<CalculationUniHead> GetJsonsHeads(int pagenumber, int pageSize)
        {
            try
            {
                var dataResult = new List<CalculationUniHead>();
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var i = pageSize * (pagenumber - 1);
                    i = i <= 0 ? 0 : i;
                    var сommand = new NpgsqlCommand($"SELECT \"Id\", \"Item\"->'DataOutput'->>'WorkItemName',  \"Item\"->'DataOutput'->>'AccountUserName', \"Item\"->'DataOutput'->>'ComputerName', \"ContractorId\", \"PipeLineId\", \"RouteId\", \"DateWorkItem\" FROM data.\"Calculations\" ORDER BY \"Id\" ASC OFFSET {i} LIMIT {pageSize}", connection);
                    connection.Open();
                    using (var reader = сommand.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            dataResult.Add(new CalculationUniHead()
                            {
                                Id              = ( Guid )reader[0],
                                Name    = reader[1].ToString(),
                                AccountUserName = reader[2].ToString(),
                                ComputerName    = reader[3].ToString(),
                                ContractorId    = ( Guid )reader[4],
                                PipeLineId      = ( Guid )reader[5],
                                RouteId         = ( Guid )reader[6],
                                DateWorkItem    = ( DateTime )reader[7]
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
        public List<CalculationUniHead> GetJsonsHeads(string workItemName)
        {
            try
            {
                var dataResult = new List<CalculationUniHead>();
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var сommand = new NpgsqlCommand($"SELECT \"Id\", \"Item\"->'DataOutput'->>'WorkItemName', \"Item\"->'DataOutput'->>'AccountUserName', \"Item\"->'DataOutput'->>'ComputerName', \"ContractorId\", \"PipeLineId\", \"RouteId\", \"DateWorkItem\" FROM data.\"Calculations\" WHERE \"Item\"->'DataOutput'->>'WorkItemName' = '{workItemName}'", connection);
                    connection.Open();
                    using (var reader = сommand.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            dataResult.Add(new CalculationUniHead()
                            {
                                Id = (Guid)reader[0],
                                Name = reader[1].ToString(),
                                AccountUserName = reader[2].ToString(),
                                ComputerName = reader[3].ToString(),
                                ContractorId = (Guid)reader[4],
                                PipeLineId = (Guid)reader[5],
                                RouteId = (Guid)reader[6],
                                DateWorkItem = (DateTime)reader[7]
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
        public List<CalculationUniHead> GetJsonsHead(Guid Id)
        {
            try
            {
                var dataResult = new List<CalculationUniHead>();
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var сommand = new NpgsqlCommand($"SELECT \"Id\", \"Item\"->'DataOutput'->>'WorkItemName',  \"Item\"->'DataOutput'->>'AccountUserName', \"Item\"->'DataOutput'->>'ComputerName', \"ContractorId\", \"PipeLineId\", \"RouteId\", \"DateWorkItem\" FROM data.\"Calculations\" WHERE \"Id\" = '{Id}'", connection);
                    connection.Open();
                    using (var reader = сommand.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            dataResult.Add(new CalculationUniHead()
                            {
                                Id = (Guid)reader[0],
                                Name = reader[1].ToString(),
                                AccountUserName = reader[2].ToString(),
                                ComputerName = reader[3].ToString(),
                                ContractorId = (Guid)reader[4],
                                PipeLineId = (Guid)reader[5],
                                RouteId = (Guid)reader[6],
                                DateWorkItem = (DateTime)reader[7]
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

        public bool InsertCarrierData(List<CarrierData> carriersData)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    Logger.Logger.Info($"Соединение с БД: [{ConnectionString}] открыто");
                    Logger.Logger.Info($"Очистка таблицы");

                    var sqlDELETE = "DELETE FROM data.carrierdata;";
                    var command = new NpgsqlCommand(sqlDELETE, connection);
                    command.ExecuteNonQuery();

                    Logger.Logger.Info($"Соединение с БД: [{ConnectionString}] открыто");

                    using (var writer = connection.BeginBinaryImport("COPY data.\"carrierdata\" (\"diagtype\", \"defectoscope\", \"carrierdiameter\", \"sensorcount\", \"speedmin\", \"speedmax\", \"cdchange\", \"id\", \"numbersensorsblock\") FROM STDIN (FORMAT BINARY)"))
                    {
                        foreach (var carrierData in carriersData)
                        {
                            writer.StartRow();
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


        /// <summary>
        /// Получить CarrierData
        /// </summary>
        /// <param name="pagenumber">Номер страницы</param>
        /// <param name="pageSize">Размер страницы</param>
        /// <returns></returns>
        public List<CarrierData> GetCarrierData()
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

            var data = GetCarrierData();
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
                    var сommand = new NpgsqlCommand( $"SELECT \"Item\" FROM data.\"Calculations\" WHERE \"Id\" = '{id}'", connection );
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
                    var сommand = new NpgsqlCommand( $"SELECT \"Item\" FROM data.\"Calculations\" WHERE \"Id\"  IN ({string.Join(", ", arr)})", connection );
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
        /// Получить Json файл по id из БД
        /// </summary>
        /// <param name="id">Уникальный номер Json'a</param>
        /// <returns></returns>
        public bool GetDeleteJson( Guid id )
        {
            try
            {
                using( var connection = new NpgsqlConnection( ConnectionString ) )
                {
                    var сommand = new NpgsqlCommand( $"DELETE FROM data.\"Calculations\" WHERE \"Id\" = '{id}'", connection );
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


        public void DataAgentInsertJsonLdiFiles(List<KeyValuePair<Guid, string>> jsons)
        {
            try
            {
                MailDistribution( jsons, Resource.ProfileUser);
                InsertJson( jsons );
            }
            catch ( Exception e )
            {
                Logger.Logger.Info( e.Message );
            }
            finally
            {
                
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
