using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Domain.Enums;

namespace SafeRouteAI.Application.Interfaces;

public interface ICommunityReportRepository
{
    Task AddAsync(CommunityReportDto report);
    Task<List<CommunityReportDto>> GetPendingAsync();
    Task<List<CommunityReportDto>> GetApprovedAsync();
    Task UpdateStatusAsync(int reportId, ReportStatus status, int adminId);
}

public interface IRouteService
{
    Task<List<RouteOptionDto>> GetRoutesAsync(
        Coordinate start,
        Coordinate destination,
        CancellationToken cancellationToken = default);
}

public interface IWeatherService
{
    Task<WeatherResult> GetWeatherAsync(
        Coordinate location,
        DateTime dateTime);
}

public interface IRiskService
{
    Task<RiskAssessmentDto> AssessAsync(
        RouteOptionDto route,
        WeatherResult weather,
        CancellationToken cancellationToken = default);
}

public interface IRouteLogRepository
{
    Task SaveAsync(
        RouteLogEntry entry,
        CancellationToken ct = default);
}