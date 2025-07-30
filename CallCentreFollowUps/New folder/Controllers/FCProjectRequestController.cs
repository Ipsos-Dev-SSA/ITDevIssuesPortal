using CallCentreFollowUps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CallCentreFollowUps.Controllers
{
    public class FCProjectRequestController : Controller
    {
        private readonly CallCentreTrackerEntities1 _context;

        public FCProjectRequestController()
        {
            _context = new CallCentreTrackerEntities1();
        }

        public ActionResult Index()
        {
            string currentUser = User.Identity.Name; // Get the logged-in user's username or ID

            // Get the user's role
            var userid = (from l in _context.aspnet_Users where l.UserName == currentUser select l.UserId).FirstOrDefault();
            var userroleid = (from l in _context.vw_aspnet_UsersInRoles where l.UserId == userid select l.RoleId).FirstOrDefault();
            var userrole = (from l in _context.aspnet_Roles where l.RoleId == userroleid select l.RoleName).FirstOrDefault();

            // Initialize projectRequests list
            List<FCProjectRequest> projectRequests;

            // Check the user's role and retrieve project requests accordingly
            if (userrole == "Supervisor Role") // Example: Admin sees all project requests
            {
                projectRequests = _context.FCProjectRequests.ToList();
            }
            else if (userrole == "Field Coordinator") // Example: Field Coordinator sees only their own requests
            {
                projectRequests = _context.FCProjectRequests
                    .Where(pr => pr.AddedBy == currentUser) // Filter by the logged-in user (Field Coordinator)
                    .ToList();
            }
            else
            {
                // Default case if the role is unknown or doesn't match
                projectRequests = new List<FCProjectRequest>();
            }

            return View(projectRequests);
        }

        //GET
        public ActionResult Create()
        {
            var activeSupervisors = _context.Supervisors
                                            .Where(s => s.IsActive == true)
                                            .ToList(); // return real entities

            ViewBag.Supervisors = new SelectList(activeSupervisors, "SupervisorID", "Name");

            ViewBag.Projects = new SelectList(_context.Projects, "ProjectID", "ProjectName");
            ViewBag.FieldCoordinators = new SelectList(_context.FieldCoordinators, "FieldCoordinatorID", "Name");

            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FCProjectRequest model)
        {
            string currentUser = User.Identity.Name;
            model.AddedBy = currentUser;

            // 🚨 DEBUG: Check what SupervisorID was submitted
            System.Diagnostics.Debug.WriteLine("SupervisorID from form: " + model.SupervisorID);

            if (ModelState.IsValid)
            {
                // Make sure you're comparing against model.SupervisorID properly
                var supervisor = _context.Supervisors
                                         .FirstOrDefault(s => s.SupervisorID == model.SupervisorID && s.IsActive == true);

                if (supervisor == null)
                {
                    ModelState.AddModelError("", "Invalid or inactive Supervisor selected.");
                    RepopulateDropdowns(model);
                    return View(model);
                }

                // Assign and calculate
                model.SupervisorID = supervisor.SupervisorID;

                var project = _context.Projects
                                      .Where(p => p.ProjectID == model.ProjectID)
                                      .Select(p => new { p.PayRate })
                                      .FirstOrDefault();

                if (project != null)
                {
                    model.Amount = project.PayRate * model.NumberOfInterviews;
                    model.TotalDue = model.Amount * 0.20m;
                }
                else
                {
                    ModelState.AddModelError("", "Invalid Project selection.");
                    RepopulateDropdowns(model);
                    return View(model);
                }

                _context.FCProjectRequests.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            RepopulateDropdowns(model);
            return View(model);
        }


        private void RepopulateDropdowns(FCProjectRequest model)
        {
            var activeSupervisors = _context.Supervisors
                                            .Where(s => s.IsActive == true)
                                            .Select(s => new { s.SupervisorID, s.Name })
                                            .ToList();
            ViewBag.Supervisors = new SelectList(activeSupervisors, "SupervisorID", "Name", model.SupervisorID);
            ViewBag.Projects = new SelectList(_context.Projects, "ProjectID", "ProjectName", model.ProjectID);
            ViewBag.FieldCoordinators = new SelectList(_context.FieldCoordinators, "FieldCoordinatorID", "Name", model.FieldCoordinatorID);
        }







        // Edit: Display the form for editing an existing project request
        public ActionResult Edit(int id)
        {
            var projectRequest = _context.FCProjectRequests
                .FirstOrDefault(p => p.RequestID == id);
            if (projectRequest == null)
            {
                return HttpNotFound();
            }

            // Populate dropdowns
            ViewBag.Supervisors = new SelectList(_context.Supervisors, "SupervisorID", "Name", projectRequest.SupervisorID);
            ViewBag.Projects = new SelectList(_context.Projects, "ProjectID", "ProjectName", projectRequest.ProjectID);
            ViewBag.FieldCoordinators = new SelectList(_context.FieldCoordinators, "FieldCoordinatorID", "Name", projectRequest.FieldCoordinatorID);
            return View(projectRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
   
        public ActionResult Edit(FCProjectRequest request)
        {
            if (!ModelState.IsValid)
            {
                RepopulateDropdowns(request);
                return View(request);
            }

            // Validate active Supervisor
            var supervisor = _context.Supervisors
                .FirstOrDefault(s => s.SupervisorID == request.SupervisorID && s.IsActive == true);
            if (supervisor == null)
            {
                ModelState.AddModelError("", "Selected Supervisor is not valid or inactive.");
                RepopulateDropdowns(request);
                return View(request);
            }

            // Validate Project and get PayRate
            var project = _context.Projects
                .Where(p => p.ProjectID == request.ProjectID)
                .Select(p => new { p.PayRate })
                .FirstOrDefault();

            if (project == null)
            {
                ModelState.AddModelError("", "Selected Project is not valid.");
                RepopulateDropdowns(request);
                return View(request);
            }

            // Fetch the existing record (tracked)
            var existing = _context.FCProjectRequests
                .FirstOrDefault(r => r.RequestID == request.RequestID);

            if (existing == null)
            {
                return HttpNotFound();
            }

            // Update values
            existing.SupervisorID = request.SupervisorID;
            existing.ProjectID = request.ProjectID;
            existing.NumberOfInterviews = request.NumberOfInterviews;
            existing.Comments = request.Comments;
            existing.DateRequested = request.DateRequested;

            // Update calculated values
            existing.Amount = project.PayRate * request.NumberOfInterviews;
            existing.TotalDue = existing.Amount * 0.20m;

            // Save changes
            _context.SaveChanges();
            return RedirectToAction("Index");
        }




        // Delete: Delete an existing project request
        public ActionResult Delete(int id)
        {
            var projectRequest = _context.FCProjectRequests
                .FirstOrDefault(p => p.RequestID == id);
            if (projectRequest == null)
            {
                return HttpNotFound();
            }

            _context.FCProjectRequests.Remove(projectRequest);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
        
}
