
using CallCentreFollowUps.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CallCentreFollowUps.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private CallCentreTrackerEntities1 db = new CallCentreTrackerEntities1();

        public ActionResult Review()
        {
            var issues = db.Issuess
                .Include(i => i.Status)
                .Where(i => i.Status.Name == "Presented for Approval" || i.Status.Name == "Approved")
                .ToList();

            ViewBag.SubmittedBy = new SelectList(db.aspnet_Users, "UserId", "UserName");

            return View(issues);
        }

        public ActionResult Approve(int id)
        {
            return UpdateStatus(id, "Approved", "Client approved the change");
        }

        public ActionResult Reject(int id)
        {
            return UpdateStatus(id, "Rejected", "Client rejected the change");
        }

        private ActionResult UpdateStatus(int id, string statusName, string comment)
        {
            var issue = db.Issuess.Find(id);
            if (issue == null)
                return HttpNotFound();

            var status = db.Statuses.FirstOrDefault(s => s.Name == statusName);
            if (status == null)
            {
                TempData["Error"] = $"Status '{statusName}' not found.";
                return RedirectToAction("Review");
            }

            issue.StatusId = status.StatusId;
            issue.UpdatedDate = DateTime.Now;

            db.IssueHistories.Add(new Models.IssueHistory
            {
                IssueId = id,
                StatusId = status.StatusId,
               
                ChangeDate = DateTime.Now,
                Comments = comment
            });

            db.SaveChanges();

            SendEmailToClientsAndCx(issue.Title, statusName, comment);

            return RedirectToAction("Review");
        }

        private Guid GetCurrentUserId()
        {
            return db.aspnet_Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.UserId ?? Guid.Empty;
        }

        private void SendEmailToClientsAndCx(string issueTitle, string statusName, string comment)
        {
            // Get role IDs for Client and CX
            var roleIds = db.aspnet_Roles
                .Where(r => r.RoleName == "Client" || r.RoleName == "CX")
                .Select(r => r.RoleId)
                .ToList();

            // Get user IDs for users in those roles
            var userIds = db.vw_aspnet_UsersInRoles
                .Where(ur => roleIds.Contains(ur.RoleId))
                .Select(ur => ur.UserId)
                .Distinct()
                .ToList();

            // Get their email addresses
            var emails = db.aspnet_Membership
                .Where(m => userIds.Contains(m.UserId) && !string.IsNullOrEmpty(m.Email))
                .Select(m => m.Email)
                .ToList();

            string subject = $"Issue '{issueTitle}' has been {statusName}";
            string body = $@"Hi Team,<br/><br/>
                            The issue titled <strong>{issueTitle}</strong> has been <strong>{statusName}</strong>.<br/>
                            Comment: {comment}<br/><br/>
                            Regards,<br/>Client Team";

            foreach (var email in emails)
            {
                CommonMethods.SendMail(email, "", subject, body, true);
            }
        }
    }
}
