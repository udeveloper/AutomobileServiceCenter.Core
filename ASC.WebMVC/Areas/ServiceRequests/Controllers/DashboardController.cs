using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.WebMVC.Configuration;
using ASC.WebMVC.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ASC.WebMVC.Area.Dashboard.Controllers
{
    [Area("ServiceRequests")]
    public class DashboardController : BaseController
    {

        private IOptions<ApplicationSettings> _settings;
        public DashboardController(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }
        public IActionResult DashBoard()
        {
            return View();
        }

        public IActionResult TestException()
        {
            var i = 0;
            // Should through Divide by zero error
            var j = 1 / i;
            return View();
        }
    }
}