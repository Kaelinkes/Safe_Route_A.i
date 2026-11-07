
using Microsoft.AspNetCore.Mvc;
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.Interfaces;

namespace SafeRouteAI.API.Controllers;

[ApiController]
[Route("api/v1/risk")]
public class RiskController : ControllerBase
{
    private readonly IRiskPredictionService _riskService;

    public RiskController(IRiskPredictionService riskService)
    {
        _riskService = riskService;
    }

    [HttpPost("predict")]
    [ProducesResponseType(
        typeof(RiskPredictionResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RiskPredictionResponse>> Predict(
        [FromBody] RiskPredictionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SegmentId))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid segment ID",
                Detail = "segmentId cannot be empty.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (request.Features is null || request.Features.Count == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Missing model features",
                Detail = "At least one feature is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await _riskService.PredictAsync(
            request, cancellationToken);

        return Ok(result);
    }
}
