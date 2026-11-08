
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SafeRouteAI.API.Services;

namespace SafeRouteAI.API.Controllers;

[ApiController]
[Route("api/v1/ml-integration")]
public sealed class MlPredictionController : ControllerBase
{
    private readonly PythonMlPredictionClient _python;

    public MlPredictionController(
        PythonMlPredictionClient python)
    {
        _python = python;
    }

    [HttpPost("predict-route")]
    public async Task<IActionResult> PredictRoute(
        [FromBody] JsonElement request,
        CancellationToken cancellationToken)
    {
        if (request.ValueKind != JsonValueKind.Object ||
            !request.TryGetProperty(
                "segments",
                out var segments) ||
            segments.ValueKind != JsonValueKind.Array ||
            segments.GetArrayLength() == 0)
        {
            return BadRequest(new
            {
                error = "A non-empty segments array is required."
            });
        }

        try
        {
            var result = await _python.PredictRouteAsync(
                request,
                cancellationToken);

            return new JsonResult(result.Payload)
            {
                StatusCode = result.StatusCode
            };
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
