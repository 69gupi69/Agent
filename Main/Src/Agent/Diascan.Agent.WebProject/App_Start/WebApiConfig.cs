using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using DiCore.Lib.WebApi.Cors;

namespace Diascan.Agent.WebProject.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MessageHandlers.Add(new CorsHandler());

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}