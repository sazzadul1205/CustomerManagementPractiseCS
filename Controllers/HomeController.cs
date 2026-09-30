using CustomerManagementPractiseCS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CustomerManagementPractiseCS.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Send admins to the admin panel
            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            // Regular users (and anonymous) go to their profile
            return RedirectToAction("Index", "Profile");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}