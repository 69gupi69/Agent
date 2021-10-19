using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diascan.Agent.DataAccess;
using Diascan.Agent.ModelDB;

namespace TestInsertJsonDBAgent
{
    public class Program
    {
        public static void Main( string[] args )
        {
            FilesIsertDBAgent( AppDomain.CurrentDomain.BaseDirectory + @"Files" );
            //GetFileDirectoryDataBase( AppDomain.CurrentDomain.BaseDirectory + @"Files" );
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
                                                              ConfigurationManager.AppSettings["PermissionCodeStaffWrapper"],
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
                                                             ConfigurationManager.AppSettings["PermissionCodeStaffWrapper"],
                                                             ConfigurationManager.AppSettings["SmptNotificationSecurityKey"],
                                                             ConfigurationManager.AppSettings[" WebApiAgentUrl"] );
            var s = agentDataJson.GetJsonsHeads(2,5);
        }
    }
}
