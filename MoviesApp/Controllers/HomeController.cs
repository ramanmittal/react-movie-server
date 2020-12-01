using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MoviesApp.Models;

namespace MoviesApp.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Initialize([FromServices]MovieContext _context, [FromServices] UserManager<ApplicationUser> _userManager, [FromServices]IConfiguration _configuration, [FromServices] RoleManager<IdentityRole<int>> _roleManager)
        {
            if (await _userManager.FindByEmailAsync(_configuration["AdminEmail"]) == null)
            {
                using (var tr = _context.Database.BeginTransaction())
                {
                    var user = new ApplicationUser
                    {
                        Email = _configuration["AdminEmail"],
                        FirstName = _configuration["AdminFirstname"],
                        LastName = _configuration["AdminLastname"],
                        UserName = _configuration["AdminEmail"],
                    };
                    await _userManager.CreateAsync(user, _configuration["AdminPassword"]);
                    await _roleManager.CreateAsync(new IdentityRole<int>(Roles.Admin.ToString()));
                    await _roleManager.CreateAsync(new IdentityRole<int>(Roles.User.ToString()));
                    await _userManager.AddToRoleAsync(user, Roles.Admin.ToString());
                    await tr.CommitAsync();
                }
            }
            return Content("System has been Initialized.");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
