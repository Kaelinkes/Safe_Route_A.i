namespace SafeRouteAI.Web.Models.Api;

public sealed record ApiResult<T>
{
    private ApiResult(T? value, ApiError? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }

    public ApiError? Error { get; }

    public bool IsSuccess => Error is null && Value is not null;

    public static ApiResult<T> Success(T value) => new(value, null);

    public static ApiResult<T> Failure(ApiError error) => new(default, error);
}