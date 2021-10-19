using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using Staff.Wrapper;
using Diascan.Agent.WebProject.Models;

namespace Diascan.Agent.WebProject.Controllers
{
    public class IdentifiersController : BaseController
    {
        // GET: Identifiers
        public ActionResult Index()
        {
            return View();
        }

        public IdentifiersController(IStaffWrapper staffWrapper, WebApiWrapper webApiWrapper) : base(staffWrapper, webApiWrapper)
        {
        }

    }
}
