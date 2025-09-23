using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Insightly_project.Controllers
{
    [Authorize(Roles = "TeamMember")]

    public class TeamMemberDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
