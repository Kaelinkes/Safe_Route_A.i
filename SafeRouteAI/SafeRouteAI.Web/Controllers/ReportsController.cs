using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SafeRouteAI.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    public IActionResult Index() => View();
    public IActionResult MyReports() => View();
}