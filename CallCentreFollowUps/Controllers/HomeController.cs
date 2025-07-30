using CallCentreFollowUps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using System.Threading.Tasks;
using System.Web.Security;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.EMMA;
using System.Web.WebPages;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Entity;
using Microsoft.Ajax.Utilities;

namespace CallCentreFollowUps.Controllers
{
    public class HomeController : Controller
    {
        CallCentreTrackerEntities1 db = new CallCentreTrackerEntities1();

        static string role = string.Empty;
        static int? agentid = null;
        static string history = string.Empty;

        public bool CheckIn1 { get; private set; }
        public bool CheckIn2 { get; private set; }
        public bool CheckIn3 { get; private set; }
        public bool CheckIn4 { get; private set; }
        public object CheckInDate { get; private set; }
        //public object CheckInLevel { get; private set; }

        public ActionResult Index()
        {
            //ViewBag.CurrentUserFullName = CommonMethods.CurrentUserFullName;
            var strUserDetails = Request.LogonUserIdentity.Name;
            var CurrentUserName = strUserDetails.Split('\\')[1].Split(' ')[0];
            CurrentUserName = CurrentUserName.Substring(0, 1).ToUpper() + CurrentUserName.Substring(1);
            ViewBag.UserName = CurrentUserName;

            return RedirectToAction("Index", "Issues");


        }

    }
}