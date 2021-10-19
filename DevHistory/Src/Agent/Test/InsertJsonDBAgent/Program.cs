using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using Diascan.Agent.DataBaseAgentAccess;
using Diascan.Agent.Types;
using Diascan.Agent.TaskManager;
using LiteDB;

namespace TestInsertJsonDBAgent
{
    public class Program
    {
        public static void Main( string[] args )
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"Start");
            //СheckingPostEmeil();
            Console.WriteLine($@" GetCarrierData : {AppDomain.CurrentDomain.BaseDirectory + @"Files"}");
            GetCarrierData(AppDomain.CurrentDomain.BaseDirectory + @"Files");
            //FilesIsertDBAgent(AppDomain.CurrentDomain.BaseDirectory + @"Files");
            //GetFileDirectoryDataBase(AppDomain.CurrentDomain.BaseDirectory + @"Files");
            Console.WriteLine(@"STOP");
            Console.ReadKey();
        }


        /// <summary>
        /// Проверка отправки письма
        /// </summary>
        private static void СheckingPostEmeil()
        {
            var workManager = new WorkManager(null, null, null);
            var fileNameJson = $"051a2e90-d222-4a15-9666-3c6545a0d222.json";
            byte[] sendData;
            var json = String.Empty;
            json = File.ReadAllText($@"C:\Users\SharovVY\Desktop\отчеты\ZBE01_09.06.2020_21.29.54.Json", Encoding.UTF8);
            var res = false;
            if (json != String.Empty)
            {
                sendData = Encoding.UTF8.GetBytes(json);
                //res = workManager.SetFile($@"http://vds01-tetemp-13/api/Agent/UploadJsonLDIFiles", fileNameJson, sendData); // тестирование на тестовом сервере
                res = workManager.SetFile($@"http://localhost:50156/api/Agent/UploadJsonLDIFiles", fileNameJson, sendData); //локальное тестирование
                //res = workManager.SetFile($@"http://vds01-pemsiis49:8221/api/Agent/UploadJsonLDIFiles", fileNameJson, sendData);  //тестирование на продуктиве
            }
            Console.WriteLine($@"{res}");
        }

        private static void FilesIsertDBAgent( string path )
        {
            const string dirictoriName = "Files";
            path = AppDomain.CurrentDomain.BaseDirectory + dirictoriName;
            var agentDataJson =  new AgentDataJson( ConfigurationManager.ConnectionStrings["AgentConnectionString"].ConnectionString,
                                                              $@"{path}\Log",
                                                              ConfigurationManager.AppSettings["ApiUrlStaffWrapper"],
                                                              ConfigurationManager.AppSettings["NotificationApiUrl"],
                                                              ConfigurationManager.AppSettings["ApplicationCodeStaffWrapper"],
                                                              ConfigurationManager.AppSettings["SmptNotificationSecurityKey"],
                                                              ConfigurationManager.AppSettings["WebApiAgentUrl"] );

            var pathsFiles = Directory.EnumerateFiles( path );
            if( pathsFiles != null )
            {
                var jsons = new List<KeyValuePair<Guid, string>>();
                foreach( var pathFile in pathsFiles )
                {
                    var fileInfo = new FileInfo( pathFile );
                    if (fileInfo.Extension.ToLower() == ".json" )
                    {
                        var json = String.Empty;
                        json = System.IO.File.ReadAllText(pathFile, Encoding.UTF8);
                        if (json != String.Empty)
                            jsons.Add( new KeyValuePair<Guid, string>( new Guid( Path.GetFileNameWithoutExtension( fileInfo.Name ) ), json ) );
                    }
                }
                if( jsons.Count > 0 )
                    agentDataJson.DataAgentInsertJsonLdiFiles( jsons  );
            }
        }

        private static void GetFileDirectoryDataBase( string path )
        {
            var agentDataJson = new AgentDataJson( "server=sds01-depgsql01;Port=5432;database=DBAgent;User Id=agent;Password=Pa3b487fo;Convert Infinity DateTime=true; ",
                                                   $@"{path}\Log",
                                                   ConfigurationManager.AppSettings["ApiUrlStaffWrapper"],
                                                   ConfigurationManager.AppSettings["NotificationApiUrl"],
                                                   ConfigurationManager.AppSettings["ApplicationCodeStaffWrapper"],
                                                   ConfigurationManager.AppSettings["SmptNotificationSecurityKey"],
                                                   ConfigurationManager.AppSettings["WebApiAgentUrl"] );
            var s = agentDataJson.GetJsonsHeads(2,5);
        }

        /// <summary>
        /// Форменрование справочника Carrier
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool GetCarrierData(string path)
        {
            if (Directory.Exists(path))
            {
                var pathXlsx = path + @"\HandbookIdentifierGuide.xlsx";
                var excelToLiteDb = new ExcelToLiteDB.ExcelToLiteDB(pathXlsx);
                if (excelToLiteDb.Convert())
                {
                    var agentDataJson = new AgentDataJson(connectionString:   ConfigurationManager.ConnectionStrings["AgentConnectionString"].ConnectionString,
                                                          filePath:           $@"{path}\Log",
                                                          apiUrlStaffWrapper: ConfigurationManager.AppSettings["ApiUrlStaffWrapper"],
                                                          notificationApiUrl: ConfigurationManager.AppSettings["NotificationApiUrl"],
                                                          applicationCode:    ConfigurationManager.AppSettings["ApplicationCodeStaffWrapper"],
                                                          securityKey:        ConfigurationManager.AppSettings["SmptNotificationSecurityKey"],
                                                          webApiAgentUrl:     ConfigurationManager.AppSettings["WebApiAgentUrl"]);
                    Console.WriteLine($@" confign
                                          {ConfigurationManager.ConnectionStrings["AgentConnectionString"].ConnectionString},
                                          {$@"{path}\Log"},
                                          {ConfigurationManager.AppSettings["ApiUrlStaffWrapper"]},
                                          {ConfigurationManager.AppSettings["NotificationApiUrl"]},
                                          {ConfigurationManager.AppSettings["ApplicationCodeStaffWrapper"]},
                                          {ConfigurationManager.AppSettings["SmptNotificationSecurityKey"]},
                                          {ConfigurationManager.AppSettings["WebApiAgentUrl"]})");

                    using (var dbSensorMediaIdentifier = new LiteDatabase(path + @"\CarrierData.db"))
                        return agentDataJson.InsertCarrierData(dbSensorMediaIdentifier.GetCollection<CarrierData>("CarrierData").FindAll().ToList());
                }
                else
                {
                    Console.WriteLine($@"Error convert file - {pathXlsx}");
                    return false;
                }
            }
            else
            {
                Console.WriteLine($@"Not path - {path}");
                return false;
            }
        }
    }
}
