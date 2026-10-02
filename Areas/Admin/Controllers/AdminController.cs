using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebSite_GioiThieuSP.Areas.Admin.Controllers
{
    // Controller phục vụ các trang giao diện mẫu (template Startmin)
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Dashboard";
            return View();
        }

        public IActionResult Tables()
        {
            ViewData["Title"] = "Tables";
            return View();
        }

        public IActionResult Forms()
        {
            ViewData["Title"] = "Forms";
            return View();
        }

        public IActionResult Buttons()
        {
            ViewData["Title"] = "Buttons";
            return View();
        }

        public IActionResult PanelsWells()
        {
            ViewData["Title"] = "Panels and Wells";
            return View();
        }

        public IActionResult Notifications()
        {
            ViewData["Title"] = "Notifications";
            return View();
        }

        public IActionResult Typography()
        {
            ViewData["Title"] = "Typography";
            return View();
        }

        public IActionResult Icons()
        {
            ViewData["Title"] = "Icons";
            return View();
        }

        public IActionResult Grid()
        {
            ViewData["Title"] = "Grid";
            return View();
        }

        public IActionResult Flot()
        {
            ViewData["Title"] = "Flot Charts";
            return View();
        }

        public IActionResult Morris()
        {
            ViewData["Title"] = "Morris.js Charts";
            return View();
        }

        public IActionResult Blank()
        {
            ViewData["Title"] = "Blank Page";
            return View();
        }

        public IActionResult Login()
        {
            ViewData["Title"] = "Login";
            return View();
        }
    }
}