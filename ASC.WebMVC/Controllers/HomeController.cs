using ASC.Utilities;
using ASC.WebMVC.Data;
using ASC.WebMVC.Areas.Identity.Services;
using ASC.WebMVC.Configuration;
using ASC.WebMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.WebMVC.Controllers
{
 
    public class HomeController : AnonymousController
    {
        private IOptions<ApplicationSettings> _settings;
        public HomeController(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;           
        }

        public IActionResult Index()
        {
            // Set Session
            HttpContext.Session.SetSession("Test", _settings.Value);
            // Usage of IOptions
            var settings = HttpContext.Session.GetSession<ApplicationSettings>("Test");

            ViewBag.Title = _settings.Value.ApplicationTitle;
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

      


        public IActionResult Error(string id)
        {
            if (id == "404")
                return View("NotFound");
            if (id == "401" && User.Identity.IsAuthenticated)
                return View("AccessDenied");
            else
                return RedirectToPage("/Account/Login", new { area = "Identity" });

           
        }
    }

}
