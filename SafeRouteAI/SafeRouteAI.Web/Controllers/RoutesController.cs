using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeRouteAI.Web.Models.Api;
using SafeRouteAI.Web.Models.Pages;
using SafeRouteAI.Web.Services;

namespace SafeRouteAI.Web.Controllers;

[Authorize]
public class RoutesController : Controller
{
    private readonly SafeRouteApiClient _apiClient;

    public RoutesController(SafeRouteApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Plan() => View();

    public IActionResult Compare() => View();

    public async Task<IActionResult> History(CancellationToken cancellationToken)
    {
        var token = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            return Challenge();
        }

        var result = await _apiClient.GetRouteHistoryAsync(token, cancellationToken);
        return View(new RouteHistoryPageViewModel
        {
            Routes = result.Value ?? [],
            ErrorMessage = result.IsSuccess
                ? null
                : "Route history could not be loaded. Please try again later."
        });
    }

    [HttpGet]
    public async Task<IActionResult> Weather(
        [FromQuery] double? latitude,
        [FromQuery] double? longitude,
        CancellationToken cancellationToken)
    {
        if (latitude is not double validLatitude || !double.IsFinite(validLatitude) || validLatitude is < -90 or > 90 ||
            longitude is not double validLongitude || !double.IsFinite(validLongitude) || validLongitude is < -180 or > 180)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid coordinates",
                Detail = "Latitude must be between -90 and 90 and longitude between -180 and 180."
            });
        }

        var result = await _apiClient.GetWeatherAsync(validLatitude, validLongitude, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        var error = result.Error!;
        var statusCode = error.Kind switch
        {
            ApiErrorKind.Validation => StatusCodes.Status400BadRequest,
            ApiErrorKind.ServiceUnavailable or ApiErrorKind.Connection or ApiErrorKind.Configuration
                => StatusCodes.Status503ServiceUnavailable,
            ApiErrorKind.Server or ApiErrorKind.InvalidResponse => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status502BadGateway
        };

        return StatusCode(statusCode, new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode switch
            {
                StatusCodes.Status400BadRequest => "Weather request rejected",
                StatusCodes.Status503ServiceUnavailable => "Weather unavailable",
                _ => "Weather request failed"
            },
            Detail = error.Kind switch
            {
                ApiErrorKind.Validation => "The weather service could not process the selected location.",
                ApiErrorKind.ServiceUnavailable or ApiErrorKind.Connection or ApiErrorKind.Configuration =>
                    "Weather information is currently unavailable. Please try again later.",
                _ => "Weather information could not be loaded."
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompareRoutes(
        [FromBody] RouteComparisonRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid route request",
                Detail = "Origin and destination coordinates are required."
            });
        }

        if (!TryValidateModel(request))
        {
            return ValidationProblem(ModelState);
        }

        if (request.Origin is null || request.Destination is null ||
            !double.IsFinite(request.Origin.Latitude) ||
            request.Origin.Latitude is < -90 or > 90 ||
            !double.IsFinite(request.Origin.Longitude) ||
            request.Origin.Longitude is < -180 or > 180 ||
            !double.IsFinite(request.Destination.Latitude) ||
            request.Destination.Latitude is < -90 or > 90 ||
            !double.IsFinite(request.Destination.Longitude) ||
            request.Destination.Longitude is < -180 or > 180)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid coordinates",
                Detail = "Both locations must contain valid latitude and longitude coordinates."
            });
        }

        if (request.Origin.Latitude == request.Destination.Latitude &&
            request.Origin.Longitude == request.Destination.Longitude)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Locations must be different",
                Detail = "Choose different origin and destination coordinates."
            });
        }

        var result = await _apiClient.CompareRoutesAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        var error = result.Error!;
        var statusCode = error.Kind switch
        {
            ApiErrorKind.Validation => StatusCodes.Status400BadRequest,
            ApiErrorKind.ServiceUnavailable or ApiErrorKind.Connection or ApiErrorKind.Configuration
                => StatusCodes.Status503ServiceUnavailable,
            ApiErrorKind.Server or ApiErrorKind.InvalidResponse => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status502BadGateway
        };

        return StatusCode(statusCode, new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode switch
            {
                StatusCodes.Status400BadRequest => "Route request rejected",
                StatusCodes.Status503ServiceUnavailable => "Route calculation unavailable",
                _ => "Route comparison failed"
            },
            Detail = error.Kind switch
            {
                ApiErrorKind.ServiceUnavailable => "Real route calculation is not connected yet.",
                ApiErrorKind.Connection or ApiErrorKind.Configuration =>
                    "The route calculation service could not be reached. Please try again later.",
                ApiErrorKind.Validation => "The route request was rejected. Check the selected coordinates.",
                _ => "Route comparison could not be completed."
            }
        });
    }
}
