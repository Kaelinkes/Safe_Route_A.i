
using Microsoft.AspNetCore.Mvc;
using SafeRouteAI.API.Services;

namespace SafeRouteAI.API.Controllers;

[ApiController]
[Route("api/v1/ml-integration")]
public sealed class MlIntegrationController : ControllerBase
{
    private readonly PythonMlHealthClient _python;

    public MlIntegrationController(PythonMlHealthClient python)
    {
        _python = python;
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health(
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _python.GetHealthAsync(
                cancellationToken);

            return Ok(new
            {
                connected = true,
                pythonService = result
            });
        }
        catch (HttpRequestException)
        {
            return Problem(
                statusCode: 502,
                title: "Python ML service unavailable");
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
