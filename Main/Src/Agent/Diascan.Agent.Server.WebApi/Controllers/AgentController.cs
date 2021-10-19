using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Diascan.Agent.DataAccess;
using Diascan.Agent.ModelDB;
using Diascan.Agent.Server.WebApi.Properties;
using DiCore.DataAccess.Repository.DataAdapters.Interfaces;
using DiCore.DataAccess.Types.CustomModels.Pipeline;
using DiCore.DataAccess.Types.CustomModels.Treeview;
using DiCore.DataAccess.Types.Models.Pipeline;
using DiCore.Lib.SqlDataQuery;
using DiCore.Lib.SqlDataQuery.SqlCode;
using DiCore.Lib.Web;
using DiCore.Wrappers.Main;
using LiteDB;
using Newtonsoft.Json.Linq;
using AgentPipeline            = Diascan.Agent.DirectoryDataModel.Pipeline;
using AgentRoute               = Diascan.Agent.DirectoryDataModel.Route;
using AgentContractorRoutesRef = Diascan.Agent.DirectoryDataModel.ContractorRouteRef;
using AgentContractor          = Diascan.Agent.DirectoryDataModel.Contractor ;
using File = System.IO.File;
using Pipeline                 = DiCore.DataAccess.Types.Models.Pipeline.Pipeline;

namespace Diascan.Agent.Server.WebApi.Controllers
{
    [RoutePrefix( "api/Agent" )]
    public class AgentController : ApiController
    {
        private        AgentDataJson     AgentDataJson;
        private IQueryDataAdapter queryDataAdapter;

        private delegate void OperationDataAgentInsert( List<KeyValuePair<Guid, string>> jsons );

        /// <summary>
        /// Путь к временному хранилищу файлов
        /// </summary>
        private readonly string            Path;

        public AgentController(IQueryDataAdapter queryDataAdapter)
        {
            this.queryDataAdapter = queryDataAdapter;
            StaffWebClient = new SimpleWebClient(ConfigurationManager.AppSettings["ApiUrlStaffWrapper"].Trim());
            const string dirictoriName = "Files";
            Path = AppDomain.CurrentDomain.BaseDirectory + dirictoriName;
            Logger.Logger.InitLogger( $@"{Path}\Log" );
            AgentDataJson = new AgentDataJson( ConfigurationManager.ConnectionStrings[ConfigKeys.AgentConnectionString].ConnectionString,
                                               $@"{Path}\Log",
                                               ConfigurationManager.AppSettings["ApiUrlStaffWrapper"],
                                               ConfigurationManager.AppSettings["NotificationApiUrl"],
                                               ConfigurationManager.AppSettings["ApplicationCodeStaffWrapper"],
                                               ConfigurationManager.AppSettings["PermissionCodeStaffWrapper"],
                                               ConfigurationManager.AppSettings["SmptNotificationSecurityKey"],
                                               ConfigurationManager.AppSettings["WebApiAgentUrl"] );



            var license = new Aspose.Cells.License();
            license.SetLicense( new MemoryStream( Resources.Aspose_Total ) );
        }

        SimpleWebClient StaffWebClient { get; set; }


        // GET api/Agent "http://vds01-tetemp-13:50156/api/Agent/" или "http://localhost:50156/api/Agent/"
        [HttpGet]
        [Route("")]
        [ActionName( "GET" )]
        public IEnumerable<string> Get()
        {
            return new string[] { "Agent1", "Agent2" };
        }

        private void InsertContractors( LiteCollection<AgentContractor> contractorsTebel, TreeviewOutput[] contractors )
        {
            try
            {
                AgentContractor[] agentContractors = new AgentContractor[contractors.Length];
                int i = 0;
                foreach( var contractor in contractors )
                {
                    agentContractors[i] = new AgentContractor { Id = new Guid( contractor.Id.ToString() ), Name = contractor.Name };
                    i++;
                }
                contractorsTebel.Insert( agentContractors );
            }
            catch( Exception e )
            {
                Console.WriteLine( e );
                throw;
            }
        }

        private bool InsertContractorRouteRef( LiteCollection<AgentContractorRoutesRef> сontractorRouteRefTebel, ContractorRouteRef[] сontractorRouteRefs )
        {
            try
            {
                var agentContractorRoutesRefs = new AgentContractorRoutesRef[сontractorRouteRefs.Length];
                int i = 0;
                foreach( var сontractorRouteRef in сontractorRouteRefs )
                {
                    agentContractorRoutesRefs[i] = new AgentContractorRoutesRef
                    {
                        RouteId = new AgentRoute { Id = сontractorRouteRef.RouteId },
                        ContractorId = new AgentContractor { Id = сontractorRouteRef.ContractorId }
                    };
                    i++;
                }
                сontractorRouteRefTebel.Insert( agentContractorRoutesRefs );
            }
            catch( Exception e )
            {
                Console.WriteLine( e );
                return false;
            }
            return true;
        }

        private void InsertRoutes( LiteCollection<AgentRoute> routesTebel, RouteWithDiameter[] routes )
        {
            try
            {
                AgentRoute[] agentRoutes = new AgentRoute[routes.Length];
                int i = 0;
                foreach( var route in routes )
                {
                    agentRoutes[i] = new AgentRoute { Id         = route.Id,
                                                      PipelineId = new AgentPipeline { Id = route.PipelineId },
                                                      Name       = route.Name,
                                                      DiameterMm =  route.DiameterMm };
                    i++;
                }
                routesTebel.Insert( agentRoutes );
            }
            catch( Exception e )
            {
                Console.WriteLine( e );
                throw;
            }
        }
        private void InsertPipelines( LiteCollection<AgentPipeline> pipelinesTebel, Pipeline[] pipelines )
        {
            try
            {
                AgentPipeline[] agentPipelines = new AgentPipeline[pipelines.Length];
                int i = 0;
                foreach( var pipeline in pipelines )
                {
                    agentPipelines[i]      = new AgentPipeline();
                    agentPipelines[i].Id   = pipeline.Id;
                    agentPipelines[i].Name = pipeline.Name;
                    i++;
                }
                pipelinesTebel.Insert( agentPipelines );
            }
            catch( Exception e )
            {
                Console.WriteLine( e );
                throw;
            }
        }

        private void InsertSpeed( LiteCollection<SpeedPipeLineInspectionDevice> speedTebel )
        {

            try
            {
                List<SpeedPipeLineInspectionDevice> spedDirectory = new List<SpeedPipeLineInspectionDevice>()
                {
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.DMU,   0.2f, 2.22f ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.DKK,   0.2f, 2.22f ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.DKU,   0.2f, 2.22f ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.DKM,   0.2f, 2.22f ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.DVU,   0.2f, 3.2f  ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.DKP,   0.2f, 3.2f  ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.USK03, 0.3f, 2f    ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.USK03, 0.2f, 3.2f  ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.USK03, 0.2f, 2f    ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.USK03, 0.2f, 3.22f ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.USK04, 0.2f, 3.2f  ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.MSK,   0.2f, 4f    ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.DMK,   0.2f, 4f    ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.OPT,   0.2f, 6f    ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.PRN,   0.2f, 3f    ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.PRN,   0.2f, 4f    ),
                    new SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice.PRN,   0.2f, 6f    ),
                };
                speedTebel.Insert( spedDirectory.ToArray() );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Создание БД справочника
        /// </summary>
        /// <param name="UrlPath"></param>
        /// <returns></returns>
        private bool CreateDirectoryDataBase( string urlPath, string nameFileDB )
        {
            Task.Run( () => { Logger.Logger.Info( " Начало создания справочника " ); } );
            if( !Directory.Exists( Path ) )
                Directory.CreateDirectory( Path );
            else if ( File.Exists( nameFileDB ) )
            {
                File.Delete( nameFileDB );
            }

            var webWrapper = new DiCoreMainWrapper( new SimpleWebClient( urlPath.Trim() ) );
            string error = string.Empty;
            var contractors         = webWrapper.GetTreeviewContractors();
            var сontractorRouteRefs = webWrapper.GetContractorRouteRefs( out error );
            var routes              = webWrapper.GetRoutesWithDiameter();
            var pipelines           = webWrapper.GetPipelines( out error );

            Task.Run( () => { Logger.Logger.Info( " Данные с WebApiUrl получены " ); } );

            using( var db = new LiteDatabase( nameFileDB ) )
            {
                var contractorsTebel        = db.GetCollection<AgentContractor>( "Contractors" );
                var сontractorRouteRefTebel = db.GetCollection<AgentContractorRoutesRef>( "ContractorRouteRef" );
                var routesTebel             = db.GetCollection<AgentRoute>( "Routes" );
                var pipelinesTebel          = db.GetCollection<AgentPipeline>( "Pipelines" );
                //var speedTebel              = db.GetCollection<SpeedPipeLineInspectionDevice>( "SpeedPipeLineInspectionDevice" );

                BsonMapper.Global.Entity<AgentContractorRoutesRef>().DbRef( x => x.ContractorId, "Contractors" ).DbRef( x => x.RouteId, "Routes" );
                BsonMapper.Global.Entity<AgentRoute>().DbRef( x => x.PipelineId, "Pipelines" );

                Task.WaitAll( new Task[]
                              {
                                  Task.Run( () => { InsertContractors( contractorsTebel, contractors ); } ),
                                  Task.Run( () => { InsertPipelines( pipelinesTebel, pipelines ); } ),
                                  Task.Run( () => { InsertRoutes( routesTebel, routes ); } ),
                                  Task.Run( () => { InsertContractorRouteRef( сontractorRouteRefTebel, сontractorRouteRefs ); } ),
                                  //Task.Run( () => { InsertSpeed( speedTebel ); } )
                              } );
            }

            return true;
        }


        /// <summary>
        /// Метод реализует передачу справочника Agent'у 
        /// </summary>
        /// <returns></returns>
        /// GET "http://vds01-tetemp-13:50156/api/Agent/FileDirectoryDataBase" или "http://localhost:50156/api/Agent/FileDirectoryDataBase"
        [HttpGet]
        [Route( "FileDirectoryDataBase" )]
        [ActionName( "GET" )]
        public HttpResponseMessage GetFileDirectoryDataBase()
        {
            string UrlPath = ConfigurationManager.AppSettings["WebApiUrl"];
            string nameFileDB = Path + @"/DirectoryDataModel.db";

            if( CreateDirectoryDataBase( UrlPath, nameFileDB ) )
            {
                Logger.Logger.Info( " Справочника создан " );

                var stream = new FileStream( nameFileDB, System.IO.FileMode.Open, FileAccess.Read );
                byte[] buffer = new byte[stream.Length];
                stream.Read( buffer, 0, buffer.Length );
                stream.Close();
                stream.Dispose();

                HttpResponseMessage result = new HttpResponseMessage( HttpStatusCode.OK )
                {
                    Content = new ByteArrayContent( buffer )
                };
                
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue( "attachment" );
                result.Content.Headers.ContentDisposition.FileName = System.IO.Path.GetFileName( nameFileDB );
                result.Content.Headers.ContentType = new MediaTypeHeaderValue( "application/octet-stream" );
                result.Content.Headers.ContentLength = buffer.Length;
                Task.Run( () => { Logger.Logger.Info( " Справочника отправлен " ); } );
                return result;
            }
            else
            {
                Task.Run( () => { Logger.Logger.Info( " Справочника неудалось создать " ); } );
                return new HttpResponseMessage( HttpStatusCode.NotFound );
            }
        }


        /// <summary>
        /// Получить количество записей Json'ов из БД
        /// </summary>
        /// <returns>Количество записей</returns>
        /// GET "http://vds01-tetemp-13:50156/api/Agent/CountJsonFromDataBase"
        /// или "http://localhost:50156/api/Agent/CountJsonFromDataBase"
        [HttpGet]
        [Route( "CountJsonFromDataBase" )]
        [ActionName( "GET" )]
        public int GetCountJsonFromDataBase() => AgentDataJson.GetCountJsonFromDataBase();


        // <summary>
        // Метод реализует передачу заголовков Calculation из базы данных клиенту
        // </summary>
        // <param name = "page" > Номер страницы</param>
        // <param name = "pageSize" > Размер страницы</param>
        // <returns></returns>
        // GET "http://vds01-tetemp-13:50156/api/Agent/JsonsHeadFromDataBase/1/10"
        // или "http://vds01-tetemp-13:50156/api/Agent/JsonsHeadFromDataBase?page=1pageSize=10"
        // или "http://localhost:50156/api/Agent/JsonsHeadFromDataBase/1/10"
        // или "http://localhost:50156/api/Agent/JsonsHeadFromDataBase?page=1pageSize=10"
        [HttpGet]
        [Route("JsonsHeadFromDataBase")]
        [Route("JsonsHeadFromDataBase/{page:int}/{pageSize:int}")]
        [ActionName("GET")]
        public IEnumerable<CalculationHead> GetJsonsHeadFromDataBase(int page, int pageSize)
        {
            var heads = AgentDataJson.GetJsonsHeads(page, pageSize);
            var webWrapper = new DiCoreMainWrapper(new SimpleWebClient(ConfigurationManager.AppSettings["WebApiUrl"].Trim()));
            foreach (var head in heads)
            {
                head.PipeLineName = webWrapper.GetPipelineById(head.PipeLineId).Name;
                head.RouteName = webWrapper.GetRouteById(head.RouteId).Name;
                head.ContractorName = (string)GetStaffContractor(head.ContractorId)["ShortName"];
            }
            return heads;
        }


        /// <summary>
        /// Метод реализует передачу заголовков Calculation из базы данных клиенту с фильтрацией
        /// </summary>
        /// <param name="contractorId"> Идентификатор заказчика    </param>
        /// <param name="pipelineId">   Идентификатор трубопровода </param>
        /// <param name="routeId">      Идентификатор участка      </param>
        /// <returns></returns>
        [HttpGet]
        [Route( "JsonsHeadAndCountFromDataBase" )]
        [ActionName("GET")]
        public IHttpActionResult GetCalculations( Guid? contractorId = null, Guid? pipelineId = null, Guid? routeId = null )
        {
            var requestParameters = GridParametersParser.Parse(HttpContext.Current.Request.Params);//new RequestParameters( HttpContext.Current.Request.Params );
            var dbObjects         = new QueryDescription();
            var calc              = dbObjects.AddTable( "data", "Calculations", "calc", true );
            calc.AddCustomColumn( "TotalCount", "COUNT(*) OVER()" );
            calc.AddColumn( "Id", EnDbDataType.Uuid );
            calc.AddCustomColumn( "Name", "\"Item\"->'DataOutput'->>'WorkItemName'" );
            calc.AddCustomColumn( "AccountUserName", "\"Item\"->'DataOutput'->>'AccountUserName'" );
            calc.AddCustomColumn( "ComputerName", "\"Item\"->'DataOutput'->>'ComputerName'" );
            var calcContractorId = calc.AddColumn( "ContractorId", EnDbDataType.Uuid );
            var calcPipeLineId   = calc.AddColumn( "PipeLineId"  , EnDbDataType.Uuid );
            var calcRouteId      = calc.AddColumn( "RouteId"     , EnDbDataType.Uuid );
            calc.AddCustomColumn( "ResponsibleWorkItem", "\"Item\"->'DataOutput'->>'ResponsibleWorkItem'" );            
            calc.AddColumn( "DateWorkItem", EnDbDataType.Timestamp );
            if ( contractorId != null )
            {
                dbObjects.AddCustomCondition( EnConditionType.And, $"\"{calc.Alias}\".\"{calcContractorId.Alias}\"=\'{contractorId}\' " );

                if ( pipelineId != null )
                    dbObjects.AddCustomCondition( EnConditionType.And, $"\"{calc.Alias}\".\"{calcPipeLineId.Alias}\"=\'{pipelineId}\' " );

                if ( routeId != null )
                    dbObjects.AddCustomCondition( EnConditionType.And, $"\"{calc.Alias}\".\"{calcRouteId.Alias}\"=\'{routeId}\' " );
            }
            var queryCreator = new QueryCreator( dbObjects );
            var queryText    = queryCreator.Create( requestParameters );
            var json         = queryDataAdapter.ExecuteReader( queryText, null, "TotalCount" );
            var jsons        = json.GetValue( "data" );
            var dataResult   = new List<CalculationHead>();
            var webWrapper   = new DiCoreMainWrapper( new SimpleWebClient( ConfigurationManager.AppSettings["WebApiUrl"].Trim() ) );
            foreach ( var res in jsons )
            {
                dataResult.Add( new CalculationHead()
                {
                    Id                  = ( Guid )res["Id"],
                    ContractorId        = ( Guid )res["ContractorId"],
                    PipeLineId          = ( Guid)res["PipeLineId"],
                    RouteId             = ( Guid )res["RouteId"],
                    TotalCount          = ( int )res["TotalCount"],
                    Name                = res["Name"].ToString(),
                    AccountUserName     = res["AccountUserName"].ToString(),
                    ComputerName        = res["ComputerName"].ToString(),
                    ContractorName      = (string)GetStaffContractor( ( Guid )res["ContractorId"] )["ShortName"],
                    PipeLineName        = webWrapper.GetPipelineById( ( Guid )res["PipeLineId"] ).Name,
                    RouteName           = webWrapper.GetRouteById( ( Guid )res["RouteId"] ).Name,
                    ResponsibleWorkItem = res["ResponsibleWorkItem"].ToString(),
                    DateWorkItem        = ( DateTime )res["DateWorkItem"]
                });
            }
            var totalCount = dataResult.Count == 0 ? 0 : dataResult[0].TotalCount;
            return Ok( new { totalCount, dataResult } );
        }

        /// <summary>
        /// Метод получает список пользователей приложения Agent с ролью User
        /// </summary>
        [HttpGet]
        [Route("GetAgentUsers")]
        [ActionName("GET")]
        public IHttpActionResult GetAgentUsers(int take, int skip, int page, int pageSize)
        {
            var usersListByPermission = GetUsers();
            var dataResult = new List<UserHeaders>();
            foreach (var res in usersListByPermission)
            {
                dataResult.Add(new UserHeaders()
                {
                    Id = (Guid)res["Id"],
                    FirstName = res["FirstName"].ToString(),
                    SecondName = res["SecondName"].ToString(),
                    LastName = res["LastName"].ToString(),
                    Phone = res["Phone"].ToString(),
                    Email = res["Email"].ToString(),
                    ContractorName = res["ContractorName"].ToString(),
                    PositionName = res["PositionName"].ToString()
                });
            }
            return Ok(new { totalCount = dataResult.Count(), dataResult = dataResult.Skip(skip).Take(pageSize).ToArray()});
        }


        [NonAction]
        private List<JObject> GetUsers()
        {
            var appName = ConfigurationManager.AppSettings["ApplicationCodeStaffWrapper"];
            var permissionName = ConfigurationManager.AppSettings["PermissionCodeStaffWrapper"];

            var usersListByPermission =
                StaffWebClient.Get<List<JObject>>($"auth/applications/{appName}/permissions/{permissionName}/users");//Full");
            return usersListByPermission;
        }

        /// <summary>
        /// Массив Id пользователей приложения Agent с ролью User
        /// </summary>
        [HttpGet]
        [Route("GetAllUserIds")]
        [ActionName("GET")]
        public string[] GetAllUserIds()
        {
            var usersListsByPermission = GetUsers();
            var userIds                = new List<string> ();
            foreach ( var usersList in usersListsByPermission )
                userIds.Add( ( string )usersList["Id"] );
            
            return userIds.ToArray();
        }

        /// <summary>
        /// Получение ОСТа по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор ОСТа</param>
        /// <returns>ОСТ</returns>
        [NonAction]
        private JObject GetStaffContractor(Guid id)
        {
            return JObject.FromObject(StaffWebClient.Get<object>($"affiliatedcompanies/?Id={id.ToString()}"));
        }

        /// <summary>
        /// Получить Json Расчет по id из БД и пердача клиенту
        /// </summary>
        /// <param name="id">Идентификатор Расчета Json'a</param>
        /// <returns>Json Расчета</returns>
        /// GET "http://vds01-tetemp-13:50156/api/Agent/JsonFromDataBase/3672619f-6165-4fde-854a-28daffdaaf76"
        /// или "http://vds01-tetemp-13:50156/api/Agent/JsonFromDataBase?id=3672619f-6165-4fde-854a-28daffdaaf76"
        /// или "http://localhost:50156/api/Agent/JsonFromDataBase/3672619f-6165-4fde-854a-28daffdaaf76"
        /// или "http://localhost:50156/api/Agent/JsonFromDataBase?id=3672619f-6165-4fde-854a-28daffdaaf76"
        [HttpGet]
        [Route( "JsonFromDataBase" )]
        [Route( "JsonFromDataBase/{id:Guid}" )]
        [ActionName( "GET" )]
        public string GetJsonFromDataBase( Guid id ) => AgentDataJson.GetJson( id );


        /// <summary>
        /// Удалить Json Расчета по id из БД и пердача клиенту
        /// </summary>
        /// <param name="id">Идентификатор Расчета Json'a</param>
        /// <returns>Json</returns>
        /// GET "http://vds01-tetemp-13:50156/api/Agent/DeleteJsonFromDataBase/e3a7e530-862e-49f8-a20e-424153523c7a"
        /// или "http://vds01-tetemp-13:50156/api/Agent/DeleteJsonFromDataBase?id=e3a7e530-862e-49f8-a20e-424153523c7a"
        /// или "http://localhost:50156/api/Agent/DeleteJsonFromDataBase/e3a7e530-862e-49f8-a20e-424153523c7a"
        /// или "http://localhost:50156/api/Agent/DeleteJsonFromDataBase?id=3672619f-6165-4fde-854a-28daffdaaf76"
        [HttpGet]
        [Route( "DeleteJsonFromDataBase" )]
        [Route( "DeleteJsonFromDataBase/{id:Guid}" )]
        [ActionName( "GET" )]
        public bool GetDeleteJsonFromDataBase( Guid id ) => AgentDataJson.GetDeleteJson( id );


        /// <summary>
        /// Получить Excel файл по id Расчета из БД и пердача клиенту
        /// </summary>
        /// <param name="id">Идентификатор Расчета Json'a</param>
        /// <returns>Json</returns>
        /// GET "http://vds01-tetemp-13:50156/api/Agent/ExcelFromDataBase/d443a653-4903-4e20-846e-ebc98d27eed9"
        /// или "http://vds01-tetemp-13:50156/api/Agent/ExcelFromDataBase?id=d443a653-4903-4e20-846e-ebc98d27eed9"
        /// или "http://localhost:50156/api/Agent/ExcelFromDataBase/d443a653-4903-4e20-846e-ebc98d27eed9"
        /// или "http://localhost:50156/api/Agent/ExcelFromDataBase?id=d443a653-4903-4e20-846e-ebc98d27eed9"
        [HttpGet]
        [Route( "ExcelFromDataBase" )]
        [Route( "ExcelFromDataBase/{id:Guid}" )]
        [ActionName("GET")]
        public HttpResponseMessage GetExcelFromDataBase( Guid id )
        {
            if ( AgentDataJson.GetExcel( id, out byte[] buffer, out string nameFileDB ) )
            {
                HttpResponseMessage result = new HttpResponseMessage( HttpStatusCode.OK )
                {
                    Content = new ByteArrayContent( buffer )
                };

                result.Content.Headers.ContentDisposition          = new ContentDispositionHeaderValue( "attachment" );
                result.Content.Headers.ContentDisposition.FileName = System.IO.Path.GetFileName( nameFileDB );
                result.Content.Headers.ContentType                 = new MediaTypeHeaderValue( "application/octet-stream" );
                result.Content.Headers.ContentLength               = buffer.Length;
                Logger.Logger.Info( " Справочника отправлен " );
                return result;
            }
            else
            {
                return new HttpResponseMessage( HttpStatusCode.NotFound );
            }
        }


        // GET api/Agent/5
        [HttpGet]
        public string Get( int id )
        {
            return "Agent";
        }

        // POST api/Agent
        [HttpPost]
        public void Post( [FromBody]string value )
        {
        }

        // PUT api/Agent/5
        [HttpPut]
        public void Put( int id, [FromBody]string value )
        {
        }

        /// <summary>
        /// Загрузка Json файлов, в временный каталог "Files"
        /// </summary>
        /// <param name="path">Путь к временному хранилищу файлов</param>
        /// <returns></returns>
        [NonAction]
        private async Task<string[]> UploadFiles( string path )
        {
            // загружаем данные на диск MultipartFormDataStreamProvider( directoryPath )
            // альтернаива - MultipartMemoryStreamProvider() загржаем данные в память
            var streamProvider = new MyMultipartFormDataStreamProvider( path );
            //int const 
            var content = new StreamContent( HttpContext.Current.Request.GetBufferlessInputStream( true ) );
            foreach( var header in Request.Content.Headers )
                content.Headers.TryAddWithoutValidation( header.Key, header.Value );           
            await content.ReadAsMultipartAsync(streamProvider);
            var fileInfo = streamProvider.FileData.Select(i =>
            {
                var info = new FileInfo(i.LocalFileName);
                return $"Файл загружен как {info.FullName} ({info.Length})";
            }).ToArray();

            // Send OK Response along with saved file names to the client. 
            return fileInfo;
        }


        /// <summary>
        /// Чтение файла
        /// </summary>
        /// <param name="datas">Данные прочитоные из файла</param>
        /// <param name="pathFile">Путь к файлу</param>
        /// <param name="fileInfo">Информация о файле</param>
        private void ReadFile( List<KeyValuePair<Guid, string>> datas, string pathFile, string fileInfo )
        {
            string data = String.Empty;
            data = File.ReadAllText( pathFile, Encoding.UTF8 );
            if ( data != String.Empty )
                datas.Add( new  KeyValuePair< Guid, string >( new Guid( fileInfo ), data ) );
            File.Delete( pathFile );
        }


        /// <summary>
        /// Добавление файлыв в базу ДБАгент
        /// </summary>
        private void FilesIsertDBAgent( OperationDataAgentInsert operation )
        {
            var pathsFiles = Directory.EnumerateFiles( Path );
            if( pathsFiles != null )
            {
                List<KeyValuePair<Guid, string>> datasList = new List<KeyValuePair<Guid, string>>();
                foreach( var pathFile in pathsFiles )
                {
                    var fileInfo = new FileInfo( pathFile );
                    if( fileInfo.Extension.ToLower() == Resources.FileExtensionJson )
                    {
                        ReadFile( datasList, pathFile, System.IO.Path.GetFileNameWithoutExtension( fileInfo.Name ) );
                    }
                    else if ( fileInfo.Extension.ToLower() == Resources.FileExtensionLog )
                    {
                        ReadFile( datasList, pathFile, System.IO.Path.GetFileNameWithoutExtension( fileInfo.Name) );
                    }
                }
                if ( datasList.Count > 0)
                    operation( datasList );
            }
        }


        /// <summary>
        /// Рассылка Excel-файла фсем пользователям ИС Агента с профилем User 
        /// </summary>
        /// <param name="id">Иднификатор расчета</param>
        /// <returns></returns>
        /// POST "http://vds01-tetemp-13:50156/api/Agent/MailingExcelFile/d443a653-4903-4e20-846e-ebc98d27eed9"
        /// или  "http://vds01-tetemp-13:50156/api/Agent/MailingExcelFile?id=d443a653-4903-4e20-846e-ebc98d27eed9"
        /// или  "http://localhost:50156/api/Agent/MailingExcelFile/d443a653-4903-4e20-846e-ebc98d27eed9"
        /// или  "http://localhost:50156/api/Agent/MailingExcelFile?id=d443a653-4903-4e20-846e-ebc98d27eed9"
        [HttpGet]
        [Route( "MailingExcelFile" )]
        [Route( "MailingExcelFile/{id:Guid}" )]
        [ActionName( "GET" )]
        public IHttpActionResult MailingExcelEile( Guid id )
        {
            var jsonData = GetJsonFromDataBase( id );
            var jsons = new List<KeyValuePair<Guid, string>> { new KeyValuePair<Guid, string>( id, jsonData )  };
            AgentDataJson.MailDistribution( jsons );
            return Ok();
        }

        /// <summary>
        /// Рассылка Excel-файлов пользователям ИС Агента с профилем User из массива userIds
        /// </summary>
        /// <param name="data">Массив идентификаторов пользователей и Массив идентификаторов расчётов</param>
        /// <returns></returns>
        /// http://localhost:50156/api/Agent/SelectiveMailing?UserIds%5B%5D=2c34e688-bd7e-45c8-a8f3-a1e330aa663a&UserIds%5B%5D=f3cfdef9-44e6-4d2b-b4c6-3938f50ffe55&CalculationIds%5B%5D=66fcd2bf-9190-47dd-86f9-cecaf5cbd08c&CalculationIds%5B%5D=f5793551-4bac-4212-89eb-9bac07075db5&CalculationIds%5B%5D=bf5b1a44-162b-44d0-8a5e-6cc5468d329c&CalculationIds%5B%5D=e36815d5-aa3f-4611-8419-e1653fbb0ac1&CalculationIds%5B%5D=3cecc402-ca90-4faf-9efa-6a25ebbdf228&CalculationIds%5B%5D=8c50439e-2a73-434a-891b-9b0b85f90d95&_=1540272671933
        [HttpGet]
        [Route( "SelectiveMailing" )]
        [ActionName( "GET" )]
        public IHttpActionResult SelectiveMailing( [FromUri] SelectiveMailingParameters data )
        {
            AgentDataJson.Mailing( AgentDataJson.GetJsons( data.CalculationIds ), data.UserIds.ToList() );
            return Ok();
        }


        /// <summary>
        /// Загрузка Json ПДИ файлов от клиента на сервер.
        /// Если папки на диске нет то вылетает сообщение с Exception!
        /// Стабильная загрузка до 268435456 кб.
        /// </summary>
        /// <returns>Возвращаем рузультат работы загруски фала</returns>
        ///      "http://vds01-tetemp-13:50156/api/Agent/UploadJsonLDIFiles"
        ///  или "http://localhost:50156/api/Agent/UploadJsonLDIFiles"
        [HttpPost]
        [Route( "UploadJsonLDIFiles" )]
        [ActionName( "POST" )]
        public async Task<IHttpActionResult> PostUploadJsonLDIFiles()
        {
            if( !Request.Content.IsMimeMultipartContent() )
                throw new HttpResponseException( HttpStatusCode.UnsupportedMediaType );

            if( !Directory.Exists( Path ) )
                Directory.CreateDirectory( Path );

            var message = await UploadFiles( Path );
            FilesIsertDBAgent( AgentDataJson.DataAgentInsertJsonLdiFiles );

            return Ok( message );
        }


        /// <summary>
        /// Загрузка Log файлов от клиента на сервер.
        /// Если папки на диске нет то вылетает сообщение с Exception!
        /// Стабильная загрузка до 268435456 кб.
        /// "http://vds01-tetemp-13:50156/api/Agent/UploadLogFiles" или "http://localhost:50156/api/Agent/UploadLogFiles"
        /// </summary>
        /// <returns>Возвращаем рузультат работы загруски фала</returns>
        [HttpPost]
        [Route( "UploadLogFiles" )]
        [ActionName( "POST" )]
        public async Task<IHttpActionResult> PostUploadLogFiles()
        {
            if( !Request.Content.IsMimeMultipartContent() )
                throw new HttpResponseException( HttpStatusCode.UnsupportedMediaType );

            if( !Directory.Exists( Path ) )
                Directory.CreateDirectory( Path );

            var message = await UploadFiles( Path );
            FilesIsertDBAgent( AgentDataJson.DataAgentInsertLogFiles );

            return Ok( message );
        }

        // DELETE api/values/5
        public void Delete( int id )
        {
        }
    }

    public class MyMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public MyMultipartFormDataStreamProvider( string path ): base( path )
        {

        }

        /// <summary>
        /// Формерование имени файла
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        public override string GetLocalFileName( HttpContentHeaders headers )
        {
            string fileName;
            if( !string.IsNullOrWhiteSpace( headers.ContentDisposition.FileName ) )
            {
                fileName = headers.ContentDisposition.FileName;
            }
            else
            {
                fileName = Guid.NewGuid() + Resources.FileExtensionJson;
            }
            return fileName.Replace( "\"", string.Empty );
        }
    }
}
