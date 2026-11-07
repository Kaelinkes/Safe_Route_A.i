namespace SafeRouteAI.Web.Models.Api;

public enum ApiErrorKind
{
    Configuration,
    Connection,
    Validation,
    ServiceUnavailable,
    Server,
    Http,
    InvalidResponse
}

public sealed record ApiError(
    ApiErrorKind Kind,
    string Title,
    string Message,
    int? StatusCode = null);