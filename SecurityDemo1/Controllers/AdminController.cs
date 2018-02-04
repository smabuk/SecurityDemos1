using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityDemo1.Models;

namespace SecurityDemo1.Controllers
{
    [Authorize(Policy = ApplicationUser.AppPolicies.Admin)]
    public class AdminController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = ApplicationUser.AppPolicies.Admin)]
        public IActionResult CreateUsers()
        {
            return null;
        }
    }
}