
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.Interfaces;
using SafeRouteAI.Application.Exceptions;

namespace SafeRouteAI.API.Controllers;

[ApiController]
[Route("api/v1/routes")]
public class RoutesController : ControllerBase
{
    private readonly IRouteComparisonService _routeService;
    private readonly ILogger<RoutesController> _logger;

    public RoutesController(
        IRouteComparisonService routeService,
        ILogger<RoutesController> logger)
    {
        _routeService = routeService;
        _logger = logger;
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
        catch (RouteNotFoundException ex)
        {
            _logger.LogWarning(
                ex,
                "No valid driving route was found.");

            return Problem(
                statusCode: 404,
                title: "Route not found",
                detail: "No valid driving route could be found for the supplied locations.");
        }
        catch (SafeRouteAI.API.Services.RouteServiceUnavailableException ex)
        {
            _logger.LogError(
                ex,
                "Routing service is unavailable.");

            return Problem(
                statusCode: 503,
                title: "Routing service unavailable",
                detail: "The routing service is currently unavailable.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Invalid route comparison request.");

            return Problem(
                statusCode: 400,
                title: "Invalid route request",
                detail: "The supplied route request is invalid.");
        }
        catch (OperationCanceledException ex)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "Routing provider timed out.");

            return Problem(
                statusCode: 503,
                title: "Routing provider timeout",
                detail: "The routing provider took too long to respond.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Routing provider request failed. HTTP status: {StatusCode}",
                ex.StatusCode?.ToString() ?? "Unavailable");

            return Problem(
                statusCode: 502,
                title: "Routing provider error",
                detail: "The routing provider could not complete the request.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Routing service encountered an operational error.");

            return Problem(
                statusCode: 502,
                title: "Routing service error",
                detail: "The routing service could not complete the request.");
        }
    }
}
