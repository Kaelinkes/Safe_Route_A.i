using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Domain.Entities;
using SafeRouteAI.Domain.Enums;
using SafeRouteAI.Application.Services;

namespace SafeRouteAI.Application.Interfaces;

public interface IRouteService
{
    Task<List<RouteOptionDto>> GetRoutesAsync(
        Coordinate start, Coordinate destination, CancellationToken cancellationToken = default);
}

public interface IWeatherService
{
    Task<WeatherResult> GetWeatherAsync(Coordinate location, DateTime time);
}

public interface IRiskService
{
    Task<RiskAssessmentDto> AssessAsync(
        RouteOptionDto route, WeatherResult weather, CancellationToken cancellationToken = default);
}

public interface ICommunityReportRepository
{
    Task AddAsync(CommunityReport report);
    Task<List<CommunityReport>> GetApprovedAsync();
    Task UpdateStatusAsync(int reportId, ReportStatus status, int adminId);
}

public record RouteLogEntry(
    int? UserId,
    Coordinate Origin,
    Coordinate Destination,
    DateTime RequestedAt,
    List<RankedRouteDto> Routes);

public interface IRouteLogRepository
{
    Task SaveAsync(RouteLogEntry entry, CancellationToken cancellationToken = default);
}