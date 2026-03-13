using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduLog.Web.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
