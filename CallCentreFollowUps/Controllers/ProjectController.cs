using CallCentreFollowUps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CallCentreFollowUps.Controllers
{
    public class ProjectController : Controller
    {
        private readonly CallCentreTrackerEntities1 _context;

        public ProjectController()
        {
            _context = new CallCentreTrackerEntities1();
        }

        // Index: Display all projects
        public ActionResult Index()
        {
            var projects = _context.Projects.ToList();
            return View(projects);
        }

        // Create: Display the form for creating a new project
        public ActionResult Create()
        {
            return View();
        }

        // Create (POST): Save the new project
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Projects.Add(project);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // Edit: Display the form for editing an existing project
        public ActionResult Edit(int id)
        {
            var project = _context.Projects.FirstOrDefault(p => p.ProjectID == id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // Edit (POST): Save changes to the project
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(project).State = System.Data.Entity.EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // Delete: Delete an existing project
        public ActionResult Delete(int id)
        {
            var project = _context.Projects.FirstOrDefault(p => p.ProjectID == id);
            if (project == null)
            {
                return HttpNotFound();
            }

            _context.Projects.Remove(project);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}