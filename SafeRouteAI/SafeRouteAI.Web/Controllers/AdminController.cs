using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeRouteAI.Web.Models.Pages;
using SafeRouteAI.Web.Services;

namespace SafeRouteAI.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly SafeRouteApiClient _apiClient;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        SafeRouteApiClient apiClient,
        ILogger<AdminController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ModerateReports(
        CancellationToken cancellationToken)
    {
        var token = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            return Challenge();
        }

        var result = await _apiClient.GetPendingReportsAsync(
            token,
            cancellationToken);
        return View(new ModerationPageViewModel
        {
            Reports = result.Value ?? [],
            ErrorMessage = result.IsSuccess
                ? null
                : "The moderation queue could not be loaded. Please try again later."
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModerateReport(
        int reportId,
        string status,
        CancellationToken cancellationToken)
    {
        if (reportId <= 0 ||
            status is not ("Approved" or "Rejected"))
        {
            return BadRequest();
        }

        var token = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            return Challenge();
        }

        var result = await _apiClient.ModerateReportAsync(
            token,
            reportId,
            status,
            cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                "Community report moderation failed for report {ReportId}: {Error}",
                reportId,
                result.Error?.Title ?? "Unknown API error");
            TempData["ModerationError"] = "The report status could not be updated.";
        }
        else
        {
            TempData["ModerationSuccess"] = $"Report #{reportId} was {status.ToLowerInvariant()}.";
        }

        return RedirectToAction(nameof(ModerateReports));
    }

    [HttpGet]
    public async Task<IActionResult> Analytics(
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken)
    {
        var token = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            return Challenge();
        }

        var result = await _apiClient.GetAdminAnalyticsAsync(
            token,
            from,
            to,
            cancellationToken);
        return View(new AdminAnalyticsPageViewModel
        {
            Analytics = result.Value,
            ErrorMessage = result.IsSuccess
                ? null
                : result.Error?.StatusCode == StatusCodes.Status400BadRequest
                    ? result.Error.Message
                    : "Analytics could not be loaded. Please try again later."
        });
    }
}
