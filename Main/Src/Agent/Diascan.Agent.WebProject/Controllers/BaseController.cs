using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Diascan.Agent.WebProject.Models;
using DiCore.Lib.Web;
using Staff.Wrapper;

namespace Diascan.Agent.WebProject.Controllers
{

    public class BaseController : Controller
    {
        protected readonly IStaffWrapper staffWrapper;
        protected Uri WebApiUrl         { get; }
        protected Uri StaffApiUrl       { get; }
        protected Uri DiagnostWebApiUrl { get; }


        protected IEnumerable<ClientAttribute> GetClientAttributesForRequestParameters( NameValueCollection parameters )
        {
            foreach ( string item in parameters )
                yield return new ClientAttribute( item, parameters[item] );
        }

        public BaseController( IStaffWrapper staffWrapper, WebApiWrapper webApiWrapper )
        {
            this.staffWrapper        = staffWrapper;
            var userData             = staffWrapper.GetUserData();
            var contractorData       = staffWrapper.GetContractorData();
            ViewBag.UserId           = staffWrapper.GetUserId();
            ViewBag.UserRoles        = staffWrapper.GetUserProfiles();
            ViewBag.ApplicationId    = staffWrapper.GetApplicationData().ApplicationId;
            ViewBag.ApplicationCode  = staffWrapper.GetApplicationData().ApplicationCode;
            ViewBag.UserEmail        = staffWrapper.GetUserData().Email;
            ViewBag.UserLogin        = staffWrapper.GetUserLogin();
            ViewBag.Settings         = new { language = "RU" };
            ViewBag.EqName           = Label.EquipmentName;
            ViewBag.BrandName        = Label.BrandName;
            ViewBag.ISName           = Label.ISName;
            ViewBag.VersionText      = $"{Label.VersionText} {GetType().Assembly.GetName().Version}";
            ViewBag.UserName         = $"{userData.LastName} {userData.FirstName} {userData.SecondName}";
            ViewBag.UserContractorId = contractorData.ContractorId;
            ViewBag.ContractorName   = contractorData.ContractorShortName;
            ViewBag.WebApiUrl        = webApiWrapper.WebApiUrl;
            ViewBag.StaffApiUrl      = webApiWrapper.StaffApiUrl;
            ViewBag.WebApiMainUrl    = webApiWrapper.WebApiMainUrl;
            ViewBag.UserRoles        = staffWrapper.GetUserPermissions().Select(role => role.Code).ToArray();
        }
    }
}