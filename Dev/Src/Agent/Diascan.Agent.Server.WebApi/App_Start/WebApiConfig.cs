using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using DiCore.Lib.WebApi.Cors;
using DiCore.Lib.WebApi.Routing;

namespace Diascan.Agent.Server.WebApi
{
    public static class WebApiConfig
    {
        public static void Register( HttpConfiguration config )
        {
            config.Services.Replace( typeof( IHttpControllerSelector ), new NamespaceHttpControllerSelector( config ) );
            // Web API configuration and services
            config.MessageHandlers.Add( new CorsHandler() );
            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);
        }


    }
}
