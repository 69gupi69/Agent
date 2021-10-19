using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using Diascan.Agent.WebProject.Models;
using DiCore.Wrappers.DocumentStorage;
using Microsoft.Practices.Unity;
using Staff.Wrapper;
using Staff.Wrapper.Web;
using Unity.Mvc5;

namespace Diascan.Agent.WebProject
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container         = new UnityContainer();
            var staffApiUrl       = ConfigurationManager.AppSettings[ConfigKeys.StaffApiUrlKey];
            var webApiUrl         = ConfigurationManager.AppSettings[ConfigKeys.WebApiUrl];
            var applicationCode   = ConfigurationManager.AppSettings[ConfigKeys.ApplicationCodeKey];
            var webApiMainUrl     = ConfigurationManager.AppSettings[ConfigKeys.WebApiMainUrl];

            container.RegisterInstance( new WebApiWrapper()
            {
                StaffApiUrl   = new Uri( staffApiUrl ),
                WebApiUrl     = new Uri( webApiUrl ),
                WebApiMainUrl = new Uri( webApiMainUrl )
            });
            container.RegisterInstance( new StaffConnectionParameters( staffApiUrl, applicationCode ) );
            container.RegisterType<HttpContextBase>( new InjectionFactory( _ => new HttpContextWrapper( HttpContext.Current ) ) );

            container.RegisterType<IStaffWrapper, StaffWrapperWeb>();

            DependencyResolver.SetResolver( new UnityDependencyResolver( container ) );
        }
    }
}