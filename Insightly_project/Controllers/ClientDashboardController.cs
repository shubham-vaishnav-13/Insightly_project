using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Insightly_project.Controllers
{
    [Authorize(Roles = "Client")]

    public class ClientDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
