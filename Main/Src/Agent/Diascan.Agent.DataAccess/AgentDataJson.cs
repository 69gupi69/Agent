using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Globalization;
using Diascan.Agent.ModelDB;
using DiCore.Lib.Web;
using DiCore.Wrappers.Notification;
using Npgsql;
using Newtonsoft.Json;
using NpgsqlTypes;
using Staff.Wrapper;
using Staff.Wrapper.Model;

namespace Diascan.Agent.DataAccess
{
    public class AgentDataJson
    {
        public string ConnectionString         { get; protected set; }
        public string ApiUrl                   { get; protected set; }
        public string NotificationApiUrl       { get; protected set; }
        public string ApplicationCode          { get; protected set; }
        public string PermissionCode           { get; protected set; }
        public string SecurityKey              { get; protected set; }
        public string WebApiAgentUrl           { get; protected set; }
        public JsonSerializerSettings Settings { get; protected set; }
        internal const string DateFormat    = "yy-MM-dd";
        internal const string T             = "T";
        internal const string Colon         = ":";
        internal const string Hyphen        = "-";
        internal const string Title         = @"Рассылка Excel отчета ПО ""ПДИ"" ";
        internal const int    LimitKilobyte = 4197376;

        public AgentDataJson( string connectionString, string filePath, string apiUrl, string notificationApiUrl, string applicationCode, string permissionCode, string securityKey, string webApiAgentUrl )
        {
            ConnectionString = connectionString;
            Logger.Logger.InitLogger( filePath );
            ApiUrl             = apiUrl;
            NotificationApiUrl = notificationApiUrl;
            ApplicationCode    = applicationCode;
            PermissionCode     = permissionCode;
            SecurityKey        = securityKey;
            WebApiAgentUrl     = webApiAgentUrl;
            Settings           = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
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
            if ( !String.IsNullOrEmpty(json) )
            {
                var calculation     = JsonConvert.DeserializeObject<Calculation>( json, Settings );
                var shortDateFormat = DateTime.Now.ToString( DateFormat );
                var shortTimeFormat = DateTime.Now.ToString( T ).Replace( Colon, Hyphen );
                    fileName        = $"{ calculation.DataOutput.WorkItemName }_{ shortDateFormat }_{ shortTimeFormat }.xls";
                    buff            = new Manager.ExportToExcel().CreateExcelFile( calculation, out flag );
            }
            else
            {
                fileName = String.Empty;
                buff     = null;
            }

            return flag;
        }


        /// <summary>
        /// Формеруем Excel'и и рассылаем всем пользователям,
        /// которые имеют рольи User в Staff
        /// </summary>
        /// <param name="jsons">Список Calculation</param>
        public void MailDistribution( List<KeyValuePair<Guid, string>> jsons )
        {
            var message            = "Данное сообщение создано автоматически. Пожалуйста, не отвечайте на это письмо.\n";
            var files              = new List<FileParameter>();
            var CountElementsAdded = 0; //количество добавленных элементов
            var kilobyteExcel      = 0;

            foreach ( var json in jsons )
            { 
                if ( GetByteExcel( json.Value, out var fileName, out var buff ) )
                {
                    kilobyteExcel = kilobyteExcel + buff.Length;
                    if( kilobyteExcel >= LimitKilobyte ) break;

                    files.Add( new FileParameter( buff, fileName ) );
                    Logger.Logger.Info( $" Файл {fileName} Excel сформирован " );
                }
                else
                {
                    Logger.Logger.Info( $" Файл {fileName} Excel НЕ сформирован !!! " );
                }
                CountElementsAdded++;
            };

            var staffWrapper = new StaffWrapper( new StaffConnectionParameters( ApiUrl, ApplicationCode ) );
            var users        = staffWrapper.GetUsersForPermission( PermissionCode );
            var emailUser    = new string[users.Length];

            Parallel.For( 0, users.Length, j => { emailUser[j] = users[j].Email; } );

            var notificationWrapper = new NotificationWrapper( new NotificationWrapperParameters( NotificationApiUrl, ApplicationCode, SecurityKey ) );

            if ( kilobyteExcel >= LimitKilobyte )
            {
                message += "Размер вложения превысел допустимый сервером предел. Перейдите по ссылке(ам) для скачивания:\n";
                var s   = 0;
                foreach ( var json in jsons )
                {
                    if ( s >= CountElementsAdded )
                        message = message + WebApiAgentUrl + $"ExcelFromDataBase/{json.Key}\n";
                    s++;
                }

                notificationWrapper.SendMessage( emailUser, emailUser, Title, message, null, files.ToArray() );
            }
            else
            {
                notificationWrapper.SendMessage( emailUser, emailUser, Title, message, null, files.ToArray() );
            }

            Logger.Logger.Info( "Excel'и файлы отправлены" );
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
                var message = "Данное сообщение создано автоматически. Пожалуйста, не отвечайте на это письмо.\n";
                var files = new List<FileParameter>();
                var CountElementsAdded = 0; //количество добавленных элементов
                var kilobyteExcel = 0;

                foreach( var json in jsons )
                {
                    if( GetByteExcel( json.Value, out var fileName, out var buff ) )
                    {
                        kilobyteExcel = kilobyteExcel + buff.Length;
                        if( kilobyteExcel >= LimitKilobyte )
                            break;

                        files.Add( new FileParameter( buff, fileName ) );
                        Logger.Logger.Info( $" Файл {fileName} Excel сформирован " );
                    }
                    else
                    {
                        Logger.Logger.Info( $" Файл {fileName} Excel НЕ сформирован !!! " );
                    }
                    CountElementsAdded++;
                };

                var staffWrapper = new StaffWrapper( new StaffConnectionParameters( ApiUrl, ApplicationCode ) );
                var users        = staffWrapper.GetUsersForPermission( PermissionCode );
                var emailUser    = new string[idUsersEmail.Count];

                var j = 0;
                foreach (var user in users)
                {
                    foreach (var idUserEmail in idUsersEmail)

                        if (user.Id == idUserEmail)
                        {
                            emailUser[j] = user.Email;
                            j++;
                        }
                }

                var notificationWrapper = new NotificationWrapper( new NotificationWrapperParameters( NotificationApiUrl, ApplicationCode, SecurityKey ) );

                if( kilobyteExcel >= LimitKilobyte )
                {
                    message += "Размер вложения превысел допустимый сервером предел. Перейдите по ссылке(ам) для скачивания:\n";
                    var s = 0;
                    foreach( var json in jsons )
                    {
                        if( s >= CountElementsAdded )
                            message = message + WebApiAgentUrl + $"ExcelFromDataBase/{json.Key}\n";
                        s++;
                    }

                    notificationWrapper.SendMessage( emailUser, emailUser, Title, message, null, files.ToArray() );
                }
                else
                {
                    notificationWrapper.SendMessage( emailUser, emailUser, Title, message, null, files.ToArray() );
                }

                Logger.Logger.Info( "Excel'и файлы отправлены" );
                return true;
            }
            catch (Exception e)
            {
                Logger.Logger.Info( "Excel'и файлы не отправлены" );
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
                    Logger.Logger.Info( $"Соединение с БД: [{ConnectionString}] открыто" );

                    using ( var writer = connection.BeginBinaryImport( "COPY data.\"Calculations\" (\"Id\", \"Item\",\"ContractorId\", \"PipeLineId\",\"RouteId\", \"DateWorkItem\") FROM STDIN (FORMAT BINARY)" ) )
                    {
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
                                Logger.Logger.Info( $"Json:{calculation.DataOutput.WorkItemName}, добавлен в базу" );
                            }
                        }
                        writer.Complete();
                    }

                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                        Logger.Logger.Info( $"Соединение с БД: [{ConnectionString}] закрыто" );
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
        public List<CalculationHead> GetJsonsHeads(int pagenumber, int pageSize)
        {
            try
            {
                var dataResult = new List<CalculationHead>();
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
                            dataResult.Add(new CalculationHead()
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
                        while( reader.Read() )
                            dataResult = reader[0].ToString();
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
                            dataResult.Add( new KeyValuePair<Guid, string>( listIGuids[i], reader[0].ToString() ) ); 
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


        public void DataAgentInsertJsonLdiFiles( List<KeyValuePair<Guid, string>> jsons )
        {
            try
            {
                MailDistribution( jsons );
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
    }
}
