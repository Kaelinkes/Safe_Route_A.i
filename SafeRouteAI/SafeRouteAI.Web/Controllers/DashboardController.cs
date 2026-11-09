using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeRouteAI.Web.Models.Pages;
using SafeRouteAI.Web.Services;

namespace SafeRouteAI.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly SafeRouteApiClient _apiClient;

    public DashboardController(SafeRouteApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var token = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            return Challenge();
        }

        var routesTask = _apiClient.GetRouteHistoryAsync(token, cancellationToken);
        var reportsTask = _apiClient.GetMyReportsAsync(token, cancellationToken);
        await Task.WhenAll(routesTask, reportsTask);

        var routes = await routesTask;
        var reports = await reportsTask;
        var errors = new[]
        {
            routes.Error?.Message,
            reports.Error?.Message
        }.Where(message => !string.IsNullOrWhiteSpace(message));

        return View(new DashboardPageViewModel
        {
            Routes = routes.Value ?? [],
            Reports = reports.Value ?? [],
            ErrorMessage = errors.Any()
                ? "Some dashboard data could not be loaded. Please try again later."
                : null
        });
    }
}
