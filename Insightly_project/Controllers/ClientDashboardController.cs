using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// This Controller is used for client dashboard 

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
