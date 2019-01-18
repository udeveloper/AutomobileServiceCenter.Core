using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.WebMVC.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASC.WebMVC.Controllers
{
    [Authorize]
    [UserActivityFilter]
    public class BaseController : Controller
    {
       
    }
}