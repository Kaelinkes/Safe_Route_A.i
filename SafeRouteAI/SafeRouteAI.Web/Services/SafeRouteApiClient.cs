using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SafeRouteAI.Web.Models.Api;

namespace SafeRouteAI.Web.Services;

public sealed class SafeRouteApiClient
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

    private readonly HttpClient _httpClient;
    private readonly ILogger<SafeRouteApiClient> _logger;

    public SafeRouteApiClient(
        HttpClient httpClient,
        ILogger<SafeRouteApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<ApiResult<HealthResponse>> GetHealthAsync(
        CancellationToken cancellationToken = default)
    {
        return SendAsync<HealthResponse>(
            new HttpRequestMessage(HttpMethod.Get, "api/v1/health"),
            cancellationToken);
    }

    public Task<ApiResult<AuthLoginResponse>> LoginAsync(
        AuthLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage(
            HttpMethod.Post,
            "api/v1/auth/login")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        return SendAsync<AuthLoginResponse>(message, cancellationToken);
    }

    public Task<ApiResult<UserProfileResponse>> RegisterAsync(
        AuthRegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage(
            HttpMethod.Post,
            "api/v1/auth/register")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        return SendAsync<UserProfileResponse>(message, cancellationToken);
    }

    public Task<ApiResult<IReadOnlyList<RouteHistoryResponse>>> GetRouteHistoryAsync(
        string token,
        CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<RouteHistoryResponse>>(
            new HttpRequestMessage(HttpMethod.Get, "api/v1/routes/history"),
            cancellationToken,
            token);

    public Task<ApiResult<IReadOnlyList<ReportResponse>>> GetMyReportsAsync(
        string token,
        CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<ReportResponse>>(
            new HttpRequestMessage(HttpMethod.Get, "api/v1/reports/mine"),
            cancellationToken,
            token);

    public Task<ApiResult<IReadOnlyList<ReportResponse>>> GetApprovedReportsAsync(
        CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<ReportResponse>>(
            new HttpRequestMessage(HttpMethod.Get, "api/v1/reports"),
            cancellationToken);

    public Task<ApiResult<IReadOnlyList<ReportResponse>>> GetPendingReportsAsync(
        string token,
        CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<ReportResponse>>(
            new HttpRequestMessage(HttpMethod.Get, "api/v1/admin/reports/pending"),
            cancellationToken,
            token);

    public Task<ApiResult<AdminAnalyticsResponse>> GetAdminAnalyticsAsync(
        string token,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (from is not null)
        {
            query.Add("from=" + Uri.EscapeDataString(from.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
        }

        if (to is not null)
        {
            query.Add("to=" + Uri.EscapeDataString(to.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
        }

        var path = "api/v1/admin/analytics";
        if (query.Count > 0)
        {
            path += "?" + string.Join("&", query);
        }

        return SendAsync<AdminAnalyticsResponse>(
            new HttpRequestMessage(HttpMethod.Get, path),
            cancellationToken,
            token);
    }

    public Task<ApiResult<ReportResponse>> SubmitReportAsync(
        string token,
        CreateReportRequest report,
        CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage(
            HttpMethod.Post,
            "api/v1/reports")
        {
            Content = JsonContent.Create(report, options: JsonOptions)
        };

        return SendAsync<ReportResponse>(message, cancellationToken, token);
    }

    public Task<ApiResult<bool>> ModerateReportAsync(
        string token,
        int reportId,
        string status,
        CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage(
            HttpMethod.Patch,
            $"api/v1/admin/reports/{reportId}/status")
        {
            Content = JsonContent.Create(
                new ModerateReportRequest(status),
                options: JsonOptions)
        };

        return SendNoContentAsync(message, token, cancellationToken);
    }

    public async Task<ApiResult<bool>> LogoutAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (_httpClient.BaseAddress is null)
        {
            return ApiResult<bool>.Failure(new ApiError(
                ApiErrorKind.Configuration,
                "API address is not configured.",
                "Configure SafeRouteApi:BaseAddress before using the API client."));
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/v1/auth/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token);

        try
        {
            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult<bool>.Success(true);
            }

            return ApiResult<bool>.Failure(
                await CreateHttpErrorAsync(response, cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogWarning(exception, "The SafeRoute API logout request timed out.");
            return ApiResult<bool>.Failure(new ApiError(
                ApiErrorKind.ServiceUnavailable,
                "The API did not respond in time.",
                "The requested service is currently unavailable."));
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "The SafeRoute API could not be reached during logout.");
            return ApiResult<bool>.Failure(new ApiError(
                ApiErrorKind.Connection,
                "The API could not be reached.",
                "SafeRoute services are temporarily unavailable."));
        }
    }

    public Task<ApiResult<WeatherResponse>> GetWeatherAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        var latitudeText = latitude.ToString("R", CultureInfo.InvariantCulture);
        var longitudeText = longitude.ToString("R", CultureInfo.InvariantCulture);
        var path = "api/v1/weather?latitude=" + Uri.EscapeDataString(latitudeText) +
                   "&longitude=" + Uri.EscapeDataString(longitudeText);

        return SendAsync<WeatherResponse>(
            new HttpRequestMessage(HttpMethod.Get, path),
            cancellationToken);
    }

    public Task<ApiResult<RiskPredictionResponse>> PredictRiskAsync(
        RiskPredictionRequest request,
        CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage(
            HttpMethod.Post,
            "api/v1/risk/predict")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        return SendAsync<RiskPredictionResponse>(message, cancellationToken);
    }

    public Task<ApiResult<RouteComparisonResponse>> CompareRoutesAsync(
        RouteComparisonRequest request,
        CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage(
            HttpMethod.Post,
            "api/v1/routes/compare")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        return SendAsync<RouteComparisonResponse>(message, cancellationToken);
    }

    private async Task<ApiResult<TResponse>> SendAsync<TResponse>(
        HttpRequestMessage request,
        CancellationToken cancellationToken,
        string? bearerToken = null)
        where TResponse : class
    {
        using (request)
        {
            if (_httpClient.BaseAddress is null)
            {
                return Failure<TResponse>(
                    ApiErrorKind.Configuration,
                    "API address is not configured.",
                    "Configure SafeRouteApi:BaseAddress before using the API client.");
            }

            if (!string.IsNullOrWhiteSpace(bearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    bearerToken);
            }

            try
            {
                using var response = await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return ApiResult<TResponse>.Failure(
                        await CreateHttpErrorAsync(response, cancellationToken));
                }

                var value = await response.Content.ReadFromJsonAsync<TResponse>(
                    JsonOptions,
                    cancellationToken);

                if (value is null)
                {
                    return InvalidResponse<TResponse>(
                        (int)response.StatusCode,
                        "The API returned an empty response.");
                }

                if (value is WeatherResponse { IsAvailable: false })
                {
                    return ApiResult<TResponse>.Failure(new ApiError(
                        ApiErrorKind.ServiceUnavailable,
                        "Weather service unavailable.",
                        "Weather information is currently unavailable.",
                        (int)response.StatusCode));
                }

                var validationMessage = ValidateResponse(value);
                if (validationMessage is not null)
                {
                    return InvalidResponse<TResponse>(
                        (int)response.StatusCode,
                        validationMessage);
                }

                return ApiResult<TResponse>.Success(value);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException exception)
            {
                _logger.LogWarning(exception, "The SafeRoute API request timed out.");
                return Failure<TResponse>(
                    ApiErrorKind.ServiceUnavailable,
                    "The API did not respond in time.",
                    "The requested service is currently unavailable.");
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "The SafeRoute API could not be reached.");
                return Failure<TResponse>(
                    ApiErrorKind.Connection,
                    "The API could not be reached.",
                    "SafeRoute services are temporarily unavailable.");
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "The SafeRoute API returned invalid JSON.");
                return InvalidResponse<TResponse>(
                    null,
                    "The API returned an invalid response.");
            }
            catch (NotSupportedException exception)
            {
                _logger.LogWarning(exception, "The SafeRoute API response format was unsupported.");
                return InvalidResponse<TResponse>(
                    null,
                    "The API returned an unsupported response format.");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An unexpected SafeRoute API client error occurred.");
                return Failure<TResponse>(
                    ApiErrorKind.Server,
                    "The API request could not be completed.",
                    "An unexpected error occurred while contacting SafeRoute services.");
            }
        }
    }

    private async Task<ApiResult<bool>> SendNoContentAsync(
        HttpRequestMessage request,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        using (request)
        {
            if (_httpClient.BaseAddress is null)
            {
                return ApiResult<bool>.Failure(new ApiError(
                    ApiErrorKind.Configuration,
                    "API address is not configured.",
                    "Configure SafeRouteApi:BaseAddress before using the API client."));
            }

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                bearerToken);

            try
            {
                using var response = await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return ApiResult<bool>.Success(true);
                }

                return ApiResult<bool>.Failure(
                    await CreateHttpErrorAsync(response, cancellationToken));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException exception)
            {
                _logger.LogWarning(exception, "The SafeRoute API request timed out.");
                return ApiResult<bool>.Failure(new ApiError(
                    ApiErrorKind.ServiceUnavailable,
                    "The API did not respond in time.",
                    "The requested service is currently unavailable."));
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "The SafeRoute API could not be reached.");
                return ApiResult<bool>.Failure(new ApiError(
                    ApiErrorKind.Connection,
                    "The API could not be reached.",
                    "SafeRoute services are temporarily unavailable."));
            }
        }
    }

    private static string? ValidateResponse<TResponse>(TResponse response)
    {
        if (response is AuthLoginResponse login &&
            (string.IsNullOrWhiteSpace(login.Token) ||
             !string.Equals(login.TokenType, "Bearer", StringComparison.OrdinalIgnoreCase) ||
             login.ExpiresIn <= 0 ||
             login.User is null ||
             login.User.UserId <= 0 ||
             string.IsNullOrWhiteSpace(login.User.Name) ||
             string.IsNullOrWhiteSpace(login.User.Email) ||
             string.IsNullOrWhiteSpace(login.User.Role)))
        {
            return "The login response is missing required account or session data.";
        }

        if (response is HealthResponse health &&
            (string.IsNullOrWhiteSpace(health.Status) ||
             string.IsNullOrWhiteSpace(health.Service) ||
             string.IsNullOrWhiteSpace(health.Version)))
        {
            return "The health response is missing required fields.";
        }

        if (response is WeatherResponse weather)
        {

            if (!double.IsFinite(weather.Latitude) ||
                !double.IsFinite(weather.Longitude) ||
                !double.IsFinite(weather.TemperatureCelsius) ||
                !double.IsFinite(weather.HumidityPercent) ||
                !double.IsFinite(weather.PrecipitationMm) ||
                !double.IsFinite(weather.VisibilityMeters) ||
                !double.IsFinite(weather.WindSpeedKmh) ||
                string.IsNullOrWhiteSpace(weather.ObservationTimeUtc))
            {
                return "The weather response contains invalid or incomplete data.";
            }
        }

        if (response is RiskPredictionResponse risk &&
            string.IsNullOrWhiteSpace(risk.SegmentId))
        {
            return "The risk response is missing its segment identifier.";
        }

        if (response is RouteComparisonResponse routes)
        {
            if (routes.Routes is null)
            {
                return "The route response is missing its routes collection.";
            }

            foreach (var route in routes.Routes)
            {
                if (string.IsNullOrWhiteSpace(route.RouteId) ||
                    !double.IsFinite(route.DistanceKm) || route.DistanceKm < 0 ||
                    !double.IsFinite(route.DurationMinutes) || route.DurationMinutes < 0 ||
                    (route.RiskScore is double score && !double.IsFinite(score)) ||
                    !Enum.IsDefined(route.RiskLevel))
                {
                    return "The route response contains invalid route data.";
                }
            }
        }

        return null;
    }

    private async Task<ApiError> CreateHttpErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        ApiProblemDetails? problem = null;

        try
        {
            problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(
                JsonOptions,
                cancellationToken);
        }
        catch (JsonException)
        {
            // Use a safe status-based message when the error body is not valid ProblemDetails JSON.
        }
        catch (NotSupportedException)
        {
            // Use a safe status-based message when the error content type is unsupported.
        }

        var statusCode = (int)response.StatusCode;
        var kind = response.StatusCode switch
        {
            HttpStatusCode.BadRequest => ApiErrorKind.Validation,
            HttpStatusCode.ServiceUnavailable => ApiErrorKind.ServiceUnavailable,
            _ when statusCode >= 500 => ApiErrorKind.Server,
            _ => ApiErrorKind.Http
        };

        var title = problem?.Title ?? "API request failed.";
        var message = kind switch
        {
            ApiErrorKind.Validation => problem?.Detail ?? problem?.Title ?? "The API rejected the request.",
            ApiErrorKind.ServiceUnavailable => problem?.Detail ?? "The requested API service is currently unavailable.",
            ApiErrorKind.Server => "The API encountered an unexpected server error.",
            _ => problem?.Detail ?? "The API request could not be completed."
        };

        return new ApiError(kind, title, message, statusCode);
    }

    private static ApiResult<T> InvalidResponse<T>(
        int? statusCode,
        string message)
    {
        return ApiResult<T>.Failure(new ApiError(
            ApiErrorKind.InvalidResponse,
            "Invalid API response.",
            message,
            statusCode));
    }

    private static ApiResult<T> Failure<T>(
        ApiErrorKind kind,
        string title,
        string message,
        int? statusCode = null)
    {
        return ApiResult<T>.Failure(new ApiError(kind, title, message, statusCode));
    }
}