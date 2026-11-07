
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeRouteAI.API.DTOs.Reports;
using SafeRouteAI.Application.Auth;
using SafeRouteAI.Application.Services;
using SafeRouteAI.Domain.Enums;

namespace SafeRouteAI.API.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/v1/admin/reports")]
public sealed class AdminReportsController : ControllerBase
{
    private readonly CommunityReportService _reportService;

    public AdminReportsController(
        CommunityReportService reportService)
    {
        _reportService = reportService;
    }

    // GET /api/v1/admin/reports/pending
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var reports = await _reportService.GetPendingAsync();

        return Ok(reports.Select(ReportResponse.FromDto));
    }

    // PATCH /api/v1/admin/reports/{id}/status
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> Moderate(
        int id,
        [FromBody] ModerateReportRequest request)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                error = "Report ID must be positive."
            });
        }

        if (!Enum.TryParse<ReportStatus>(
                request.Status,
                ignoreCase: true,
                out var status) ||
            status is not (
                ReportStatus.Approved or ReportStatus.Rejected))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid moderation status",
                detail: "Status must be Approved or Rejected.");
        }

        var adminIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(adminIdValue, out var adminId) ||
            adminId <= 0)
        {
            return Unauthorized();
        }

        try
        {
            await _reportService.ModerateAsync(
                id,
                status,
                adminId);
        }
        catch (KeyNotFoundException)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Report not found",
                detail: "No report exists with the supplied ID.");
        }

        return NoContent();
    }
}
