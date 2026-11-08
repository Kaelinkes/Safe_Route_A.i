using SafeRouteAI.Web.Models.Api;
using SafeRouteAI.Web.Models.Reports;

namespace SafeRouteAI.Web.Models.Pages;

public sealed class DashboardPageViewModel
{
    public IReadOnlyList<RouteHistoryResponse> Routes { get; init; } = [];
    public IReadOnlyList<ReportResponse> Reports { get; init; } = [];
    public string? ErrorMessage { get; init; }
}

public sealed class RouteHistoryPageViewModel
{
    public IReadOnlyList<RouteHistoryResponse> Routes { get; init; } = [];
    public string? ErrorMessage { get; init; }
}

public sealed class MyReportsPageViewModel
{
    public IReadOnlyList<ReportResponse> Reports { get; init; } = [];
    public string? ErrorMessage { get; init; }
}

public sealed class ModerationPageViewModel
{
    public IReadOnlyList<ReportResponse> Reports { get; init; } = [];
    public string? ErrorMessage { get; init; }
}

public sealed class AdminAnalyticsPageViewModel
{
    public AdminAnalyticsResponse? Analytics { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed class ReportPageViewModel
{
    public ReportSubmissionViewModel Submission { get; init; } = new();
    public string? ErrorMessage { get; init; }
}
