using Microsoft.AspNetCore.Mvc;

namespace WebSite_GioiThieuSP.Areas.Admin.Controllers;

[Area("Admin")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
