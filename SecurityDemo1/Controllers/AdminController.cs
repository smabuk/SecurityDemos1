using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecurityDemo1.Models;
using SecurityDemo1.Models.AdminViewModels;

namespace SecurityDemo1.Models.AdminViewModels
{
        public class AdminIndexViewModel
        {
            public string GivenName { get; set; }
            public string Surname { get; set; }
        }
}

namespace SecurityDemo1.Controllers
{
    [Authorize(Policy = ApplicationUser.AppPolicies.Admin)]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            AdminIndexViewModel model = new AdminIndexViewModel
            {
                GivenName = user.GivenName,
                Surname = user.Surname
            };

            return View(model);
        }

        [Authorize(Policy = ApplicationUser.AppPolicies.Admin)]
        public IActionResult CreateUsers()
        {
            return null;
        }
    }
}