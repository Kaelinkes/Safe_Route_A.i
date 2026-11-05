
using System.Net;
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
            return Problem(
                statusCode: 503,
                title: "Routing service unavailable",
                detail: "Real route calculation is not connected yet.");
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound ||
                  ex.StatusCode == HttpStatusCode.BadRequest)
        {
            return Problem(
                statusCode: 422,
                title: "Route could not be calculated",
                detail: "No valid driving route could be found for the supplied locations.");
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return Problem(
                statusCode: 503,
                title: "Routing provider temporarily unavailable",
                detail: "The routing provider is currently limiting requests. Please try again later.");
        }
        catch (HttpRequestException)
        {
            return Problem(
                statusCode: 502,
                title: "Routing provider error",
                detail: "The routing provider could not complete the request.");
        }
    }
}
