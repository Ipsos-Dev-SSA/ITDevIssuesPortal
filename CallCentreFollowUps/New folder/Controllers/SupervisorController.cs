using CallCentreFollowUps.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.Linq;
using System.Web.Mvc;

namespace CallCentreFollowUps.Controllers
{
    public class SupervisorController : Controller
    {
        private readonly CallCentreTrackerEntities1 _context;
        private readonly CallCentreTrackerEntities1 _payrollContext;

        public SupervisorController()
        {
            _context = new CallCentreTrackerEntities1();
            _payrollContext = new CallCentreTrackerEntities1();
        }

        private void LoadRegionsDropdown()
        {
            var countries = _payrollContext.Countries
                .OrderBy(c => c.CountryName)
                .ToList();

            ViewBag.RegionList = new SelectList(countries, "CountryName", "CountryName");
        }

        public ActionResult Index()
        {
            var supervisors = _context.Supervisors.ToList();
            return View(supervisors);
        }

        public ActionResult Create()
        {
            LoadRegionsDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Supervisor supervisor)
        {
            if (ModelState.IsValid)
            {
                // Check if the PC Number already exists in the database
                bool isPCNumberExists = _context.Supervisors
                    .Any(s => s.PCNumber == supervisor.PCNumber);

                if (isPCNumberExists)
                {
                    // Add an error to ModelState
                    ModelState.AddModelError("PCNumber", "The PC Number must be unique. This PC Number already exists.");
                }

                if (!ModelState.IsValid)
                {
                    // If ModelState is invalid, return the form with the validation error
                    LoadRegionsDropdown(); // Ensure dropdown values are reloaded on validation failure
                    return View(supervisor);
                }

                // Add the supervisor to the context and save changes
                _context.Supervisors.Add(supervisor);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // In case of any validation failure, reload regions and return to the form
            LoadRegionsDropdown();
            return View(supervisor);
        }


        public ActionResult Edit(int id)
        {
            var supervisor = _context.Supervisors.FirstOrDefault(s => s.SupervisorID == id);
            if (supervisor == null)
            {
                return HttpNotFound();
            }

            LoadRegionsDropdown();
            return View(supervisor);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Supervisor supervisor, int id)
        {
            if (ModelState.IsValid)
            {
                var super = _context.Supervisors.FirstOrDefault(s => s.SupervisorID == id);
                if (super == null)
                {
                    return HttpNotFound();
                }

                // Update the fields
                supervisor.Name = supervisor.Name;
                super.Region = supervisor.Region;
                super.IsActive = supervisor.IsActive;
                super.PCNumber = supervisor.PCNumber;

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            LoadRegionsDropdown();
            return View(supervisor);
        }


        public ActionResult Delete(int id)
        {
            var supervisor = _context.Supervisors.FirstOrDefault(s => s.SupervisorID == id);
            if (supervisor == null)
            {
                return HttpNotFound();
            }

            _context.Supervisors.Remove(supervisor);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
