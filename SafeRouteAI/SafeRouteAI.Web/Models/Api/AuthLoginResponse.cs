namespace SafeRouteAI.Web.Models.Api;

public sealed record AuthLoginRequest(string Email, string Password);

public sealed record AuthLoginResponse(
    string Token,
    string TokenType,
    int ExpiresIn,
    AuthUserResponse User);

public sealed record AuthUserResponse(
    int UserId,
    string Name,
    string Email,
    string Role);
