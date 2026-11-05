using Microsoft.AspNetCore.Mvc;
namespace SafeRouteAI.Web.Controllers;
public class AdminController : Controller
{
    public IActionResult ModerateReports() => View();
    public IActionResult Analytics() => View();
}
