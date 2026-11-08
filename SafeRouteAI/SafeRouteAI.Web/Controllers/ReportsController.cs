using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeRouteAI.Web.Models.Api;
using SafeRouteAI.Web.Models.Pages;
using SafeRouteAI.Web.Models.Reports;
using SafeRouteAI.Web.Services;

namespace SafeRouteAI.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly SafeRouteApiClient _apiClient;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        SafeRouteApiClient apiClient,
        ILogger<ReportsController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index() => View(new ReportSubmissionViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        ReportSubmissionViewModel model,
        CancellationToken cancellationToken)
    {
        if (!model.ResponsibleUse)
        {
            ModelState.AddModelError(
                nameof(model.ResponsibleUse),
                "Please confirm that the report is accurate and responsible.");
        }

        if (!double.TryParse(
                model.Latitude,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var latitude) ||
            !double.IsFinite(latitude) ||
            latitude is < -90 or > 90)
        {
            ModelState.AddModelError(
                nameof(model.Latitude),
                "Choose a valid map latitude.");
        }

        if (!double.TryParse(
                model.Longitude,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var longitude) ||
            !double.IsFinite(longitude) ||
            longitude is < -180 or > 180)
        {
            ModelState.AddModelError(
                nameof(model.Longitude),
                "Choose a valid map longitude.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var token = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            return Challenge();
        }

        var result = await _apiClient.SubmitReportAsync(
            token,
            new CreateReportRequest(
                latitude,
                longitude,
                model.IncidentType,
                model.Description),
            cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                "Community report submission failed: {Error}",
                result.Error?.Title ?? "Unknown API error");
            ViewData["ErrorMessage"] = "Your report could not be saved. Please try again.";
            return View(model);
        }

        TempData["ReportSubmitted"] = "Your report was submitted and is awaiting review.";
        return RedirectToAction(nameof(MyReports));
    }

    [HttpGet]
    public async Task<IActionResult> MyReports(CancellationToken cancellationToken)
    {
        var token = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            return Challenge();
        }

        var result = await _apiClient.GetMyReportsAsync(token, cancellationToken);
        return View(new MyReportsPageViewModel
        {
            Reports = result.Value ?? [],
            ErrorMessage = result.IsSuccess
                ? null
                : "Your reports could not be loaded. Please try again later."
        });
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Approved(CancellationToken cancellationToken)
    {
        var result = await _apiClient.GetApprovedReportsAsync(cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ProblemDetails
                {
                    Title = "Community reports unavailable",
                    Detail = "Approved community reports could not be loaded."
                });
        }

        return Ok(result.Value);
    }
}
