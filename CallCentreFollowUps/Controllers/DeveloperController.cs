using CallCentreFollowUps.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CallCentreFollowUps; // For CommonMethods
using DocumentFormat.OpenXml.Office2010.Excel;
using System.ComponentModel.DataAnnotations;

namespace CallCentreFollowUps.Controllers
{
    //[Authorize(Roles = "TeamRole")]
    public class DeveloperController : Controller
    {
        private CallCentreTrackerEntities1 db = new CallCentreTrackerEntities1();

        public ActionResult Assigned()
        {
            var currentUserId = GetCurrentUserId();

            // Get current user's team
            var currentUser = db.aspnet_Users.FirstOrDefault(u => u.UserId == currentUserId);
            if (currentUser == null || currentUser.TeamId == null)
            {
                return HttpNotFound("User or team not found.");
            }

            var currentTeamId = currentUser.TeamId.Value;

            // Return issues for the team with matching statuses
            var issues = db.Issuess
                .Include(i => i.Status)
                .Include(i => i.aspnet_Users)
                .Where(i => i.TeamId == currentTeamId &&
                            (i.Status.Name == "Assigned" ||
                             i.Status.Name == "In Progress" ||
                             i.Status.Name == "Completed"))
                .ToList();

            // 🟡 Fetch issue history for all shown issues (you can filter this more if needed)
            var issueIds = issues.Select(i => i.IssueId).ToList();
            var history = db.IssueHistories
                            .Where(h => issueIds.Contains(h.IssueId))
                            .OrderByDescending(h => h.ChangeDate)
                            .ToList();

            ViewBag.IssueHistory = history;
            ViewBag.SubmittedBy = new SelectList(db.aspnet_Users, "UserId", "UserName");

            return View(issues);
        }



        [HttpPost]
        public ActionResult UpdateStatusWithComment(int? issueId, string newStatus, string comment, DateTime? completionDate)
        {
            var username = User.Identity.Name;
            var user = db.aspnet_Users.FirstOrDefault(u => u.UserName == username);

            if (!issueId.HasValue || issueId.Value <= 0)
            {
                TempData["Error"] = "Invalid issue ID.";
                return RedirectToAction("Assigned");
            }

            var issue = db.Issuess.Find(issueId.Value);
            if (issue == null)
            {
                TempData["Error"] = "Issue not found.";
                return RedirectToAction("Assigned");
            }

            var status = db.Statuses
                .FirstOrDefault(s => s.Name.Replace(" ", "").Equals(newStatus.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));

            if (status == null)
            {
                TempData["Error"] = "Invalid status.";
                return RedirectToAction("Assigned");
            }

            var currentUserId = GetCurrentUserId();
            var currentUser = db.aspnet_Users.FirstOrDefault(u => u.UserId == currentUserId);

            string currentUserEmail = null;
            if (currentUser != null)
            {
                var membership = db.aspnet_Membership.FirstOrDefault(m => m.UserId == currentUser.UserId);
                if (membership != null)
                {
                    currentUserEmail = membership.Email;
                }
            }

            if (currentUser == null || currentUser.TeamId == null)
            {
                return HttpNotFound("User or team not found.");
            }

            issue.StatusId = status.StatusId;
            issue.Comment = comment;
            issue.UpdatedDate = DateTime.Now;
            issue.AssignedTo = currentUser.UserId;
            issue.AssignedToUsername = currentUserEmail;

            if (completionDate.HasValue)
            {
                issue.CompletionDate = completionDate.Value;
            }

            db.IssueHistories.Add(new Models.IssueHistory
            {
                IssueId = issue.IssueId,
                StatusId = status.StatusId,
                StatusName = status.Name,
                ChangedBy = user.UserName,
                ChangeDate = DateTime.Now,
                Comments = comment
            });

            db.SaveChanges();

            // ✉️ Send email to all users in the "CX" role
            var cxUsers = (from u in db.aspnet_Users
                           join ur in db.vw_aspnet_UsersInRoles on u.UserId equals ur.UserId
                           join r in db.aspnet_Roles on ur.RoleId equals r.RoleId
                           join m in db.aspnet_Membership on u.UserId equals m.UserId
                           where r.RoleName == "CX" && m.Email != null
                           select new { u.UserName, m.Email }).ToList();

            string subject = $"Issue Update: {issue.Title} is now '{status.Name}'";

            string emailBody = $@"
                    <p>Hi,</p>
                    <p>The following issue has been updated:</p>
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
                                <td>Updated Status</td>
                                <td>{status.Name}</td>
                            </tr>
                            <tr>
                                <td>Comment</td>
                                <td>{comment}</td>
                            </tr>
                            <tr>
                                <td>Updated On</td>
                                <td>{DateTime.Now:yyyy-MM-dd hh:mm:ss tt}</td>
                            </tr>
                        </tbody>
                    </table>
                    <br/>
                    <p>Kind Regards,<br/><strong>{username}</strong></p>
                   ";

            foreach (var cxUser in cxUsers)
            {
                CommonMethods.SendMail(cxUser.Email, currentUserEmail, subject, emailBody, true);
            }

            TempData["Success"] = "Issue status updated successfully.";
            return RedirectToAction("Assigned");
        }





            public ActionResult MarkInProgress(int id)
        {
            return UpdateStatus(id, "In Progress", "Developer started working on the issue");
        }

        public ActionResult MarkCompleted(int id)
        {
            return UpdateStatus(id, "Completed", "Developer completed fix and internal validation");
        }

        public ActionResult PushToTest(int id)
        {
            return UpdateStatus(id, "Pushed to Test", "Fix pushed to test environment");
        }

        public ActionResult PushToLive(int id)
        {
            return UpdateStatus(id, "Pushed to Live", "Fix pushed to live environment");
        }

        private ActionResult UpdateStatus(int id, string statusName, string comment)
        {

            var username = User.Identity.Name;
            var user = db.aspnet_Users.FirstOrDefault(u => u.UserName == username);
            var issue = db.Issuess
                .Include(i => i.aspnet_Users)
                .FirstOrDefault(i => i.IssueId == id);

            if (issue == null)
                return HttpNotFound();

            var status = db.Statuses.FirstOrDefault(s => s.Name == statusName);
            if (status == null)
            {
                TempData["Error"] = $"Status '{statusName}' not found.";
                return RedirectToAction("Assigned");
            }

            issue.StatusId = status.StatusId;
            issue.UpdatedDate = DateTime.Now;

            db.IssueHistories.Add(new Models.IssueHistory
            {
                IssueId = issue.IssueId,
                StatusId = status.StatusId,
                ChangedBy = user.UserName,
                ChangeDate = DateTime.Now,
                Comments = comment
            });

            db.SaveChanges();

            // Send email to issue owner
            var owner = db.aspnet_Membership.FirstOrDefault(u => u.UserId == issue.AssignedTo);
            if (owner != null)
            {
                string subject = $"Issue Update: {issue.Title} is now '{statusName}'";
                string body = $@"Hi {owner.Email},<br/><br/>
                                 The issue <strong>{issue.Title}</strong> has been updated to status <strong>{statusName}</strong>.<br/>
                                 <strong>Comment:</strong> {comment}<br/><br/>
                                 Regards,<br/>Developer Team";

                CommonMethods.SendMail(owner.Email, "", subject, body, true);
            }

            TempData["Success"] = $"Issue '{issue.Title}' marked as '{statusName}' and notification sent.";
            return RedirectToAction("Assigned");
        }

        private Guid GetCurrentUserId()
        {
            return db.aspnet_Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.UserId ?? Guid.Empty;
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
