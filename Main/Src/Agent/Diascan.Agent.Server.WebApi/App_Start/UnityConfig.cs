using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.Practices.Unity;
using Diascan.Agent.ModelDB;
using DiCore.DataAccess.Repository.DataAdapters;
using DiCore.DataAccess.Repository.DataAdapters.Interfaces;
using Unity.WebApi;

namespace Diascan.Agent.Server.WebApi.App_Start
{
    public class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            var connectionString = ConfigurationManager.ConnectionStrings[ConfigKeys.AgentConnectionString].ConnectionString;
            container.RegisterType<IQueryDataAdapter, QueryDataAdapter>(new InjectionConstructor(connectionString));
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}