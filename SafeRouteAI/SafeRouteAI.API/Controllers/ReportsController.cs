
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeRouteAI.API.DTOs.Reports;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Services;

namespace SafeRouteAI.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly CommunityReportService _reportService;

    public ReportsController(CommunityReportService reportService)
    {
        _reportService = reportService;
    }

    // GET /api/v1/reports
    // Public: only approved reports are returned.
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetApproved()
    {
        var reports = await _reportService.GetApprovedAsync();

        return Ok(reports.Select(ReportResponse.FromDto));
    }

    // GET /api/v1/reports/mine
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var reports = await _reportService.GetByUserAsync(userId);
        return Ok(reports.Select(ReportResponse.FromDto));
    }

    // POST /api/v1/reports
    // Requires a valid logged-in user.
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateReportRequest request)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId) ||
            userId <= 0)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.IncidentType))
        {
            return BadRequest(new
            {
                error = "Incident type is required."
            });
        }

        var report = new CommunityReportDto
        {
            UserId = userId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IncidentType = request.IncidentType.Trim(),
            Description = request.Description?.Trim()
        };

        var created = await _reportService.SubmitAsync(report);

        if (!created)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid community report",
                detail: "The supplied report is invalid.");
        }

        return StatusCode(
            StatusCodes.Status201Created,
            ReportResponse.FromDto(report));
    }

    private bool TryGetUserId(out int userId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out userId) && userId > 0;
    }
}
