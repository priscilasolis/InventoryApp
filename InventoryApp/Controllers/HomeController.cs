using InventoryApp.Models;
using InventoryApp.Repositories;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InventoryApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly InventoryRepository _repository = new InventoryRepository();

        public ActionResult Index()
        {
            List<string> roles;
            try
            {
                ApplicationUser currentUser = _repository.UserManager.FindById(User.Identity.GetUserId());
                roles = _repository.UserManager.GetRoles(User.Identity.GetUserId()).ToList();
            } catch (InvalidOperationException)
            {
                return RedirectToAction("Login", "Account");
            }

            if (roles.Any(i => i == "Administrator" || i == "Supervisor"))
            {
                ViewBag.Title = "Dashboard";
                ViewData["UserRole"] = "Super";
                return View();
            }
            ViewBag.Title = "Home page";
            ViewData["UserRole"] = "User";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "InventoryApp - April 2017";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Tecnológico de Monterrey Campus Chihuahua";

            return View();
        }
    }
}