using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}