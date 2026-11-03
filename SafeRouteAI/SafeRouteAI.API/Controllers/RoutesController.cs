
using Microsoft.AspNetCore.Mvc;
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.Interfaces;
using SafeRouteAI.API.Services;

namespace SafeRouteAI.API.Controllers;

[ApiController]
[Route("api/v1/routes")]
public class RoutesController : ControllerBase
{
    private readonly IRouteComparisonService _routeService;

    public RoutesController(IRouteComparisonService routeService)
    {
        _routeService = routeService;
    }

    [HttpPost("compare")]
    public async Task<IActionResult> Compare(
        [FromBody] RouteComparisonRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _routeService.CompareAsync(
                request, cancellationToken);

            return Ok(result);
        }
        catch (RouteServiceUnavailableException)
        {
            return StatusCode(503, new ProblemDetails
            {
                Title = "Routing service unavailable",
                Detail = "Real route calculation is not connected yet.",
                Status = 503
            });
        }
    }
}
