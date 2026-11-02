
using Microsoft.AspNetCore.Mvc;
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.Interfaces;

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
    [ProducesResponseType(
        typeof(RouteComparisonResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RouteComparisonResponse>> Compare(
        [FromBody] RouteComparisonRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Origin is null || request.Destination is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Missing coordinates",
                Detail = "Origin and destination are required.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await _routeService.CompareAsync(
            request, cancellationToken);

        return Ok(result);
    }
}
