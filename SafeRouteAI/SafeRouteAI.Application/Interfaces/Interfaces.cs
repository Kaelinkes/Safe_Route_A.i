using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Domain.Entities;
using SafeRouteAI.Domain.Enums;

namespace SafeRouteAI.Application.Interfaces;

public interface IRoutingService
{
    Task<List<RouteOptionDto>> GetRoutesAsync(Coordinate origin, Coordinate destination);
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