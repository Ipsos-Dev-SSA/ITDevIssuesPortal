using CallCentreFollowUps.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CallCentreFollowUps;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;

namespace CallCentreFollowUps.Controllers
{
   
    public class CxController : Controller
    {
        private CallCentreTrackerEntities1 db = new CallCentreTrackerEntities1();

        public ActionResult Index()
        {

            var strUserDetails = Request.LogonUserIdentity.Name;
            var CurrentUserName = strUserDetails.Split('\\')[1].Split(' ')[0];
            CurrentUserName = CurrentUserName.Substring(0, 1).ToUpper() + CurrentUserName.Substring(1);
            ViewBag.UserName = CurrentUserName; 
            var issues = db.Issuess
              .Include(i => i.Status)
              .Include(i => i.aspnet_Users)
              .Where(i => i.Status.Name == "Completed"
                       || i.Status.Name == "Logged"
                       || i.Status.Name == "In Progress"
                       || i.Status.Name == "Pushed to Test"
                       || i.Status.Name == "Rejected"
                       || i.Status.Name == "Assigned")
              .OrderByDescending(i => i.UpdatedDate)
              .ToList();

            ViewBag.SubmittedBy = new SelectList(db.aspnet_Users, "UserId", "UserName");
            return View(issues);
        }


        public ActionResult Assign(int id)
        {
            var issue = db.Issuess.Find(id);
            if (issue == null) return HttpNotFound();

            // Load Teams
            var teams = db.Teams
                .OrderBy(t => t.TeamName)
                .Select(t => new { t.TeamId, t.TeamName })
                .ToList();

            ViewBag.TeamList = new SelectList(teams, "TeamId", "TeamName");

            // Load Statuses
            List<SelectListItem> statusList;

            var currentStatus = db.Statuses.FirstOrDefault(s => s.StatusId == issue.StatusId);

            if (currentStatus != null && currentStatus.Name == "Logged" || currentStatus.Name == "Rejected" || currentStatus.Name == "In Progress")
            {
                // Only show "Assigned" status if current is "Logged"
                statusList = db.Statuses
                    .Where(s => s.Name == "Assigned" )
                    .OrderBy(s => s.Name)
                    .Select(s => new SelectListItem { Value = s.StatusId.ToString(), Text = s.Name })
                    .ToList();
            }

           else if (currentStatus != null && currentStatus.Name == "Assigned")
            {
                // Show "Closed" and "Completed" options when current status is "Assigned"
                statusList = db.Statuses
                    .Where(s => s.Name == "Closed" || s.Name == "Completed")
                    .OrderBy(s => s.Name)
                    .Select(s => new SelectListItem
                    {
                        Value = s.StatusId.ToString(),
                        Text = s.Name
                    })
                    .ToList();
            }
            else if (currentStatus != null && currentStatus.Name == "Completed")
            {
                // Only show "Assigned" status if current is "Logged"
                statusList = db.Statuses
                    .Where(s => s.Name == "Pushed to Test")
                    .OrderBy(s => s.Name)
                    .Select(s => new SelectListItem { Value = s.StatusId.ToString(), Text = s.Name })
                    .ToList();
            }
            else if(currentStatus != null && currentStatus.Name == "Pushed to Test")
            {
                // Only show "Assigned" status if current is "Logged"
                statusList = db.Statuses
                    .Where(s => s.Name == "Presented for Approval")
                    .OrderBy(s => s.Name)
                    .Select(s => new SelectListItem { Value = s.StatusId.ToString(), Text = s.Name })
                    .ToList();
            }
        
            else
            {
                // Default behavior (show all statuses)
                statusList = db.Statuses
                    .OrderBy(s => s.Name)
                    .Select(s => new SelectListItem { Value = s.StatusId.ToString(), Text = s.Name })
                    .ToList();
            }

            var history = db.IssueHistories
                .Where(h => h.IssueId == id)
                .OrderByDescending(h => h.ChangeDate)
                .ToList();

            ViewBag.IssueHistory = history;

            ViewBag.StatusList = new SelectList(statusList, "Value", "Text");

            return View(issue);
        }


        [HttpPost]
        public ActionResult Assign(int id, int teamId, int newStatusId, string comment)
        {
            var username = User.Identity.Name;
            var user = db.aspnet_Users.FirstOrDefault(u => u.UserName == username);
            var issue = db.Issuess.Find(id);
            if (issue == null)
                return HttpNotFound();

            var status = db.Statuses.FirstOrDefault(s => s.StatusId == newStatusId);
            if (status == null)
            {
                TempData["Error"] = "Selected status not found.";
                return RedirectToAction("Index");
            }

            issue.TeamId = teamId;
            issue.StatusId = newStatusId;
            issue.UpdatedDate = DateTime.Now;
            issue.Comment = comment;

            db.IssueHistories.Add(new Models.IssueHistory
            {
                IssueId = issue.IssueId,
                StatusId = newStatusId,
                StatusName = status.Name,
                ChangedBy = ViewBag.CurrentUserName,
                ChangeDate = DateTime.Now,
                Comments = comment
            });

            db.SaveChanges();

            // Resolve physical file paths for attachments
            List<string> attachmentPaths = new List<string>();
            if (!string.IsNullOrEmpty(issue.AttachmentPath))
            {
                var relativePaths = issue.AttachmentPath.Split(',');
                foreach (var relPath in relativePaths)
                {
                    string fullPath = Server.MapPath(relPath);
                    if (System.IO.File.Exists(fullPath))
                    {
                        attachmentPaths.Add(fullPath);
                    }
                }
            }

            // Notify CX role members
            var fromEmail = ConfigurationManager.AppSettings["outgoingsmtpmailusername"];

            var cxRoleId = db.aspnet_Roles
                .Where(r => r.RoleName == "CX")
                .Select(r => r.RoleId)
                .FirstOrDefault();

            // Get users in the selected team who are in the CX role
            var teamMembers = db.aspnet_Users
                .Where(u => u.TeamId == teamId)
                .Join(db.vw_aspnet_UsersInRoles, u => u.UserId, ur => ur.UserId, (u, ur) => new { User = u, ur.RoleId })
                .Where(joined => joined.RoleId == cxRoleId)
                .Join(db.aspnet_Membership, j => j.User.UserId, m => m.UserId, (j, m) => new { j.User.UserName, m.Email })
                .Where(x => x.Email != null)
                .ToList();

            string systemBaseUrl = ConfigurationManager.AppSettings["SystemBaseUrl"];
            string issueLink = $"{systemBaseUrl}/Developer/Assigned/{issue.IssueId}";

            foreach (var member in teamMembers)
            {
                string subject = $"[Team Issue Assigned] {issue.Title}";
                string body = $@"
                    <p>Hi {member.UserName},</p>

                    <p>
                        A new issue titled <strong>{issue.Title}</strong> has been assigned to your team for action.
                    </p>

                    <table style='border-collapse: collapse; width: 100%; font-family: Arial, sans-serif;'>
                        <tr>
                            <td style='padding: 8px; font-weight: bold; border: 1px solid #ddd;'>Status</td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{status.Name}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; font-weight: bold; border: 1px solid #ddd;'>Assigned By</td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{username}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; font-weight: bold; border: 1px solid #ddd;'>Date Assigned</td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{DateTime.Now:yyyy-MM-dd HH:mm}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; font-weight: bold; border: 1px solid #ddd;'>Comment</td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{comment}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; font-weight: bold; border: 1px solid #ddd;'>View Issue</td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>
                                <a href='{issueLink}' target='_blank'>{issueLink}</a>
                            </td>
                        </tr>
                    </table>

                    <br/>

                    <p>Kind regards,<br/>{username}</p>
                ";

                // Send email
                CommonMethods.SendMail(member.Email, fromEmail, subject, body, true, attachmentPaths);
            }

            TempData["Success"] = "Issue assigned to team and status updated successfully.";
            return RedirectToAction("Index");
        }






            [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PresentToClient(int id)
        {
            var username = User.Identity.Name;
            var user = db.aspnet_Users.FirstOrDefault(u => u.UserName == username);
            var issue = db.Issuess.Find(id);
            if (issue == null) return HttpNotFound();

            var status = db.Statuses.FirstOrDefault(s => s.Name == "Presented for Approval");
            if (status == null)
            {
                TempData["Error"] = "Status 'Presented for Approval' not found.";
                return RedirectToAction("Index");
            }

            issue.StatusId = status.StatusId;
            issue.UpdatedDate = DateTime.Now;

            db.IssueHistories.Add(new Models.IssueHistory
            {
                IssueId = issue.IssueId,
                StatusId = status.StatusId,
                ChangedBy = user.UserName,
                ChangeDate = DateTime.Now,
                Comments = "Presented to client for approval"
            });

            db.SaveChanges();

            // ✅ Get all users in the "Client" role
            var clientRole = db.aspnet_Roles.FirstOrDefault(r => r.RoleName == "Client");
            if (clientRole != null)
            {
                var clientUserIds = db.vw_aspnet_UsersInRoles
                    .Where(r => r.RoleId == clientRole.RoleId)
                    .Select(r => r.UserId)
                    .ToList();

                var clientEmails = db.aspnet_Membership
                    .Where(m => clientUserIds.Contains(m.UserId) && !string.IsNullOrEmpty(m.Email))
                    .Select(m => m.Email)
                    .ToList();

                string systemBaseUrl = ConfigurationManager.AppSettings["SystemBaseUrl"];
                string issueLink = $"{systemBaseUrl}/CX/Assign/{issue.IssueId}";

                string subject = $"Issue Presented to Client: {issue.Title}";
                string body = $@"
            <p>Dear Client,</p>

            <p>
                The issue titled <strong>{issue.Title}</strong> has been presented for your approval.
            </p>

            <table style='border-collapse: collapse; width: 100%; font-family: Arial, sans-serif;'>
                <tr>
                    <td style='padding: 8px; font-weight: bold; border: 1px solid #ddd;'>Status</td>
                    <td style='padding: 8px; border: 1px solid #ddd;'>Presented for Approval</td>
                </tr>
                <tr>
                    <td style='padding: 8px; font-weight: bold; border: 1px solid #ddd;'>Submitted By</td>
                    <td style='padding: 8px; border: 1px solid #ddd;'>{username}</td>
                </tr>
                <tr>
                    <td style='padding: 8px; font-weight: bold; border: 1px solid #ddd;'>Date</td>
                    <td style='padding: 8px; border: 1px solid #ddd;'>{DateTime.Now:yyyy-MM-dd HH:mm}</td>
                </tr>
                <tr>
                    <td style='padding: 8px; font-weight: bold; border: 1px solid #ddd;'>View Issue</td>
                    <td style='padding: 8px; border: 1px solid #ddd;'>
                        <a href='{issueLink}' target='_blank'>{issueLink}</a>
                    </td>
                </tr>
            </table>

            <br/>

            <p>Kind regards,<br/>{username}</p>";

                if (clientEmails.Any())
                {
                    foreach (var email in clientEmails)
                    {
                        SafeSendMail(email, subject, body);
                    }
                }
                else
                {
                    TempData["Warning"] = "No email addresses found for Client role.";
                }
            }
            else
            {
                TempData["Warning"] = "Client role not found. Email not sent.";
            }

            TempData["Success"] = "Issue successfully presented to client.";
            return RedirectToAction("Index");
        }



        public ActionResult Close(int id)
        {
            var username = User.Identity.Name;
            var user = db.aspnet_Users.FirstOrDefault(u => u.UserName == username);
            var issue = db.Issuess.Find(id);
            if (issue == null) return HttpNotFound();

            var status = db.Statuses.FirstOrDefault(s => s.Name == "Closed");
            if (status == null)
            {
                TempData["Error"] = "Status 'Closed' not found.";
                return RedirectToAction("Index");
            }

            issue.StatusId = status.StatusId;
            issue.UpdatedDate = DateTime.Now;

            db.IssueHistories.Add(new Models.IssueHistory
            {
                IssueId = issue.IssueId,
                StatusId = status.StatusId,
                ChangedBy = user.UserName,
                ChangeDate = DateTime.Now,
                Comments = "Closed by CX after live deployment"
            });

            db.SaveChanges();

            var devUser = db.aspnet_Membership.FirstOrDefault(u => u.UserId == issue.AssignedTo);
            if (devUser != null)
            {
                string subject = $"Issue Closed: {issue.Title}";
                string body = $"Hi,<br/><br/>The issue titled: <strong>{issue.Title}</strong> has been closed after successful deployment.<br/><br/>Regards,<br/>CX Team";

                SafeSendMail(devUser.Email, subject, body);
            }

            TempData["Success"] = "Issue closed successfully.";
            return RedirectToAction("Index");
        }

        private void SafeSendMail(string toEmail, string subject, string body)
        {
            try
            {
                if (!string.IsNullOrEmpty(toEmail))
                {
                    CommonMethods.SendMail(toEmail, "", subject, body, true);
                }
                else
                {
                    TempData["Warning"] = "Email not sent. Developer has no registered email address.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to send email. Error: {ex.Message}";
            }
        }

        private Guid GetCurrentUserId()
        {
            var user = db.aspnet_Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            return user?.UserId ?? Guid.Empty;
        }

        // GET: Issues/Create
        // GET: Issues/Create
        public ActionResult Create()
        {
            ViewBag.StatusId = new SelectList(db.Statuses, "StatusId", "Name");
            ViewBag.SubmittedBy = new SelectList(db.aspnet_Users, "UserId", "UserName");

            // Add categories dropdown
            var categories = db.IssueCategories
                               .Where(c => c.IsActive)
                               .OrderBy(c => c.Name)
                               .Select(c => new { c.CategoryId, c.Name })
                               .ToList();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "Name");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Issue issue, IEnumerable<HttpPostedFileBase> attachments)
        {
            if (ModelState.IsValid)
            {
                var username = User.Identity.Name;
                var user = db.aspnet_Users.FirstOrDefault(u => u.UserName == username);

                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    PopulateCreateDropdowns();
                    return View(issue);
                }

                var defaultStatus = db.Statuses.FirstOrDefault(s => s.Name == "Logged");
                if (defaultStatus == null)
                {
                    ModelState.AddModelError("", "Default status 'Logged' not found.");
                    PopulateCreateDropdowns();
                    return View(issue);
                }

                var userRoleId = db.vw_aspnet_UsersInRoles
                    .Where(ur => ur.UserId == user.UserId)
                    .Select(ur => ur.RoleId)
                    .FirstOrDefault();

                if (userRoleId == Guid.Empty || !db.aspnet_Roles.Any(r => r.RoleId == userRoleId))
                {
                    ModelState.AddModelError("", "User role is not assigned or not found.");
                    PopulateCreateDropdowns();
                    return View(issue);
                }

                var categoryExists = db.IssueCategories.Any(c => c.CategoryId == issue.CategoryId && c.IsActive);
                if (!categoryExists)
                {
                    ModelState.AddModelError("CategoryId", "Please select a valid issue category.");
                    PopulateCreateDropdowns();
                    return View(issue);
                }

                // Handle multiple file uploads
                List<string> uploadedFilePaths = new List<string>();
                if (attachments != null && attachments.Any())
                {
                    string directoryPath = Server.MapPath("~/Uploads/Attachments");
                    Directory.CreateDirectory(directoryPath);

                    foreach (var file in attachments)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string fileName = Path.GetFileName(file.FileName);
                            string filePath = Path.Combine(directoryPath, fileName);
                            file.SaveAs(filePath);
                            uploadedFilePaths.Add(filePath); // Absolute path for attachment
                        }
                    }

                    issue.AttachmentPath = string.Join(",", uploadedFilePaths.Select(p => "/Uploads/Attachments/" + Path.GetFileName(p)));
                }

                issue.StatusId = defaultStatus.StatusId;
                issue.SubmittedBy = username;
                issue.CreatedDate = DateTime.Now;
                issue.RoleId = userRoleId;

                db.Issuess.Add(issue);
                db.SaveChanges();

                //db.IssueHistories.Add(new IssueHistory
                //{
                //    IssueId = issue.IssueId,
                //    StatusId = issue.StatusId,
                //    ChangedBy = user.UserName,
                //    ChangeDate = DateTime.Now,
                //    Comments = "Issue Logged"
                //});

                db.SaveChanges();

                // EMAIL: Notify all CX users with attachments and link
                var cxRoleId = db.aspnet_Roles
                                 .Where(r => r.RoleName == "CX")
                                 .Select(r => r.RoleId)
                                 .FirstOrDefault();

                if (cxRoleId != Guid.Empty)
                {
                    var cxUserIds = db.vw_aspnet_UsersInRoles
                                      .Where(ur => ur.RoleId == cxRoleId)
                                      .Select(ur => ur.UserId)
                                      .ToList();

                    var cxEmails = db.aspnet_Membership
                                     .Where(m => cxUserIds.Contains(m.UserId))
                                     .Select(m => m.Email)
                                     .ToList();

                    string systemBaseUrl = ConfigurationManager.AppSettings["SystemBaseUrl"];
                    string issueLink = $"{systemBaseUrl}/CX/Assign/{issue.IssueId}";

                    string emailBody = $@"
                    <p>A new issue has been logged and assigned for review.</p>
                    <table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; font-family: Arial, sans-serif; font-size: 14px;'>
                        <thead style='background-color: #f2f2f2;'>
                            <tr>
                                <th style='text-align: left;'>Description</th>
                                <th style='text-align: left;'>Value</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>Issue Title</td>
                                <td>{issue.Title}</td>
                            </tr>
                            <tr>
                                <td>Submitted By</td>
                                <td>{username}</td>
                            </tr>
                            <tr>
                                <td>Date</td>
                                <td>{DateTime.Now:yyyy-MM-dd hh:mm:ss tt}</td>
                            </tr>
                            <tr>
                                <td>View Issue</td>
                                <td><a href='{issueLink}' target='_blank'>{issueLink}</a></td>
                            </tr>
                        </tbody>
                    </table>
                    <br/>
                    <p>Kind Regards,<br/><strong>{username}</strong></p>
                ";

                    foreach (var toEmail in cxEmails)
                    {
                        CommonMethods.SendMail(
                            toEmail,
                            CommonMethods.CurrentUserEmail,
                            $"Issue Logged: {issue.Title}",
                            emailBody,
                            true,
                            uploadedFilePaths // Pass list of full file paths
                        );
                    }
                }

                return RedirectToAction("Index");
            }

            PopulateCreateDropdowns();
            return View(issue);
        }


        // Helper method to repopulate dropdowns when ModelState invalid
        private void PopulateCreateDropdowns()
        {
            ViewBag.StatusId = new SelectList(db.Statuses, "StatusId", "Name");
            ViewBag.SubmittedBy = new SelectList(db.aspnet_Users, "UserId", "UserName");

            var categories = db.IssueCategories
                               .Where(c => c.IsActive)
                               .OrderBy(c => c.Name)
                               .Select(c => new { c.CategoryId, c.Name })
                               .ToList();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "Name");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
