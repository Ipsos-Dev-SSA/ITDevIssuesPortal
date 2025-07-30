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

            using (var db = new CallCentreTrackerEntities1()) // Your EF DbContext
            {
                var respondent = db.tblRespondents
                    .Where(r => r.RespondentID != 0 &&
                                !string.IsNullOrEmpty(r.PhaseStatus) &&
                                !string.IsNullOrEmpty(r.PhaseOutcome) &&
                                !string.IsNullOrEmpty(r.interviewStatus) &&
                                !string.IsNullOrEmpty(r.InterviewOutcome))
                    .Select(r => new
                    {
                        r.PhaseStatus,
                        r.PhaseOutcome,
                        r.interviewStatus,
                        r.InterviewOutcome
                    })
                    .FirstOrDefault();

                if (respondent != null)
                {
                    ViewBag.PhaseStatus = respondent.PhaseStatus;
                    ViewBag.PhaseOutcome = respondent.PhaseOutcome;
                    ViewBag.InterviewStatus = respondent.interviewStatus;
                    ViewBag.InterviewOutcome = respondent.InterviewOutcome;
                }
                else
                {
                    ViewBag.PhaseStatus = "N/A";
                    ViewBag.PhaseOutcome = "N/A";
                    ViewBag.InterviewStatus = "N/A";
                    ViewBag.InterviewOutcome = "N/A";
                }
            }




            List<RespDataBase> _respondentBase = new List<RespDataBase>();

            var respondents = new List<tblRespondent>();
            //var respondents = db.tblRespondents.ToList<tblRespondent>();
            if (role == "SupervisorRole")
            {
                //respondents = db.tblRespondents.ToList();
                var loggedInUser = User.Identity.Name; // Gets the logged-in user's domain\username
                var userCountryID = db.aspnet_Users
                    .Where(u => u.UserName == loggedInUser)
                    .Select(u => u.CountryID)
                    .FirstOrDefault();

                respondents = db.tblRespondents
                    .Where(x => x.StatusID != 13 && x.CountryID != null && x.CountryID == userCountryID)
                    .ToList();



                foreach (var r in respondents)
                {
                    RespDataBase _r = new RespDataBase();
                    _r.AddedBy = r.AddedBy;
                    _r.CheckIn1 = Convert.ToString(r.CheckIn1);
                    _r.CheckIn1Date = r.CheckIn1Date.HasValue ? r.CheckIn1Date.Value.ToString("dd-MM-yyyy") : "";
                    _r.CheckIn2 = Convert.ToString(r.CheckIn2);
                    _r.CheckIn2Date = r.CheckIn2Date.HasValue ? r.CheckIn2Date.Value.ToString("dd-MM-yyyy") : "";
                    _r.CheckIn3 = Convert.ToString(r.CheckIn3);
                    _r.CheckIn3Date = r.CheckIn3Date.HasValue ? r.CheckIn3Date.Value.ToString("dd-MM-yyyy") : "";
                    _r.CheckIn4 = Convert.ToString(r.CheckIn4);
                    _r.CheckIn4Date = r.CheckIn4Date.HasValue ? r.CheckIn4Date.Value.ToString("dd-MM-yyyy") : "";
                    _r.CheckInDate = r.CheckInDate.HasValue ? r.CheckInDate.Value.ToString("dd-MM-yyyy") : "";
                    _r.DateAdded = r.DateAdded.HasValue ? r.DateAdded.Value.ToString("dd-MM-yyyy") : "";
                    _r.EDD_Latest = r.EDD_Latest;
                    _r.InstanceID = Convert.ToString(r.InstanceID);
                    _r.PhoneNumber = r.PhoneNumber;
                    _r.Q1 = r.Q1.HasValue ? r.Q1.Value.ToString("dd-MM-yyyy") : "";
                    _r.Q2 = r.Q2;
                    _r.Q202_EDD = r.Q202_EDD.HasValue ? r.Q202_EDD.Value.ToString("dd-MM-yyyy") : "";
                    _r.Q3 = r.Q3;
                    _r.Status = (db.lutStatus.Where(x => x.StatusID == r.StatusID).Select(x => x.Status).FirstOrDefault());
                    _r.Q607_C_1 = r.Q607_C_1;
                    _r.Q608a_q608_C_1 = r.Q608a_q608_C_1;
                    _r.Q609a_q609_C_1 = r.Q609a_q609_C_1;
                    _r.Q610a_q610_C_1 = r.Q610a_q610_C_1;
                    _r.Q610_ = r.Q610_;
                    _r.Q611a_q611_C_1 = r.Q611a_q611_C_1;
                    _r.Q612_Phone_C_1 = r.Q612_Phone_C_1;
                    _r.ResonseId = Convert.ToString(r.ResonseId);
                    _r.RESPID = Convert.ToString(r.RESPID);
                    _r.RespondentID = r.RespondentID;
                    _r.Resp_FirstName = r.Resp_FirstName;
                    _r.Resp_Oth = r.Resp_Oth;
                    _r.Resp_Surname = r.Resp_Surname;
                    _r.Name = (db.tblAgentActivities.Where(x => x.RespondentID == r.RespondentID).OrderByDescending(x => x.DateAdded).Select(x => x.tblAgent.Name).FirstOrDefault());
                    _r.StatusID = Convert.ToString(r.StatusID);
                    _r.TimeZone = Convert.ToString(r.TimeZone);
                    _r.CheckInLevel = r.CheckInLevel == null ? "1st Call Attempt Pending" : r.CheckInLevel;
                    _r.PhysicalCheckInLevel = r.VisitLevel == null ? "" : r.VisitLevel;



                    _respondentBase.Add(_r);
                }
            }
            else if (role == "Developer")
            {


                var loggedInUser = User.Identity.Name; // Gets the logged-in user's domain\username
                var userCountryID = db.aspnet_Users
                    .Where(u => u.UserName == loggedInUser)
                    .Select(u => u.CountryID)
                    .FirstOrDefault();

                // Ensure userCountryID is not null before filtering
                if (userCountryID != null)
                {
                    // Filter respondents based on CountryID
                    respondents = (from l in db.tblAgentActivities
                                   where l.AgentID == agentid
                                         && l.tblRespondent.StatusID != 13
                                         && l.tblRespondent.CountryID == userCountryID
                                   orderby l.DateAdded descending
                                   select l.tblRespondent)
                                  .Distinct()
                                  .ToList();
                }
                else
                {
                    respondents = new List<tblRespondent>(); // Return an empty list if userCountryID is null
                }

                foreach (var r in respondents)
                {
                    RespDataBase _r = new RespDataBase();
                    _r.AddedBy = r.AddedBy;
                    _r.CheckIn1 = Convert.ToString(r.CheckIn1);
                    _r.CheckIn1Date = r.CheckIn1Date.HasValue ? r.CheckIn1Date.Value.ToString("dd-MM-yyyy") : "";
                    _r.CheckIn2 = Convert.ToString(r.CheckIn2);
                    _r.CheckIn2Date = r.CheckIn2Date.HasValue ? r.CheckIn2Date.Value.ToString("dd-MM-yyyy") : "";
                    _r.CheckIn3 = Convert.ToString(r.CheckIn3);
                    _r.CheckIn3Date = r.CheckIn3Date.HasValue ? r.CheckIn3Date.Value.ToString("dd-MM-yyyy") : "";
                    _r.CheckIn4 = Convert.ToString(r.CheckIn4);
                    _r.CheckIn4Date = r.CheckIn4Date.HasValue ? r.CheckIn4Date.Value.ToString("dd-MM-yyyy") : "";
                    _r.CheckInDate = r.CheckInDate.HasValue ? r.CheckInDate.Value.ToString("dd-MM-yyyy") : "";
                    _r.DateAdded = r.DateAdded.HasValue ? r.DateAdded.Value.ToString("dd-MM-yyyy HH:mm:ss") : "";
                    _r.InstanceID = Convert.ToString(r.InstanceID);
                    _r.PhoneNumber = r.PhoneNumber;
                    _r.Q1 = r.Q1.HasValue ? r.Q1.Value.ToString("dd-MM-yyyy") : "";
                    _r.Q2 = r.Q2;
                    _r.Q202_EDD = r.Q202_EDD.HasValue ? r.Q202_EDD.Value.ToString("dd-MM-yyyy") : "";
                    _r.Q3 = r.Q3;
                    _r.Status = (db.lutStatus.Where(x => x.StatusID == r.StatusID).Select(x => x.Status).FirstOrDefault());
                    _r.Q607_C_1 = r.Q607_C_1;
                    _r.Q608a_q608_C_1 = r.Q608a_q608_C_1;
                    _r.Q609a_q609_C_1 = r.Q609a_q609_C_1;
                    _r.Q610a_q610_C_1 = r.Q610a_q610_C_1;
                    _r.Q610_ = r.Q610_;
                    _r.Q611a_q611_C_1 = r.Q611a_q611_C_1;
                    _r.Q612_Phone_C_1 = r.Q612_Phone_C_1;
                    _r.ResonseId = Convert.ToString(r.ResonseId);
                    _r.RESPID = Convert.ToString(r.RESPID);
                    _r.RespondentID = r.RespondentID;
                    _r.Resp_FirstName = r.Resp_FirstName;
                    _r.Resp_Oth = r.Resp_Oth;
                    _r.Resp_Surname = r.Resp_Surname;
                    _r.StatusID = Convert.ToString(r.StatusID);
                    _r.TimeZone = Convert.ToString(r.TimeZone);
                    _r.CheckInLevel = r.CheckInLevel == null ? "1st Call Attempt Pending" : r.CheckInLevel;
                    _r.VisitLevel = r.VisitLevel == null ? "1st Physical Visit Pending" : r.VisitLevel;



                    _respondentBase.Add(_r);
                }

            }
            //Pie Chart 


            // Compute Status Counts for Chart.js
            var statusCounts = _respondentBase
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.StatusData = statusCounts;



            ViewBag.role = role;
            ViewBag.agentid = agentid;



            var loggedInUser2 = User.Identity.Name;
            var userCountryID2 = db.aspnet_Users
                .Where(u => u.UserName == loggedInUser2)
                .Select(u => u.CountryID)
                .FirstOrDefault();

            ViewBag.agents = db.tblAgents
            .Where(a => a.CountryID == userCountryID2)
            .ToList();
            ViewBag.history = new List<tblAgentActivity>();

            ViewBag.date = db.tblRespondents.Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.CheckInDate) == DateTime.Today).Count();

            RespModel model = new RespModel();
            model.RespondentList = _respondentBase;
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult UserLogin(string authenticate, string country)
        {

            CallCentreTrackerEntities1 mtn = new CallCentreTrackerEntities1();

            var useremailaddress = (from l in mtn.tblEFSUsers where l.panelist_code == authenticate select l.u_email).FirstOrDefault();
            var userid = (from l in mtn.aspnet_Membership where l.Email == useremailaddress select l.UserId).FirstOrDefault();
            var username = (from l in mtn.aspnet_Users where l.UserId == userid select l.UserName).FirstOrDefault();
            var countrysession = (from l in mtn.LutEfsCountries where l.CountryCode == country select l.Country).FirstOrDefault();

            if (userid.ToString() != "00000000-0000-0000-0000-000000000000")
            {

                //  FormsAuthentication.SetAuthCookie(username, true);
                // Set values in session
                Session["PanelistCode"] = authenticate;
                Session["CountrySession"] = country;
                return Redirect("~/Home/SignIn2/?username=" + username+"&country="+country);
            }
            else
                //   return Redirect("http://client.ipsos.co.za/MTNVerificationPortal");
                return RedirectToAction("NotAuthorized", "Home");
        }
        public ActionResult ViewRespondent(int respondentId)
        {
            using (var db = new CallCentreTrackerEntities1()) // Your EF DbContext
            {
                var respondent = db.tblRespondents
                    .Where(r => r.RespondentID == respondentId)
                    .Select(r => new
                    {
                        r.PhaseStatus,
                        r.PhaseOutcome,
                        r.interviewStatus,
                        r.InterviewOutcome
                    })
                    .FirstOrDefault();

                if (respondent != null)
                {
                    ViewBag.PhaseStatus = respondent.PhaseStatus;
                    ViewBag.PhaseOutcome = respondent.PhaseOutcome;
                    ViewBag.InterviewStatuss = respondent.interviewStatus;
                    ViewBag.InterviewOutcome = respondent.InterviewOutcome;
                }
                else
                {
                    ViewBag.PhaseStatus = "N/A";
                    ViewBag.PhaseOutcome = "N/A";
                    ViewBag.InterviewStatus = "N/A";
                    ViewBag.InterviewOutcome = "N/A";
                }
            }

            try
            {
                var respondent = db.tblRespondents.FirstOrDefault(r => r.RespondentID == respondentId);
                if (respondent == null)
                {
                    return View(new RespDataBase());
                }

                // Retrieve all statuses from the database
                List<lutStatu> statusList = db.lutStatus.ToList();
                ViewBag.status = new SelectList(statusList, "StatusID", "Status", respondent.StatusID);

                //
                var statuses = db.LutPhaseStatus
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Description
                    }).ToList();

                ViewBag.PhaseStatusList = statuses ?? new List<SelectListItem>();

                //

                ViewBag.InterviewStatus = db.LutPhaseStatus
                 .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Description })
                 .ToList();

                ViewBag.PhaseStatusListInterview = db.InterviewOutcomes
                    .Select(x => new SelectListItem { Value = x.GroupID.ToString(), Text = x.Category })
                    .Distinct()
                    .ToList();

                ViewBag.PhaseStatusListInterview = db.InterviewOutcomes
                .GroupBy(x => new { x.GroupID, x.Category }) // Ensure uniqueness
                .Select(g => new SelectListItem { Value = g.Key.GroupID.ToString(), Text = g.Key.Category })
                .ToList();


                ViewBag.history = db.tblAgentActivities
                    .Where(x => x.tblRespondent.RespondentID == respondentId)
                    .OrderByDescending(x => x.DateAdded)
                    .ToList() ?? new List<tblAgentActivity>();




                RespDataBase _r = new RespDataBase
                {
                    AddedBy = respondent.AddedBy ?? "",
                    CheckIn1 = respondent.CheckIn1?.ToString() ?? "0",
                    CheckIn1Date = respondent.CheckIn1Date?.ToString("dd-MM-yyyy") ?? "",
                    CheckIn2 = respondent.CheckIn2?.ToString() ?? "0",
                    CheckIn2Date = respondent.CheckIn2Date?.ToString("dd-MM-yyyy") ?? "",
                    CheckIn3 = respondent.CheckIn3?.ToString() ?? "0",
                    CheckIn3Date = respondent.CheckIn3Date?.ToString("dd-MM-yyyy") ?? "",
                    CheckIn4 = respondent.CheckIn4?.ToString() ?? "0",
                    CheckIn4Date = respondent.CheckIn4Date?.ToString("dd-MM-yyyy") ?? "",
                    CheckIn5Date = respondent.CheckIn5Date?.ToString("dd-MM-yyyy") ?? "",
                    CheckInDate = respondent.CheckInDate?.ToString("dd-MM-yyyy") ?? "",
                    DateAdded = respondent.DateAdded.HasValue ? respondent.DateAdded.Value.ToString("dd-MM-yyyy HH:mm:ss") : "",

                    EDD_Latest = respondent.EDD_Latest,
                    InstanceID = respondent.InstanceID?.ToString() ?? "0",
                    PhoneNumber = respondent.PhoneNumber ?? "",
                    StatusID = respondent.StatusID.ToString(),
                    Status = db.lutStatus.FirstOrDefault(x => x.StatusID == respondent.StatusID)?.Status ?? "Unknown",
                    RespondentID = respondent.RespondentID,
                    Resp_FirstName = respondent.Resp_FirstName ?? "",
                    Resp_Oth = respondent.Resp_Oth ?? "",
                    Resp_Surname = respondent.Resp_Surname ?? "",
                    TimeZone = respondent.TimeZone?.ToString() ?? "Unknown",
                    CheckInLevel = respondent.CheckInLevel == null ? "1st Call Attempt Pending" : respondent.CheckInLevel,
                    VisitLevel = respondent.VisitLevel == null ? "1st Physical Visit Pending" : respondent.VisitLevel,
                    ScreeningLevel = respondent.ScreeningLevel == null ? "Screening Phase Pending" : respondent.ScreeningLevel,
                    InterviewerLevel = respondent.InterviewerLevel == null ? "Interview Phase Pending" : respondent.InterviewerLevel,






                };


                respondent.CheckInLevel = _r.CheckInLevel;



                respondent.VisitLevel = _r.VisitLevel;



                respondent.ScreeningLevel = _r.ScreeningLevel;



                respondent.InterviewerLevel = _r.InterviewerLevel;

                //

                respondent.PhaseStatus = _r.PhaseStatus.ToString();

                respondent.PhaseOutcome = _r.PhaseOutcome.ToString();

                respondent.interviewStatus = _r.interviewStatus.ToString();

                respondent.InterviewOutcome = _r.InterviewOutcome.ToString();

                respondent.DateAdded = _r.DateAdded.AsDateTime();








                // **Check-In Attempts (Calls)**
                if (respondent.CheckIn1 == false)
                {
                    _r.CheckInLevel = respondent.CheckInLevel;
                    _r.CheckIn1Date = DateTime.Now.ToString("dd-MM-yyyy");
                    //respondent.CheckIn1 = true;
                    //respondent.CheckIn1Date = DateTime.Now;
                    //db.SaveChanges();
                    return View(_r);
                }

                if (respondent.CheckIn1 == true && respondent.CheckIn2 == false)
                {
                    _r.CheckInLevel = respondent.CheckInLevel;
                    _r.CheckIn2Date = DateTime.Now.ToString("dd-MM-yyyy");

                }

                if (respondent.CheckIn1 == true && respondent.CheckIn2 == true && respondent.CheckIn3 == false)
                {
                    _r.CheckInLevel = respondent.CheckInLevel;
                    _r.CheckIn3Date = DateTime.Now.ToString("dd-MM-yyyy");

                    return View(_r);
                }

                if (respondent.CheckIn1 == true && respondent.CheckIn2 == true && respondent.CheckIn3 == true && respondent.CheckIn4 == false)
                {
                    _r.CheckInLevel = respondent.CheckInLevel;
                    _r.CheckIn4Date = DateTime.Now.ToString("dd-MM-yyyy");

                    return View(_r);
                }

                if (respondent.CheckIn1 == true && respondent.CheckIn2 == true && respondent.CheckIn3 == true && respondent.CheckIn4 == true && respondent.CheckIn5 == false)
                {
                    _r.CheckInLevel = respondent.CheckInLevel;
                    _r.CheckIn5Date = DateTime.Now.ToString("dd-MM-yyyy");

                    return View(_r);
                }
                else
                { _r.CheckInLevel = respondent.CheckInLevel; }
                _r.CheckInDate = null;
                respondent.CheckInLevel = _r.CheckInLevel;

                if (respondent.PhysicalAttempt1 == false)
                {
                    _r.VisitLevel = respondent.VisitLevel;

                    return View(_r);
                }

                if (respondent.PhysicalAttempt1 == true && respondent.PhysicalAttempt2 == false)
                {
                    _r.VisitLevel = respondent.VisitLevel;

                    return View(_r);
                }

                if (respondent.PhysicalAttempt1 == true && respondent.PhysicalAttempt2 == true && respondent.PhysicalAttempt3 == false)
                {
                    _r.VisitLevel = respondent.VisitLevel;

                    return View(_r);
                }


                _r.CheckInDate = null;

                return View(_r);

            }
            catch (Exception ex)
            {
                RespDataBase _r = new RespDataBase();
                string errorMessage = $"Error: {ex.Message}\nStackTrace: {ex.StackTrace}";
                CommonMethods.SendMail("Jack.Kabinga@ipsos.com", "Jack.Kabinga@ipsos.com", "Respondent Detail Error", errorMessage, false);
                return View(_r);
            }
        }








        [HttpPost]
        public ActionResult AssignAgent(int ddlAgent, int respondentId, string comment)
        {
            var respondent = db.tblRespondents.Where(x => x.RespondentID == respondentId).OrderByDescending(x => x.DateAdded).FirstOrDefault();

            try
            {
                var activity = new tblAgentActivity();
                activity.AgentID = ddlAgent;
                activity.RespondentID = respondentId;
                activity.StatusID = 1;
                activity.Comments = comment;
                //activity.Contacts = comment;
                activity.DateAdded = DateTime.Now;

                db.tblAgentActivities.Add(activity);
                respondent.StatusID = 1;
                db.SaveChanges();
                ViewBag.message = "Assigned successfully";
                ViewBag.alert = "alert-success";

            }
            catch (Exception e)
            {
                ViewBag.message = e.Message;
                ViewBag.alert = "alert-danger";
            }
            ViewBag.role = role;
            ViewBag.agentid = agentid;
            ViewBag.status = db.lutStatus.ToList();
            ViewBag.agents = db.tblAgents.ToList();
            //ViewBag.checkInLevels = db.lutCheckInLevels.ToList();
            var history = db.tblAgentActivities.Where(x => x.RespondentID == respondentId).OrderByDescending(x => x.DateAdded).ToList();
            ViewBag.history = history != null ? history : new List<tblAgentActivity>();
            var _r = formatRespondent(respondent);
            return View("ViewRespondent", _r);
        }

        [HttpPost]
        public ActionResult SelectRespondent(int respondentID, bool selected)
        {
            return View();
        }
        [HttpPost]


        public ActionResult Index(RespModel model, int[] respondentIDs)
        {
            if (respondentIDs == null || respondentIDs.Length == 0)
            {
                ViewBag.message = "No respondents selected.";
                ViewBag.alert = "alert-warning";
                return View(model);
            }

            for (int i = 0; i < respondentIDs.Length; i++)
            {
                var id = respondentIDs[i];
                var r = db.tblRespondents.Where(x => x.RespondentID == id)
                                         .OrderByDescending(x => x.DateAdded)
                                         .FirstOrDefault();

                if (r == null)
                    continue; // Skip if respondent not found

                try
                {
                    var activity = new tblAgentActivity
                    {
                        AgentID = model.agentId,
                        RespondentID = r.RespondentID,
                        StatusID = r.StatusID,
                        DateAdded = DateTime.Now
                    };

                    db.tblAgentActivities.Add(activity);
                    db.SaveChanges();

                    ViewBag.message = "Assigned successfully";
                    ViewBag.alert = "alert-success";
                }
                catch (Exception e)
                {
                    ViewBag.message = e.Message;
                    ViewBag.alert = "alert-danger";
                }
            }

            ViewBag.status = db.lutStatus.ToList();
            ViewBag.agents = db.tblAgents.ToList();

            List<RespDataBase> list = new List<RespDataBase>();
            var respondents = db.tblRespondents.Where(x => x.StatusID != 13).ToList();
            foreach (var _r in respondents)
            {
                var respondent = formatRespondent(_r);
                list.Add(respondent);
            }
            model.RespondentList = list;
            return View(model);
        }

        public ActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult SignIn(string username, string password)
        {
            using (var supervisorDb = new CallCentreTrackerEntities1())
            {
                // Get user
                var user = supervisorDb.vw_aspnet_Users
                    .FirstOrDefault(u => u.UserName == username);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Login", "Home");
                }

                // Get user membership info
                var membership = supervisorDb.aspnet_Membership
                    .FirstOrDefault(m => m.UserId == user.UserId);

                if (membership == null)
                {
                    TempData["ErrorMessage"] = "Membership information not found.";
                    return RedirectToAction("Login", "Home");
                }

                // Simple plaintext password check (assuming passwords are not hashed)
                if (membership.Password != password)
                {
                    TempData["ErrorMessage"] = "Incorrect password.";
                    return RedirectToAction("Login", "Home");
                }

                // Get user role
                var userRoleId = supervisorDb.vw_aspnet_UsersInRoles
                    .Where(ur => ur.UserId == user.UserId)
                    .Select(ur => ur.RoleId)
                    .FirstOrDefault();

                var userRole = supervisorDb.aspnet_Roles
                    .Where(r => r.RoleId == userRoleId)
                    .Select(r => r.RoleName)
                    .FirstOrDefault();

                // Optional: Get agent ID from your main DB if needed
                agentid = db.tblAgents
                    .Where(a => a.UserName == username)
                    .Select(a => a.AgentID)
                    .FirstOrDefault();

                role = userRole;

                Session["Role"] = role;
                FormsAuthentication.SetAuthCookie(username, false);

                // Redirect based on known roles
                if (role == "Supervisor Role")
                {
                    return RedirectToAction("LandingPage", "Home");
                }
                if (role == "CX")
                {
                    return RedirectToAction("Index", "CX");
                }
                if (role == "Developer")
                {
                    return RedirectToAction("Assigned", "Developer");
                }
                if (role == "Client")
                {
                    return RedirectToAction("Review", "Client");
                }



             // ✅ Check if role exists in aspnet_Roles table
              var allowedRoles = supervisorDb.aspnet_Roles
            .Select(r => r.RoleName)
            .ToList();

                if (allowedRoles.Contains(role))
                {
                    return RedirectToAction("Index", "Issues");
                }

                // Fallback
                TempData["ErrorMessage"] = "User role not recognized.";
                return RedirectToAction("Login", "Home");
            }
        }




        public ActionResult SignIn2(string username, int country)
        {
            using (var supervisorDb = new CallCentreTrackerEntities1())
            {
                // Get user
                var user = supervisorDb.vw_aspnet_Users
                    .FirstOrDefault(u => u.UserName == username);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Login", "Home");
                }


                // Get user membership info
                var membership = supervisorDb.aspnet_Membership
                    .FirstOrDefault(m => m.UserId == user.UserId);

                if (membership == null)
                {
                    TempData["ErrorMessage"] = "Membership information not found.";
                    return RedirectToAction("Login", "Home");
                }

                // Get user role
                var userRoleId = supervisorDb.vw_aspnet_UsersInRoles
                    .Where(ur => ur.UserId == user.UserId)
                    .Select(ur => ur.RoleId)
                    .FirstOrDefault();

                var userRole = supervisorDb.aspnet_Roles
                    .Where(r => r.RoleId == userRoleId)
                    .Select(r => r.RoleName)
                    .FirstOrDefault();

                // Get agent ID and country ID
                var agentData = db.aspnet_Users
                    .Where(a => a.UserName == username)
                    .Select(a => new { a.UserId, a.CountryID })
                    .FirstOrDefault();

                if (agentData == null)
                {
                    TempData["ErrorMessage"] = "Agent information not found.";
                    return RedirectToAction("Login", "Home");
                }

               
                var userCountryId = agentData.CountryID;

                role = userRole;
                Session["Role"] = role;
                FormsAuthentication.SetAuthCookie(username, false);

                // Redirects based on fixed roles
                if (role == "Supervisor Role")
                {
                    return RedirectToAction("LandingPage", "Home");
                }
                if (role == "CX")
                {
                    return RedirectToAction("Index", "CX");
                }
                // Get allowed country IDs from lutEfsCountry
                var allowedCountryIds = db.LutEfsCountries
                    .Select(c => c.Id)
                    .ToList();
                if (role == "Developer" && role.Equals(allowedCountryIds))
                {
                    return RedirectToAction("Assigned", "Developer");
                }
                if (role == "Client")
                {
                    return RedirectToAction("Review", "Client");
                }

             

                if (userCountryId != null && allowedCountryIds.Contains(country))
                {
                    return RedirectToAction("Index", "Issues");
                }

                // Fallback
                TempData["ErrorMessage"] = "User country not recognized or access denied.";
                return RedirectToAction("Login", "Home");
            }
        }


        public ActionResult LandingPage()
        {
            var username = User.Identity.Name;

            var user = db.aspnet_Users.FirstOrDefault(u => u.UserName == username);
            if (user == null)
            {
                return HttpNotFound("User not found.");
            }

            // Get Role Name by joining user roles
            var roleName = (from ur in db.vw_aspnet_UsersInRoles
                            join r in db.aspnet_Roles on ur.RoleId equals r.RoleId
                            where ur.UserId == user.UserId
                            select r.RoleName).FirstOrDefault();

            ViewBag.RoleName = roleName;

            return View();
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Home");
        }
        public ActionResult UpdateCallAttemptActivity(int? ddlStatus, int respondentId, string comment, DateTime? EddLatest, string checkinLevel, string contact)
        {
            if (ddlStatus == null)
            {
                ViewBag.message = "Error: Status must be selected.";
                ViewBag.alert = "alert-danger";
                return View("Error");
            }

            var respondent = db.tblRespondents.Where(x => x.RespondentID == respondentId).FirstOrDefault();
            try
            {
                if (respondent.CheckIn1 == false)
                {

                    respondent.CheckIn1 = true;
                    respondent.CheckIn1Date = DateTime.Now;
                    respondent.CheckIn1StatusId = ddlStatus;
                    if (ddlStatus == 4)
                        respondent.CheckInLevel = "Call Attempts Completed";
                    else if (ddlStatus == 2)
                        respondent.CheckInLevel = "Call Denied";
                    else
                        respondent.CheckInLevel = "2nd Call Attempt Pending";

                }
                else if (respondent.CheckIn1 == true && respondent.CheckIn2 == false)
                {
                    // respondent.CheckInLevel = "2nd  Call Attempt Pending";
                    respondent.CheckIn2 = true;
                    respondent.CheckIn2Date = DateTime.Now;
                    respondent.CheckIn2StatusId = ddlStatus;
                    if (ddlStatus == 4)
                        respondent.CheckInLevel = "Call Attempts Completed";
                    else if (ddlStatus == 2)
                        respondent.CheckInLevel = "Call Denied";
                    else
                        respondent.CheckInLevel = "3rd Call Attempt Pending";

                }
                else if (respondent.CheckIn1 == true && respondent.CheckIn2 == true && respondent.CheckIn3 == false)
                {

                    respondent.CheckIn3 = true;
                    respondent.CheckIn3Date = DateTime.Now;
                    respondent.CheckIn3StatusId = ddlStatus;
                    if (ddlStatus == 4)
                        respondent.CheckInLevel = "Call Attempts Completed";
                    else if (ddlStatus == 2)
                        respondent.CheckInLevel = "Call Denied";
                    else
                        respondent.CheckInLevel = "4th Call Attempt Pending";
                }
                else if (respondent.CheckIn1 == true && respondent.CheckIn2 == true && respondent.CheckIn3 == true && respondent.CheckIn4 == false)
                {
                    // respondent.CheckInLevel = "4th Call Attempt Pending";
                    respondent.CheckIn4 = true;
                    respondent.CheckIn4Date = DateTime.Now;
                    respondent.CheckIn4StatusId = ddlStatus;
                    if (ddlStatus == 4)
                        respondent.CheckInLevel = "Call Attempts Completed";
                    else if (ddlStatus == 2)
                        respondent.CheckInLevel = "Call Denied";
                    else
                        respondent.CheckInLevel = "5th Call Attempt Pending";

                }
                else if (respondent.CheckIn1 == true && respondent.CheckIn2 == true && respondent.CheckIn3 == true && respondent.CheckIn4 == true && respondent.CheckIn5 == false)
                {
                    respondent.CheckInLevel = "Call Attempts Completed";
                    respondent.CheckIn5 = true;
                    respondent.CheckIn5Date = DateTime.Now;
                    respondent.CheckIn5StatusId = ddlStatus;


                }
                else
                {
                    respondent.CheckInLevel = "Call Attempts Completed";
                    respondent.CheckInDate = DateTime.Now;
                }


                var activity = new tblAgentActivity();
                activity.AgentID = agentid;
                activity.RespondentID = respondentId;
                activity.Phase = "Call Attempt";
                activity.StatusID = ddlStatus.Value;
                activity.Comments = comment;
                activity.Contacts = contact;
                activity.DateAdded = DateTime.Now;

                db.tblAgentActivities.Add(activity);
                respondent.StatusID = ddlStatus.Value;
                //  respondent.CheckInLevel = checkinLevel;

                db.SaveChanges();
                ViewBag.message = "Updated successfully";
                ViewBag.alert = "alert-success";
            }
            catch (Exception e)
            {
                ViewBag.message = e.Message;
                ViewBag.alert = "alert-danger";
                CommonMethods.SendMail("Jack.Kabinga@ipsos.com", "Jack.Kabinga@ipsos.com", "Test Error", e.Message.ToString(), false);

            }
            List<lutStatu> statusList = db.lutStatus.ToList();
            ViewBag.status = new SelectList(statusList, "StatusID", "Status", respondent.StatusID);

            ViewBag.agents = db.tblAgents.ToList();
            ViewBag.CheckInLevel = respondent.CheckInLevel;
            ViewBag.role = role;
            ViewBag.agentid = agentid;
            // ViewBag.status = db.lutStatus.ToList();
            ViewBag.history = db.tblAgentActivities.Where(x => x.RespondentID == respondentId).OrderByDescending(x => x.DateAdded).ToList();

            var _r = formatRespondent(respondent);
            return Redirect("~/Home/ViewRespondent/?respondentId=" + respondentId);
            //return View("ViewRespondent", _r);
        }
        private RespDataBase formatRespondent(tblRespondent respondent)
        {
            RespDataBase _r = new RespDataBase();
            _r.AddedBy = respondent.AddedBy;
            _r.CheckIn1 = Convert.ToString(respondent.CheckIn1);
            _r.CheckIn1Date = respondent.CheckIn1Date.HasValue ? respondent.CheckIn1Date.Value.ToString("dd-MM-yyyy") : "";
            _r.CheckIn2 = Convert.ToString(respondent.CheckIn2);
            _r.CheckIn2Date = respondent.CheckIn2Date.HasValue ? respondent.CheckIn2Date.Value.ToString("dd-MM-yyyy") : "";
            _r.CheckIn3 = Convert.ToString(respondent.CheckIn3);
            _r.CheckIn3Date = respondent.CheckIn3Date.HasValue ? respondent.CheckIn3Date.Value.ToString("dd-MM-yyyy") : "";
            _r.CheckIn4 = Convert.ToString(respondent.CheckIn4);
            _r.CheckIn4Date = respondent.CheckIn4Date.HasValue ? respondent.CheckIn4Date.Value.ToString("dd-MM-yyyy") : "";
            _r.CheckInDate = respondent.CheckInDate.HasValue ? respondent.CheckInDate.Value.ToString("dd-MM-yyyy") : "";
            _r.DateAdded = respondent.DateAdded.HasValue ? respondent.DateAdded.Value.ToString("dd-MM-yyyy HH:mm:ss") : "";

            _r.EDD_Latest = respondent.EDD_Latest;
            _r.InstanceID = Convert.ToString(respondent.InstanceID);
            _r.PhoneNumber = respondent.PhoneNumber;
            _r.Q1 = respondent.Q1.HasValue ? respondent.Q1.Value.ToString("dd-MM-yyyy") : "";
            _r.Q2 = respondent.Q2;
            _r.Q202_EDD = respondent.Q202_EDD.HasValue ? respondent.Q202_EDD.Value.ToString("dd-MM-yyyy") : "";
            _r.Q3 = respondent.Q3;
            _r.Status = (db.lutStatus.Where(x => x.StatusID == respondent.StatusID).Select(x => x.Status).FirstOrDefault());
            _r.Q607_C_1 = respondent.Q607_C_1;
            _r.Q608a_q608_C_1 = respondent.Q608a_q608_C_1;
            _r.Q609a_q609_C_1 = respondent.Q609a_q609_C_1;
            _r.Q610a_q610_C_1 = respondent.Q610a_q610_C_1;
            _r.Q610_ = respondent.Q610_;
            _r.Q611a_q611_C_1 = respondent.Q611a_q611_C_1;
            _r.Q612_Phone_C_1 = respondent.Q612_Phone_C_1;
            _r.ResonseId = Convert.ToString(respondent.ResonseId);
            _r.RESPID = Convert.ToString(respondent.RESPID);
            _r.RespondentID = respondent.RespondentID;
            _r.Resp_FirstName = respondent.Resp_FirstName;
            _r.Resp_Oth = respondent.Resp_Oth;
            _r.Resp_Surname = respondent.Resp_Surname;
            //_r.StatusID = respondent.lutStatu.Status;
            //_r.Name = respondent..Status;
            _r.Name = (db.tblAgentActivities.Where(x => x.RespondentID == respondent.RespondentID).OrderByDescending(x => x.DateAdded).Select(x => x.tblAgent.Name).FirstOrDefault());
            _r.TimeZone = Convert.ToString(respondent.TimeZone);



            if (respondent.CheckIn1 == false)
            {
                _r.CheckInLevel = "1st Check in Pending";
                _r.CheckIn1Date = Convert.ToString((respondent.EDD_Latest ?? DateTime.Now).AddDays(7));


                //_r.CheckInDate = r.CheckIn1Date.HasValue ? r.CheckIn1Date.Value.ToString("dd-MM-yyyy") : "";

            }
            else if (respondent.CheckIn1 == true && respondent.CheckIn2 == false)
            {
                _r.CheckInLevel = "2nd Check in Pending";
                _r.CheckIn2Date = DateTime.Now.ToString();
                //_r.CheckInDate = r.CheckIn2Date.HasValue ? r.CheckIn2Date.Value.ToString("dd-MM-yyyy") : "";

            }
            else if (respondent.CheckIn2 == true && respondent.CheckIn3 == false)
            {
                _r.CheckInLevel = "3rd Check in Pending";
                _r.CheckIn3Date = Convert.ToString(respondent.CheckIn2Date.Value.AddDays(7));
                //_r.CheckInDate = r.CheckIn3Date.HasValue ? r.CheckIn3Date.Value.ToString("dd-MM-yyyy") : "";

            }
            else if (respondent.CheckIn3 == true && respondent.CheckIn4 == false)
            {
                _r.CheckInLevel = "4th Check in Pending";
                _r.CheckIn4Date = Convert.ToString(respondent.CheckIn3Date.Value.AddDays(7));
                //_r.CheckInDate = r.CheckIn4Date.HasValue ? r.CheckIn4Date.Value.ToString("dd-MM-yyyy") : "";

            }
            else if (respondent.CheckIn4 == true && respondent.CheckIn4 == false)
            {
                _r.CheckInLevel = "5th Check in Pending";
                _r.CheckIn5Date = Convert.ToString(respondent.CheckIn4Date.Value.AddDays(7));
                //_r.CheckInDate = r.CheckIn4Date.HasValue ? r.CheckIn4Date.Value.ToString("dd-MM-yyyy") : "";

            }

            else
            {
                _r.CheckInLevel = "Call Attempts Completed";
                _r.CheckInDate = null;
            }


            return _r;
        }


        public ActionResult UpdatePhysicalAttemptActivity(int? ddlStatus, int respondentId, string comment, DateTime? EddLatest, string VisitLevel, string contact)
        {
            if (ddlStatus == null)
            {
                ViewBag.message = "Error: Status must be selected.";
                ViewBag.alert = "alert-danger";
                return View("Error");
            }

            var respondent = db.tblRespondents.Where(x => x.RespondentID == respondentId).FirstOrDefault();

            if (respondent == null)
            {
                ViewBag.message = "Error: Respondent not found.";
                ViewBag.alert = "alert-danger";
                return View("Error");
            }

            // Updating Physical Attempt Status
            if (respondent.PhysicalAttempt1.Equals(false))
            {
                // respondent.VisitLevel = "1st Physical Visit Pending";
                respondent.PhysicalAttempt1 = true;
                respondent.PhysicalAttempt1Date = DateTime.Now;
                respondent.PhysicalAttemptStatusId1 = ddlStatus;

                if (ddlStatus == 4)
                    respondent.VisitLevel = "Physical Visits Completed";
                else if (ddlStatus == 2)
                    respondent.VisitLevel = "Call Denied";
                else
                    respondent.VisitLevel = "2nd Physical Visit Pending";
            }
            else if (respondent.PhysicalAttempt1.Equals(true) && respondent.PhysicalAttempt2.Equals(false))
            {
                respondent.PhysicalAttempt2 = true;
                respondent.PhysicalAttempt2Date = DateTime.Now;
                respondent.PhysicalAttemptStatusId2 = ddlStatus;

                if (ddlStatus == 4)
                    respondent.VisitLevel = "Physical Visits Completed";
                else if (ddlStatus == 2)
                    respondent.VisitLevel = "Call Denied";
                else
                    respondent.VisitLevel = "3rd Physical Visit Pending";
            }
            else if (respondent.PhysicalAttempt1.Equals(true) && respondent.PhysicalAttempt2.Equals(true) && respondent.PhysicalAttempt3.Equals(false))
            {
                respondent.VisitLevel = "Physical Visits Completed";
                respondent.PhysicalAttempt3 = true;
                respondent.PhysicalAttempt3Date = DateTime.Now;
                respondent.PhysicalAttemptStatusId3 = ddlStatus;
            }
            else
            {
                respondent.VisitLevel = "Physical Visits Completed";
            }

            ViewBag.VisitLevel = respondent.VisitLevel;
            db.Entry(respondent).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            // Update Status
            respondent.StatusID = ddlStatus.Value;
            db.Entry(respondent).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            try
            {
                // Logging the update
                var activity = new tblAgentActivity
                {
                    AgentID = agentid,
                    RespondentID = respondentId,
                    StatusID = ddlStatus.Value,
                    Phase = "Physical Visits",
                    Comments = comment,
                    Contacts = contact,
                    DateAdded = DateTime.Now
                };

                db.tblAgentActivities.Add(activity);
                db.SaveChanges();

                ViewBag.message = "Updated successfully";
                ViewBag.alert = "alert-success";
            }
            catch (Exception e)
            {
                ViewBag.message = e.Message;
                ViewBag.alert = "alert-danger";
            }

            // Reloading updated data
            ViewBag.agents = db.tblAgents.ToList();
            ViewBag.VisitLevel = respondent.VisitLevel;
            ViewBag.role = role;
            ViewBag.agentid = agentid;
            ViewBag.status = db.lutStatus.ToList();
            ViewBag.history = db.tblAgentActivities.Where(x => x.RespondentID == respondentId).OrderByDescending(x => x.DateAdded).ToList();

            var _r = formatRespondentPhysical(respondent);
            return Redirect("~/Home/ViewRespondent/?respondentId=" + respondentId);
        }

        private RespDataBase formatRespondentPhysical(tblRespondent respondent)
        {
            RespDataBase _r = new RespDataBase
            {
                AddedBy = respondent.AddedBy,
                PhysicalAttempt1 = Convert.ToString(respondent.PhysicalAttempt1),
                CheckIn1Date = respondent.CheckIn1Date?.ToString("dd-MM-yyyy") ?? "",
                PhysicalAttempt2 = Convert.ToString(respondent.PhysicalAttempt2),
                CheckIn2Date = respondent.CheckIn2Date?.ToString("dd-MM-yyyy") ?? "",
                PhysicalAttempt3 = Convert.ToString(respondent.PhysicalAttempt3),
                CheckIn3Date = respondent.CheckIn3Date?.ToString("dd-MM-yyyy") ?? "",
                CheckInDate = respondent.CheckInDate?.ToString("dd-MM-yyyy") ?? "",
                DateAdded = respondent.DateAdded?.ToString("dd-MM-yyyy") ?? "",
                EDD_Latest = respondent.EDD_Latest,
                InstanceID = Convert.ToString(respondent.InstanceID),
                PhoneNumber = respondent.PhoneNumber,
                Q1 = respondent.Q1?.ToString("dd-MM-yyyy") ?? "",
                Q2 = respondent.Q2,
                Q202_EDD = respondent.Q202_EDD?.ToString("dd-MM-yyyy") ?? "",
                Q3 = respondent.Q3,
                Status = db.lutStatus.Where(x => x.StatusID == respondent.StatusID).Select(x => x.Status).FirstOrDefault(),
                Q607_C_1 = respondent.Q607_C_1,
                Q608a_q608_C_1 = respondent.Q608a_q608_C_1,
                Q609a_q609_C_1 = respondent.Q609a_q609_C_1,
                Q610a_q610_C_1 = respondent.Q610a_q610_C_1,
                Q610_ = respondent.Q610_,
                Q611a_q611_C_1 = respondent.Q611a_q611_C_1,
                Q612_Phone_C_1 = respondent.Q612_Phone_C_1,
                ResonseId = Convert.ToString(respondent.ResonseId),
                RESPID = Convert.ToString(respondent.RESPID),
                RespondentID = respondent.RespondentID,
                Resp_FirstName = respondent.Resp_FirstName,
                Resp_Oth = respondent.Resp_Oth,
                Resp_Surname = respondent.Resp_Surname,
                Name = db.tblAgentActivities.Where(x => x.RespondentID == respondent.RespondentID).OrderByDescending(x => x.DateAdded)
                        .Select(x => x.tblAgent.Name).FirstOrDefault(),
                TimeZone = Convert.ToString(respondent.TimeZone),
                VisitLevel = respondent.VisitLevel // Correctly assigning the updated VisitLevel
            };


            if (respondent.PhysicalAttempt1 == false) // Check if the first attempt exists
            {
                _r.VisitLevel = "1st Visit Pending";
                _r.CheckIn1Date = Convert.ToString((respondent.EDD_Latest ?? DateTime.Now).AddDays(7));
                respondent.PhysicalAttempt1 = true;
            }
            else if (respondent.PhysicalAttempt1 == true && respondent.PhysicalAttempt2 == false) // Second attempt pending
            {
                _r.VisitLevel = "2nd Visit Pending";
                _r.CheckIn2Date = Convert.ToString(respondent.CheckIn1Date?.AddDays(7) ?? DateTime.Now.AddDays(7));
                respondent.PhysicalAttempt1 = true;
            }
            else if (respondent.PhysicalAttempt2 == true && respondent.PhysicalAttempt3 == false) // Third attempt pending
            {
                _r.VisitLevel = "3rd Visit Pending";
                _r.CheckIn3Date = Convert.ToString(respondent.CheckIn2Date?.AddDays(7) ?? DateTime.Now.AddDays(7));
            }



            else
            {
                _r.VisitLevel = "Visit Attempts Completed";
                _r.CheckInDate = null;
            }


            return _r;
        }

        public ActionResult UpdateScreeningPhase(RespDataBase model, int? ScreeningID, int respondentId, string comment, DateTime? EddLatest, string VisitLevel, string contact)
        {
            if (ScreeningID == null)
            {
                TempData["message"] = "Error: Status must be selected.";
                TempData["alert"] = "alert-danger";
                return RedirectToAction("ViewRespondent", new { respondentId });
            }

            var respondent = db.tblRespondents.FirstOrDefault(x => x.RespondentID == model.RespondentID);

            if (respondent == null)
            {
                TempData["message"] = "Error: Respondent not found.";
                TempData["alert"] = "alert-danger";
                return RedirectToAction("ViewRespondent", new { respondentId });
            }

            // Fetch the text values for PhaseStatus and PhaseOutcome
            var phaseStatusText = db.LutPhaseStatus
                                    .Where(x => x.Id == model.PhaseStatus)
                                    .Select(x => x.Description)
                                    .FirstOrDefault();

            var phaseOutcomeText = db.LutPhases
                                     .Where(x => x.Id == model.PhaseOutcome)
                                     .Select(x => x.Description)
                                     .FirstOrDefault();

            // Ensure retrieved values are not null
            if (string.IsNullOrEmpty(phaseStatusText) || string.IsNullOrEmpty(phaseOutcomeText))
            {
                TempData["message"] = "Error: Invalid Phase Status or Outcome.";
                TempData["alert"] = "alert-danger";
                return RedirectToAction("ViewRespondent", new { respondentId });
            }

            // Save text values instead of IDs
            respondent.PhaseStatus = phaseStatusText;   // Store as string
            respondent.PhaseOutcome = phaseOutcomeText; // Store as string

            if (phaseStatusText == "Eligible") // Adjust this check based on actual text values
                respondent.ScreeningLevel = "Screening Phase Completed";
            else
                respondent.ScreeningLevel = "Screening Phase Pending";

            // Update respondent screening details
            respondent.ScreeningID = ScreeningID.Value;
            respondent.ScreeningDate = DateTime.Now;

            // Mark PhaseOutcome as modified explicitly
            db.Entry(respondent).Property(x => x.PhaseOutcome).IsModified = true;

            try
            {
                db.SaveChanges();

                // Logging the update in tblAgentActivity
                var activity = new tblAgentActivity
                {
                    AgentID = agentid, // Ensure agentid is properly retrieved
                    RespondentID = respondentId,
                    StatusID = ScreeningID.Value,
                    Phase = "Screening Phase",
                    Comments = comment,
                    Contacts = contact,
                    DateAdded = DateTime.Now
                };

                db.tblAgentActivities.Add(activity);
                db.SaveChanges();

                TempData["message"] = "Updated successfully";
                TempData["alert"] = "alert-success";
            }
            catch (Exception e)
            {
                TempData["message"] = "Error: " + e.Message;
                TempData["alert"] = "alert-danger";
            }

            return Redirect("~/Home/ViewRespondent/?respondentId=" + respondentId);
        }


        public JsonResult GetPhaseOutcomes(int phaseStatusId)
        {
            var outcomes = db.LutPhaseOutcomes
                .Where(po => po.PhaseStatusId == phaseStatusId)
                .Select(po => new
                {
                    Id = po.Id,
                    Description = po.Description
                }).ToList();

            return Json(outcomes, JsonRequestBehavior.AllowGet);
        }


        public ActionResult UpdateInterviewPhase(RespDataBase model, int? InterviewID, int respondentId, string comment, DateTime? EddLatest, string VisitLevel, string contact)
        {
            if (InterviewID == null)
            {
                TempData["message"] = "Error: Status must be selected.";
                TempData["alert"] = "alert-danger";
                return RedirectToAction("ViewRespondent", new { respondentId });
            }

            var respondent = db.tblRespondents.FirstOrDefault(x => x.RespondentID == model.RespondentID);

            if (respondent == null)
            {
                TempData["message"] = "Error: Respondent not found.";
                TempData["alert"] = "alert-danger";
                return RedirectToAction("ViewRespondent", new { respondentId });
            }

            // Fetch the text values for interviewStatus and InterviewOutcome
            var interviewStatusText = db.InterviewOutcomes
                                        .Where(x => x.GroupID == model.interviewStatus)
                                        .Select(x => x.Category)
                                        .FirstOrDefault();

            var interviewOutcomeText = db.InterviewOutcomes
                                         .Where(x => x.ID == model.InterviewOutcome)
                                         .Select(x => x.Description)
                                         .FirstOrDefault();

            // Save text values instead of IDs
            respondent.interviewStatus = interviewStatusText;
            respondent.InterviewOutcome = interviewOutcomeText;

            // Update respondent interview details
            respondent.InterviewID = InterviewID.Value;
            respondent.InterviewDate = DateTime.Now;

            // Set InterviewerLevel based on new text-based values
            if (interviewStatusText == "ELIGIBLE" &&
               (interviewOutcomeText == "Eligible in process to make an appointment" ||
                interviewOutcomeText == "Incomplete effective interview"))
            {
                respondent.InterviewerLevel = "Interview Phase Pending";
            }
            else if (interviewStatusText == "ELIGIBLE")
            {
                respondent.InterviewerLevel = "Interview Phase Completed";
            }
            else
            {
                respondent.InterviewerLevel = "Interview Phase Pending";
            }

            // Mark InterviewOutcome as modified explicitly
            db.Entry(respondent).Property(x => x.InterviewOutcome).IsModified = true;

            try
            {
                db.SaveChanges();

                // Logging the update in tblAgentActivity
                var activity = new tblAgentActivity
                {
                    AgentID = agentid, // Ensure agentid is properly retrieved
                    RespondentID = respondentId,
                    Phase = "Interview Phase",
                    StatusID = model.PhaseStatus,
                    Comments = comment,
                    Contacts = contact,
                    DateAdded = DateTime.Now
                };

                db.tblAgentActivities.Add(activity);
                db.SaveChanges();

                TempData["message"] = "Updated successfully";
                TempData["alert"] = "alert-success";
            }
            catch (Exception e)
            {
                TempData["message"] = "Error: " + e.Message;
                TempData["alert"] = "alert-danger";
            }

            return Redirect("~/Home/ViewRespondent/?respondentId=" + respondentId);
        }


        // JsonResult for dynamically fetching outcomes based on selected GroupID

        public JsonResult GetOutcomesByGroupID(int groupId)
        {
            var outcomes = db.InterviewOutcomes
                .Where(o => o.GroupID == groupId)  // Filter by selected GroupID
                .Select(o => new { Value = o.ID, Text = o.Description }) // Return ID & Description
                .ToList();

            return Json(outcomes, JsonRequestBehavior.AllowGet);
        }


        public ActionResult IndexStatistics()
        {
            var CurrentUserName = GetCurrentUserName();

            // Get respondent details (e.g., Phase Status, Phase Outcome, etc.)
            var respondentDetails = GetRespondentDetails();

            // Create a list to store the respondents
            List<RespDataBase> _respondentBase = new List<RespDataBase>();

            using (var db = new CallCentreTrackerEntities1()) // Your EF DbContext
            {
                // Get respondents based on role (e.g., "SupervisorRole")
                var respondents = GetRespondentsByRole(db, "SupervisorRole");

                // Get the case summary counts using the CaseSummaryService
                var caseSummaryService = new CaseSummaryService(db);  // Initialize the CaseSummaryService
                var caseSummary = caseSummaryService.GetCaseCounts(); // Fetch the case summary counts

                // Assign respondent details and case summary to ViewBag
                ViewBag.CaseSummary = caseSummary;
                ViewBag.RespondentDetails = respondents;

                return View(respondents); // Pass the list of respondents to the view
            }
        }



        // Helper method to get the current user name
        private string GetCurrentUserName()
        {
            var strUserDetails = Request.LogonUserIdentity.Name;
            var CurrentUserName = strUserDetails.Split('\\')[1].Split(' ')[0];
            CurrentUserName = CurrentUserName.Substring(0, 1).ToUpper() + CurrentUserName.Substring(1);
            return CurrentUserName;
        }

        // Helper method to get the respondent details (e.g., Phase Status, Phase Outcome, etc.)
        private dynamic GetRespondentDetails()
        {
            using (var db = new CallCentreTrackerEntities1()) // Your EF DbContext
            {
                var respondent = db.tblRespondents
                    .Where(r => r.RespondentID != 0 &&
                                !string.IsNullOrEmpty(r.PhaseStatus) &&
                                !string.IsNullOrEmpty(r.PhaseOutcome) &&
                                !string.IsNullOrEmpty(r.interviewStatus) &&
                                !string.IsNullOrEmpty(r.InterviewOutcome))
                    .Select(r => new
                    {
                        r.PhaseStatus,
                        r.PhaseOutcome,
                        r.interviewStatus,
                        r.InterviewOutcome
                    })
                    .FirstOrDefault();

                if (respondent != null)
                {
                    return new
                    {
                        PhaseStatus = respondent.PhaseStatus,
                        PhaseOutcome = respondent.PhaseOutcome,
                        interviewStatus = respondent.interviewStatus,
                        InterviewOutcome = respondent.InterviewOutcome
                    };
                }
                else
                {
                    return new
                    {
                        PhaseStatus = "N/A",
                        PhaseOutcome = "N/A",
                        interviewStatus = "N/A",
                        InterviewOutcome = "N/A"
                    };
                }
            }
        }

        // Helper method to get the respondents based on user role (e.g., "SupervisorRole")
        private List<RespondentViewModel> GetRespondentsByRole(CallCentreTrackerEntities1 db, string role)
        {
            var respondents = new List<RespondentViewModel>();

            if (role == "SupervisorRole")
            {
                respondents = db.tblRespondents
                    .Where(r => r.RespondentID != 0)
                    .Select(r => new RespondentViewModel
                    {
                        RespondentID = r.RespondentID,
                        Resp_Surname = r.Resp_Surname,
                        Resp_FirstName = r.Resp_FirstName,
                        Resp_Oth = r.Resp_Oth,
                        PhoneNumber = r.PhoneNumber,
                        Q607_C_1 = r.Q607_C_1,
                        Q608a_q608_C_1 = r.Q608a_q608_C_1,
                        Q609a_q609_C_1 = r.Q609a_q609_C_1,
                        DateAdded = r.DateAdded,
                        AddedBy = r.AddedBy,
                        ScreeningLevel = r.ScreeningLevel,
                        InterviewerLevel = r.InterviewerLevel,
                        StatusID = r.StatusID.ToString(),
                        PhaseStatus = r.PhaseStatus,
                        PhaseOutcome = r.PhaseOutcome,
                        interviewStatus = r.interviewStatus,
                        InterviewOutcome = r.InterviewOutcome,
                    })
                    .ToList();
            }

            return respondents;
        }


        public class CaseSummary
{
    public int CompletedCases { get; set; }
    public int AppointmentCases { get; set; }
    public int RefusalCases { get; set; }
    public int OutOfTargetCases { get; set; }
    public int IneligibleCases { get; set; }
    public int UnobtainableCases { get; set; }

    // Use double for percentages
    public double CompletedCasesPercentage { get; set; }
    public double AppointmentCasesPercentage { get; set; }
    public double RefusalCasesPercentage { get; set; }
    public double OutOfTargetCasesPercentage { get; set; }
    public double IneligibleCasesPercentage { get; set; }
    public double UnobtainableCasesPercentage { get; set; }
}


   public class CaseSummaryService
{
    private readonly CallCentreTrackerEntities1 _context;

    public CaseSummaryService(CallCentreTrackerEntities1 context)
    {
        _context = context;
    }

    public CaseSummary GetCaseCounts()
    {
        var totalRespondents = _context.tblRespondents.Count();

        var completedCases = _context.tblRespondents
            .Where(i => i.interviewStatus == "ELIGIBLE" && i.InterviewOutcome != "NULL" && i.InterviewOutcome != "Eligible in process to make an appointment")
            .Count();

        var appointmentCases = _context.tblRespondents
            .Where(i => i.InterviewOutcome == "Eligible in process to make an appointment")
            .Count();

        var refusalCases = _context.tblRespondents
            .Where(i => i.interviewStatus == "INTERVIEW REFUSAL")
            .Count();

        var outOfTargetCases = _context.tblRespondents
            .Where(i => i.PhaseStatus == "Out of Target")
            .Count();

        var ineligibleCases = _context.tblRespondents
            .Where(i => i.PhaseStatus == "Ineligible")
            .Count();

        var unobtainableCases = _context.tblRespondents
            .Where(i => i.PhaseStatus == "Unobtainable")
            .Count();

        // Calculate percentages
        var completedCasesPercentage = totalRespondents > 0 ? Math.Round((double)completedCases / totalRespondents * 100, 2) : 0;
        var appointmentCasesPercentage = totalRespondents > 0 ? Math.Round((double)appointmentCases / totalRespondents * 100, 2) : 0;
        var refusalCasesPercentage = totalRespondents > 0 ? Math.Round((double)refusalCases / totalRespondents * 100, 2) : 0;
        var outOfTargetCasesPercentage = totalRespondents > 0 ? Math.Round((double)outOfTargetCases / totalRespondents * 100, 2) : 0;
        var ineligibleCasesPercentage = totalRespondents > 0 ? Math.Round((double)ineligibleCases / totalRespondents * 100, 2) : 0;
        var unobtainableCasesPercentage = totalRespondents > 0 ? Math.Round((double)unobtainableCases / totalRespondents * 100, 2) : 0;

        return new CaseSummary
        {
            CompletedCases = completedCases,
            AppointmentCases = appointmentCases,
            RefusalCases = refusalCases,
            OutOfTargetCases = outOfTargetCases,
            IneligibleCases = ineligibleCases,
            UnobtainableCases = unobtainableCases,

            // Return percentages as double, no % sign yet
            CompletedCasesPercentage = completedCasesPercentage,
            AppointmentCasesPercentage = appointmentCasesPercentage,
            RefusalCasesPercentage = refusalCasesPercentage,
            OutOfTargetCasesPercentage = outOfTargetCasesPercentage,
            IneligibleCasesPercentage = ineligibleCasesPercentage,
            UnobtainableCasesPercentage = unobtainableCasesPercentage
        };
    }
}



    }

}