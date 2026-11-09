
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.Services;

namespace SafeRouteAI.API.Controllers;

[ApiController]
[Route("api/v1/routes")]
public sealed class RouteRiskPredictionController : ControllerBase
{
    private readonly RouteRiskPredictionAdapter _adapter;

    public RouteRiskPredictionController(
        RouteRiskPredictionAdapter adapter)
    {
        _adapter = adapter;
    }

    [HttpPost("predict-risk")]
    public async Task<IActionResult> PredictRisk(
        [FromBody] RouteRiskPredictionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _adapter.PredictAsync(
                request,
                cancellationToken);

            return new JsonResult(result.Payload)
            {
                StatusCode = result.StatusCode
            };
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
        catch (HttpRequestException)
        {
            return Problem(
                statusCode: 502,
                title: "Python ML service unavailable or invalid response");
        }
        catch (TaskCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            return Problem(
                statusCode: 504,
                title: "Python ML service timed out");
        }
    }
}
