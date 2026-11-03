using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Domain.Entities;
using SafeRouteAI.Domain.Enums;

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

public interface ICommunityReportRepository
{
    Task AddAsync(CommunityReport report);
    Task<List<CommunityReport>> GetApprovedAsync();
    Task UpdateStatusAsync(int reportId, ReportStatus status, int adminId);
}