using Microsoft.AspNetCore.Mvc;
namespace SafeRouteAI.Web.Controllers;
public class RoutesController : Controller
{
    public IActionResult Plan() => View();
    public IActionResult Compare() => View();
    public IActionResult History() => View();
}
