using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Configuration;
using CallCentreFollowUps;
using CallCentreFollowUps.Controllers;
using CallCentreFollowUps.Models;
using IssueHistory = CallCentreFollowUps.Models.IssueHistory;
using System.IO;
using System.Net.Mail;
using System.Web;
using System.Collections.Generic;

namespace IssuesLog.Controllers
{
    public class IssuesController : Controller
    {
        private CallCentreTrackerEntities1 db = new CallCentreTrackerEntities1();


        public ActionResult Index()
        {
            var username = User.Identity.Name;
            var user = db.aspnet_Users.FirstOrDefault(u => u.UserName == username);

            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "User not found");
            }

            var userRoleId = db.vw_aspnet_UsersInRoles
                               .Where(ur => ur.UserId == user.UserId)
                               .Select(ur => ur.RoleId)
                               .FirstOrDefault();

            if (userRoleId == Guid.Empty)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "User does not have an assigned role");
            }
            var allowedRoles = db.aspnet_Roles
            .Select(r => r.RoleName)
            .ToList();

            var authenticate = Session["PanelistCode"]?.ToString();
            var countryCode = Session["CountrySession"]?.ToString();

            // Get current user's role
            var currentUserRole = Session["Role"]?.ToString();

            // Only return BadRequest if:
            // - PanelistCode or CountrySession is missing
            // - AND user's role is NOT found in allowedRoles
            bool isUserRoleAllowed = allowedRoles.Contains(currentUserRole);

            if ((string.IsNullOrEmpty(authenticate) || string.IsNullOrEmpty(countryCode)) && !isUserRoleAllowed)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Missing panelist code or country");
            }


            // Optional: retrieve the full country name if needed
            // var countryName = db.LutEfsCountries.FirstOrDefault(c => c.CountryCode == countryCode)?.Country;

            var useremailaddress = (from l in db.tblEFSUsers
                                    where l.panelist_code == authenticate
                                    select l.u_email).FirstOrDefault();

            var userid = (from l in db.aspnet_Membership
                          where l.Email == useremailaddress
                          select l.UserId).FirstOrDefault();

            var mappedUsername = (from l in db.aspnet_Users
                                  where l.UserId == userid
                                  select l.UserName).FirstOrDefault();

            var countryId = (from u in db.aspnet_Users
                             where u.UserName == mappedUsername
                             select u.CountryID).FirstOrDefault();

            // Use the countryId or countryCode as per your DB schema for filtering
            var issues = db.Issuess
                .Where(i => i.RoleId == userRoleId)
                .OrderByDescending(i => i.CreatedDate)
                .ToList();

            return View(issues);
        }



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

                // Handle file attachments
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
                            uploadedFilePaths.Add(filePath);
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

                db.IssueHistories.Add(new IssueHistory
                {
                    IssueId = issue.IssueId,
                    StatusId = issue.StatusId,
                    ChangedBy = user.UserName,
                    ChangeDate = DateTime.Now,
                    Comments = "Issue Logged"
                });

                db.SaveChanges();

                // Notify CX role members
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

                    // Safely format issue link
                    string baseUrl = ConfigurationManager.AppSettings["SystemBaseUrl"];
                    string issueLink = $"{baseUrl.TrimEnd('/')}/CX/Assign/{issue.IssueId}";

                    // Log generated link (for live debugging)


                    string imageUrl = $"{baseUrl.TrimEnd('/')}/Content/images/MTNEmailBanner.png";

                    string emailBody = $@"
                        <div style='text-align:center; margin-bottom:20px;'>
                            <img src='{imageUrl}' alt='MTN and Ipsos Banner' style='max-width:100%; height:auto;' />
                        </div>

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
                                    <td><a href='{issueLink}' target='_blank'>🔗 {issueLink}</a></td>
                                </tr>
                            </tbody>
                        </table>
                        <br/>
                        <p>Kind Regards,<br/><strong>{username}</strong></p>";



                    // Send email to all CX users
                    foreach (var toEmail in cxEmails)
                    {
                        CommonMethods.SendMail(
                            toEmail,
                            CommonMethods.CurrentUserEmail,
                            $"Issue Logged: {issue.Title}",
                            emailBody,
                            true,
                            uploadedFilePaths
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




        // POST: Issues/PresentToClient/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PresentToClient(int id)
        {
            var issue = db.Issuess.Find(id);
            if (issue == null) return HttpNotFound();

            var username = User.Identity.Name;
            var user = db.aspnet_Users.FirstOrDefault(u => u.UserName == username);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            var status = db.Statuses.FirstOrDefault(s => s.Name == "Presented for Approval");
            if (status == null)
            {
                TempData["Error"] = "Status 'Presented for Approval' not found.";
                return RedirectToAction("Index");
            }

            issue.StatusId = status.StatusId;
            issue.UpdatedDate = DateTime.Now;

            db.IssueHistories.Add(new CallCentreFollowUps.Models.IssueHistory
            {
                IssueId = issue.IssueId,
                StatusId = status.StatusId,
                ChangedBy = user.UserName,
                ChangeDate = DateTime.Now,
                Comments = "Presented to client for approval"
            });

            db.SaveChanges();

            var assignedUser = db.aspnet_Membership.FirstOrDefault(u => u.UserId == issue.AssignedTo);
            string toEmail = assignedUser?.Email ?? ConfigurationManager.AppSettings["outgoingsmtpmailusername"];

            CommonMethods.SendMail(
                toEmail,
                CommonMethods.CurrentUserEmail,
                $"Issue Presented to Client: {issue.Title}",
                $"Issue '{issue.Title}' was presented to the client on {DateTime.Now}.",
                true
            );

            return RedirectToAction("Index");
        }

        // POST: Issues/ApproveClient/5
        [HttpPost]
        public ActionResult ApproveClient(int id)
        {
            var issue = db.Issuess.Find(id);
            if (issue == null) return HttpNotFound();

            var approvedStatus = db.Statuses.FirstOrDefault(s => s.Name == "Approved");
            if (approvedStatus != null)
            {
                issue.StatusId = approvedStatus.StatusId;
                issue.UpdatedDate = DateTime.Now;

                db.IssueHistories.Add(new IssueHistory
                {
                    IssueId = issue.IssueId,
                    StatusId = approvedStatus.StatusId,
                    
                    ChangeDate = DateTime.Now,
                    Comments = "Client approved the issue"
                });

                db.SaveChanges();

                var assignedUser = db.aspnet_Membership.FirstOrDefault(u => u.UserId == issue.AssignedTo);
                string toEmail = assignedUser?.Email ?? ConfigurationManager.AppSettings["outgoingsmtpmailusername"];

                CommonMethods.SendMail(
                    toEmail,
                    CommonMethods.CurrentUserEmail,
                    $"Issue Approved: {issue.Title}",
                    $"Issue '{issue.Title}' was approved by the client.",
                    true
                );
            }

            return RedirectToAction("Index");
        }

        // POST: Issues/Deploy/5
        [HttpPost]
        public ActionResult Deploy(int id)
        {
            var issue = db.Issuess.Find(id);
            if (issue == null) return HttpNotFound();

            var approvedStatus = db.Statuses.FirstOrDefault(s => s.Name == "Approved");
            if (issue.StatusId != approvedStatus?.StatusId)
            {
                TempData["Message"] = "Cannot deploy until issue is approved.";
                return RedirectToAction("Index");
            }

            var deployedStatus = db.Statuses.FirstOrDefault(s => s.Name == "Pushed to Live");
            if (deployedStatus != null)
            {
                issue.StatusId = deployedStatus.StatusId;
                issue.UpdatedDate = DateTime.Now;

                db.IssueHistories.Add(new IssueHistory
                {
                    IssueId = issue.IssueId,
                    StatusId = deployedStatus.StatusId,
                   
                    ChangeDate = DateTime.Now,
                    Comments = "Issue deployed to live environment"
                });

                db.SaveChanges();

                var assignedUser = db.aspnet_Membership.FirstOrDefault(u => u.UserId == issue.AssignedTo);
                string toEmail = assignedUser?.Email ?? ConfigurationManager.AppSettings["outgoingsmtpmailusername"];

                CommonMethods.SendMail(
                    toEmail,
                    CommonMethods.CurrentUserEmail,
                    $"Issue Deployed: {issue.Title}",
                    $"Issue '{issue.Title}' was deployed to the live environment on {DateTime.Now}.",
                    true
                );
            }

            return RedirectToAction("Index");
        }

        // Helper to get current user's ID
        private Guid GetCurrentUserId()
        {
            var username = User.Identity.Name;
            var user = db.aspnet_Users.FirstOrDefault(u => u.UserName == username);
            return user?.UserId ?? Guid.Empty;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult ByStatus(string status)
        {
            var issues = db.Issuess

                .Where(i => i.Status.Name == status)
                .ToList();

            ViewBag.StatusFilter = status;
            return View("Index", issues);
        }
        public ActionResult GetTopIssueHistories()
        {
            var historyList = db.IssueHistories
                                .OrderByDescending(h => h.ChangeDate)
                                
                                .ToList();

            return View(historyList);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var issue = db.Issuess.Find(id);
            if (issue == null)
                return HttpNotFound();

            // Show only "Logged" status
            var loggedStatus = db.Statuses
                                 .Where(s => s.Name == "Logged")
                                 .ToList();
            ViewBag.StatusId = new SelectList(loggedStatus, "StatusId", "Name", issue.StatusId);

            ViewBag.AssignedTo = new SelectList(db.aspnet_Users, "UserId", "UserName", issue.AssignedTo);

            var categories = db.IssueCategories
                               .Where(c => c.IsActive)
                               .OrderBy(c => c.Name)
                               .Select(c => new { c.CategoryId, c.Name })
                               .ToList();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "Name", issue.CategoryId);

            return View(issue);
        }


        // POST: Issues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Issue issue, IEnumerable<HttpPostedFileBase> attachments)
        {
            if (ModelState.IsValid)
            {
                var existingIssue = db.Issuess.Find(issue.IssueId);
                if (existingIssue == null)
                    return HttpNotFound();

                // Validate category
                var categoryExists = db.IssueCategories.Any(c => c.CategoryId == issue.CategoryId && c.IsActive);
                if (!categoryExists)
                {
                    ModelState.AddModelError("CategoryId", "Please select a valid issue category.");
                    PopulateEditDropdowns(issue);
                    return View(issue);
                }

                existingIssue.Title = issue.Title;
                existingIssue.Description = issue.Description;
                existingIssue.AssignedTo = issue.AssignedTo;
                existingIssue.StatusId = issue.StatusId;
                existingIssue.UpdatedDate = DateTime.Now;
                existingIssue.CategoryId = issue.CategoryId;
                existingIssue.Priority = issue.Priority;

                // Handle attachments
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
                            uploadedFilePaths.Add(filePath);
                        }
                    }

                    existingIssue.AttachmentPath = string.Join(",", uploadedFilePaths.Select(p => "/Uploads/Attachments/" + Path.GetFileName(p)));
                }

                db.IssueHistories.Add(new IssueHistory
                {
                    IssueId = issue.IssueId,
                    StatusId = issue.StatusId,
                    ChangedBy = User.Identity.Name,
                    ChangeDate = DateTime.Now,
                    Comments = "Issue updated"
                });

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            PopulateEditDropdowns(issue);
            return View(issue);
        }
        private void PopulateEditDropdowns(Issue issue)
        {
            // Only Logged status for editing
            var loggedStatus = db.Statuses
                                 .Where(s => s.Name == "Logged")
                                 .ToList();
            ViewBag.StatusId = new SelectList(loggedStatus, "StatusId", "Name", issue.StatusId);

            ViewBag.AssignedTo = new SelectList(db.aspnet_Users, "UserId", "UserName", issue.AssignedTo);

            var categories = db.IssueCategories
                               .Where(c => c.IsActive)
                               .OrderBy(c => c.Name)
                               .Select(c => new { c.CategoryId, c.Name })
                               .ToList();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "Name", issue.CategoryId);
        }



        // GET: Issues/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var issue = db.Issuess.Find(id);
            if (issue == null)
                return HttpNotFound();

            return View(issue);
        }

        // POST: Issues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var issue = db.Issuess.Find(id);

            // Remove related history first
            var relatedHistories = db.IssueHistories.Where(h => h.IssueId == id).ToList();
            db.IssueHistories.RemoveRange(relatedHistories);

            db.Issuess.Remove(issue);
            db.SaveChanges();

            return RedirectToAction("Index");
        }





    }
}