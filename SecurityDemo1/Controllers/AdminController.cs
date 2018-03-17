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
        public string GivenName { get; set; } = "";
        public string Surname { get; set; } = "";
        public bool IsLoggedIn { get; set; } = false;
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

            AdminIndexViewModel model = new AdminIndexViewModel();

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                model.GivenName = user.GivenName;
                model.Surname = user.Surname;
                model.IsLoggedIn = true;
            }

            return View(model);
        }

        [Authorize(Policy = ApplicationUser.AppPolicies.Admin)]
        public IActionResult CreateUsers()
        {
            return null;
        }
    }
}