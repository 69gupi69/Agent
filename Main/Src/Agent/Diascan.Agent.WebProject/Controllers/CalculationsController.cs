using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Diascan.Agent.WebProject.Models;
using DiCore.Lib.Web;
using Staff.Wrapper;
using DiCore.Wrappers.Notification;
using Newtonsoft.Json.Linq;

namespace Diascan.Agent.WebProject.Controllers
{
    public class CalculationsController : BaseController
    {
        // GET: Calculations
        public ActionResult Index()
        {
            return View();
        }

        public CalculationsController(IStaffWrapper staffWrapper, WebApiWrapper webApiWrapper) : base(staffWrapper, webApiWrapper)
        {
        }       
    }
}