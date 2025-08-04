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
            var strUserDetails = Request.LogonUserIdentity.Name;
            var CurrentUserName = strUserDetails.Split('\\')[1].Split(' ')[0];
            CurrentUserName = CurrentUserName.Substring(0, 1).ToUpper() + CurrentUserName.Substring(1);
            ViewBag.UserName = CurrentUserName;

            using (var db = new CallCentreTrackerEntities1())
            {
                var userRoles = from u in db.aspnet_Users
                                join ur in db.vw_aspnet_UsersInRoles on u.UserId equals ur.UserId
                                join r in db.aspnet_Roles on ur.RoleId equals r.RoleId
                                where u.UserName.ToLower() == CurrentUserName.ToLower()
                                select r.RoleName;

                if (userRoles!=null)
                {
                    return RedirectToAction("Index", "Cx");
                }
            }

            return RedirectToAction("Index", "Issues");
        }



    }
}