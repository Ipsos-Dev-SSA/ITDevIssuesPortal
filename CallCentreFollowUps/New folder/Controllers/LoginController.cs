using CallCentreFollowUps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CallCentreFollowUps.Controllers
{
    public class LoginController : Controller
    {
        private readonly CallCentreTrackerEntities1 _context;

        public LoginController()
        {
            _context = new CallCentreTrackerEntities1();
        }

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(FieldCoordinator login)
        {
            var fc = _context.FieldCoordinators
                .FirstOrDefault(f => f.Username == login.Username && f.Password == login.Password);
            if (fc != null)
            {
                Session["FCID"] = fc.FieldCoordinatorID;
                return RedirectToAction("Dashboard", "Home");
            }

            ModelState.AddModelError("", "Invalid username or password.");
            return View(login);
        }

        // Logout: Log the field coordinator out
        public ActionResult Logout()
        {
            Session.Remove("FCID");
            return RedirectToAction("Index");
        }
    }
}