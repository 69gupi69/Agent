using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace TestUpload
{
    public class RequestState
    {
        // This class stores the state of the request.
        const int BUFFER_SIZE = 1024;
        public StringBuilder requestData;
        public byte[] bufferRead;
        public WebRequest request;
        public WebResponse response;
        public Stream responseStream;
        public RequestState()
        {
            bufferRead = new byte[BUFFER_SIZE];
            requestData = new StringBuilder( "" );
            request = null;
            responseStream = null;
        }
    }

    class Program
    {
        public static ManualResetEvent allDone = new ManualResetEvent( false );
        const int BUFFER_SIZE = 1024;
        static void Main( string[] args )
        {
            //GetDirectori();

            HttpWebRequest req = ( HttpWebRequest )HttpWebRequest.Create( /*"http://vds01-tetemp-13:50156/api/Agent/"*/ "http://localhost:50156/api/Agent/" );
            HttpWebResponse rsp = ( HttpWebResponse )req.GetResponse();
            if (HttpStatusCode.OK == rsp.StatusCode)
            {
                try
                {
                    rsp.Close();
                    InsertJson();
                    //InsertLog();
                    //GetJsons();
                    //GetJsonsHeadFromDataBase();
                    //GetDirectori();

                    //string path = AppDomain.CurrentDomain.BaseDirectory + "Files";

                    //if( !Directory.Exists( path ) )
                    //    Directory.CreateDirectory( path );

                    //string DBfilePath = path + @"/DirectoryDataModel.db";

                    //FileInfo infoDBfilePath = new FileInfo(DBfilePath);

                    //DateTime dateTimeUtc = DateTime.UtcNow;
                    //DateTime creationTimeUtc = infoDBfilePath.CreationTimeUtc;

                    //var lastUpdate = dateTimeUtc - creationTimeUtc;

                    //TimeSpan interval = new TimeSpan(0, 24, 0, 0);

                    //var hourXUpdate = interval.Subtract(lastUpdate);

                    //if (hourXUpdate >= TimeSpan.Zero && hourXUpdate <= interval)
                    //{

                    //    Console.WriteLine(hourXUpdate);

                    //    Console.WriteLine(hourXUpdate.Seconds);

                    //    Console.WriteLine(hourXUpdate.TotalSeconds);
                    //}



                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    Console.ReadKey();
                }
            }
        }

        private static void InsertJson()
        {


            try
            {
                HttpWebRequest req = ( HttpWebRequest )HttpWebRequest.Create( /*"http://vds01-tetemp-13:50156/api/Agent/"*/ "http://localhost:50156/api/Agent/" );
                HttpWebResponse rsp = ( HttpWebResponse )req.GetResponse();
                if( HttpStatusCode.OK == rsp.StatusCode )
                {
                    // HTTP = 200 - Интернет безусловно есть!
                    rsp.Close();
                    Console.WriteLine( true );
                    var pathsFiles = Directory.EnumerateFiles( AppDomain.CurrentDomain.BaseDirectory + @"Files" );
                    if( pathsFiles != null )
                    {
                        Dictionary<string, byte[]> listBuf = new Dictionary<string, byte[]>();
                        foreach( var pathFileJson in pathsFiles )
                        {
                            var fileInfo = new FileInfo( pathFileJson );
                            if( fileInfo.Extension.ToLower() == ".json" )
                                listBuf.Add( fileInfo.Name , FileToByteArray( pathFileJson ) );
                        }
                        SetFile( "http://localhost:50156/api/Agent/UploadJsonLDIFiles" /*"http://vds01-tetemp-13:50156/api/Agent/UploadJsonLDIFiles"*/, listBuf );
                    }
                }
                else
                {
                    // сервер вернул отрицательный ответ, возможно что инета нет
                    rsp.Close();
                    Console.WriteLine( false );
                }
            }
            catch( Exception e )
            {
                Console.WriteLine( e );
                throw;
            }
            finally
            {
                Console.ReadKey();
            }
        }

        private static void InsertLog()
        {


            try
            {
                HttpWebRequest req = ( HttpWebRequest )HttpWebRequest.Create( /*"http://vds01-tetemp-13:50156/api/Agent/"*/ "http://localhost:50156/api/Agent/" );
                HttpWebResponse rsp = ( HttpWebResponse )req.GetResponse();
                if( HttpStatusCode.OK == rsp.StatusCode )
                {
                    // HTTP = 200 - Интернет безусловно есть!
                    rsp.Close();
                    Console.WriteLine( true );
                    var pathsFiles = Directory.EnumerateFiles( AppDomain.CurrentDomain.BaseDirectory + @"Files" );
                    foreach( var pathFileJson in pathsFiles )
                    {
                        var fileInfo = new FileInfo( pathFileJson );
                        if( fileInfo.Extension.ToLower() == ".log" )
                            SetFile2( "http://localhost:50156/api/Agent/UploadLogFiles" /*"http://vds01-tetemp-13:50156/api/Agent/UploadJsonLDIFiles"*/, fileInfo.FullName );
                    }
                }
                else
                {
                    // сервер вернул отрицательный ответ, возможно что инета нет
                    rsp.Close();
                    Console.WriteLine( false );
                }
            }
            catch( Exception e )
            {
                Console.WriteLine( e );
                throw;
            }
            finally
            {
                Console.ReadKey();
            }
        }


        private static void GetDirectori()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Files";

            if( !Directory.Exists( path ) )
                Directory.CreateDirectory( path );

            string filePath = path + @"/DirectoryDataModel.db";


            try
            {
                var response = HttpWebRequest.Create( "http://localhost:50156/api/Agent/FileDirectoryDataBase" /*"http://vds01-tetemp-13:50156/api/Agent/"*/ ).GetResponse();

                var streamContent = new StreamContent( response.GetResponseStream() ?? throw new InvalidOperationException() );

                var s = streamContent.ReadAsByteArrayAsync();

                byte[] bufBytes = s.Result;

                FileStream fileStream = new FileStream( filePath, FileMode.Create, FileAccess.Write );

                fileStream.WriteAsync( bufBytes, 0, bufBytes.Length );
            }
            catch( Exception e )
            {
                Console.WriteLine( e );
                throw;
            }
            finally
            {
                Console.ReadKey();
            }
        }


        private static void GetJsons()
        {
            try
            {
                RequestState myRequestState = new RequestState();

                var guid = new Guid("eb203b35-f015-4a39-968c-a62692f5142a");
                var url  = $"http://localhost:50156/api/Agent/JsonFromDataBase/{guid}"
                         /*$"http://vds01-tetemp-13:50156/api/Agent/JsonFromDataBase/{guid}"*/;
                var sdf = WebRequest.Create( url );

                myRequestState.request = sdf;

                var response = sdf.BeginGetResponse( new AsyncCallback( RespCallback ), myRequestState );
                allDone.WaitOne();
                myRequestState.response.Close();
            }
            catch( Exception e )
            {
                Console.WriteLine( e );
                throw;
            }
            finally
            {
                Console.ReadKey();
            }
        }

        private static void GetJsonsHeadFromDataBase()
        {
            try
            {
                RequestState myRequestState = new RequestState();

                const int pagenumber = 1;
                const int pagesize   = 10;

                var url1 = $"http://localhost:50156/api/Agent/JsonsHeadFromDataBase/{pagenumber}/{pagesize}"
                         /*"http://vds01-tetemp-13:50156/api/Agent/JsonsHeadFromDataBase/{pagenumber}/{pagesize}"*/;
                var url2 = $"http://localhost:50156/api/Agent/JsonsHeadFromDataBase?pagenumber={pagenumber}pagesize={pagesize}"
                         /*"http://vds01-tetemp-13:50156/api/Agent/JsonsHeadFromDataBase?pagenumber={pagenumber}&pagesize={pagesize}"*/;

                var sdf = WebRequest.Create(url1);

                myRequestState.request = sdf;

                var response = sdf.BeginGetResponse( new AsyncCallback( RespCallback ), myRequestState );
                allDone.WaitOne();
                myRequestState.response.Close();

            }
            catch( Exception e )
            {
                Console.WriteLine( e );
                throw;
            }
            finally
            {
                Console.ReadKey();
            }
        }

        private static void RespCallback( IAsyncResult asynchronousResult )
        {
            try
            {
                // Set the State of request to asynchronous.
                RequestState myRequestState = ( RequestState )asynchronousResult.AsyncState;
                WebRequest myWebRequest1 = myRequestState.request;
                // End the Asynchronous response.
                myRequestState.response = myWebRequest1.EndGetResponse( asynchronousResult );
                // Read the response into a 'Stream' object.
                Stream responseStream = myRequestState.response.GetResponseStream();
                myRequestState.responseStream = responseStream;
                // Begin the reading of the contents of the HTML page and print it to the console.
                IAsyncResult asynchronousResultRead = responseStream.BeginRead( myRequestState.bufferRead, 0, BUFFER_SIZE, new AsyncCallback( ReadCallBack ), myRequestState );

            }
            catch( WebException e )
            {
                Console.WriteLine( "WebException raised!" );
                Console.WriteLine( "\n{0}", e.Message );
                Console.WriteLine( "\n{0}", e.Status );
            }
            catch( Exception e )
            {
                Console.WriteLine( "Exception raised!" );
                Console.WriteLine( "Source : " + e.Source );
                Console.WriteLine( "Message : " + e.Message );
            }
        }


        private static void ReadCallBack( IAsyncResult asyncResult )
        {
            try
            {
                // Result state is set to AsyncState.
                RequestState myRequestState = ( RequestState )asyncResult.AsyncState;
                Stream responseStream = myRequestState.responseStream;
                int read = responseStream.EndRead( asyncResult );
                // Read the contents of the HTML page and then print to the console.
                if( read > 0 )
                {
                    myRequestState.requestData.Append( Encoding.UTF8.GetString( myRequestState.bufferRead, 0, read ) );
                    IAsyncResult asynchronousResult = responseStream.BeginRead( myRequestState.bufferRead, 0, BUFFER_SIZE, new AsyncCallback( ReadCallBack ), myRequestState );
                }
                else
                {
                    Console.WriteLine( "\nThe HTML page Contents are:  " );
                    if( myRequestState.requestData.Length > 1 )
                    {
                        string sringContent;
                        sringContent = myRequestState.requestData.ToString();
                        Console.WriteLine( sringContent );
                    }
                    Console.WriteLine( "\nPress 'Enter' key to continue........" );
                    responseStream.Close();
                    allDone.Set();
                }
            }
            catch( WebException e )
            {
                Console.WriteLine( "WebException raised!" );
                Console.WriteLine( "\n{0}", e.Message );
                Console.WriteLine( "\n{0}", e.Status );
            }
            catch( Exception e )
            {
                Console.WriteLine( "Exception raised!" );
                Console.WriteLine( "Source : {0}", e.Source );
                Console.WriteLine( "Message : {0}", e.Message );
            }

        }



        public static void SetFile2( String serviceUrl, String file )
        {
            try
            {
                using ( var webClient = new WebClient() )
                {
                    var responseArray  = webClient.UploadFile( serviceUrl, "POST", file );
                    Console.WriteLine( Encoding.UTF8.GetString( responseArray ) );
                }
            }
            catch (Exception e)
            {
                Console.WriteLine( e );
            }
        }

        public static void SetFile( String serviceUrl, Dictionary<string, byte[]> listBuf )
        {
            try
            {
                using( var client = new HttpClient() )
                {
                    client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
                    using( var content = new MultipartFormDataContent() )
                    {
                        foreach (var bytese in listBuf )
                        {
                            var fileContent = new ByteArrayContent( bytese.Value );//(System.IO.File.ReadAllBytes(fileName));
                            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue( "attachment" ) { FileName = bytese.Key };
                            content.Add( fileContent );
                        }

                        var res = client. PostAsync( serviceUrl, content ). Result;
                        Console.WriteLine( res.Content+"/n/n" + res.Headers + "/n/n" + res.StatusCode + "/n/n" );
                    }
                }
            }
            catch( Exception e )
            {
                Console.WriteLine( e );
            }
        }


        /// <summary>
        /// Чтение файла в буфер
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns name="bufferFile"> </returns>
        static private byte[] FileToByteArray( string filePath )
        {
            FileStream fileStream = new FileStream( filePath, FileMode.Open, FileAccess.Read );
            byte[] buffer = new byte[fileStream.Length];
            fileStream.Read( buffer, 0, buffer.Length );

            // close file reader
            fileStream.Close();
            fileStream.Dispose();

            return buffer;
        }
    }
}
