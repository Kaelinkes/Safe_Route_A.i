
using Microsoft.AspNetCore.Mvc;
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.Interfaces;
using SafeRouteAI.API.Services;
using SafeRouteAI.Application.Exceptions;

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
                request,
                cancellationToken);

            return Ok(result);
        }
        catch (RouteNotFoundException)
        {
            return Problem(
                statusCode: 404,
                title: "Route not found",
                detail: "No valid driving route could be found for the supplied locations.");
        }
        catch (ArgumentException)
        {
            return Problem(
                statusCode: 400,
                title: "Invalid route request",
                detail: "The supplied route request is invalid.");
        }
        catch (RouteServiceUnavailableException)
        {
            return Problem(
                statusCode: 503,
                title: "Routing service unavailable",
                detail: "Real route calculation is currently unavailable.");
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            return Problem(
                statusCode: 503,
                title: "Routing provider timeout",
                detail: "The routing provider took too long to respond. Please try again.");
        }
        catch (HttpRequestException)
        {
            return Problem(
                statusCode: 502,
                title: "Routing provider error",
                detail: "The routing provider could not complete the request.");
        }
        catch (InvalidOperationException)
        {
            return Problem(
                statusCode: 502,
                title: "Routing service error",
                detail: "The routing service could not complete the request.");
        }
    }
}
