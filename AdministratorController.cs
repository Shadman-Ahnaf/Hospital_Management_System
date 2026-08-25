using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministratorController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
