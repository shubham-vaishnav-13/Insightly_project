using Insightly_project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Insightly_project.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, true, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(email);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin")) return RedirectToAction("Index", "AdminDashboard");
                if (roles.Contains("TeamMember")) return RedirectToAction("Index", "TeamMemberDashboard");
                if (roles.Contains("Client")) return RedirectToAction("Index", "ClientDashboard");

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }
    }
}
