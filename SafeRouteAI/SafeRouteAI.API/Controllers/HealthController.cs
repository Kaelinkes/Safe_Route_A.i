
using Microsoft.AspNetCore.Mvc;

namespace SafeRouteAI.API.Controllers;

[ApiController]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "Running",
            service = "SafeRoute AI API",
            version = "1.0.0"
        });
    }
}
