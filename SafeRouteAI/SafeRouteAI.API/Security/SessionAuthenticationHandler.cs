
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SafeRouteAI.Application.Auth;

namespace SafeRouteAI.API.Security;

public sealed class SessionAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "SessionBearer";

    private readonly AuthService _authService;

    public SessionAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        AuthService authService)
        : base(options, logger, encoder)
    {
        _authService = authService;
    }

    protected override async Task<AuthenticateResult>
        HandleAuthenticateAsync()
    {
        var authorization =
            Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorization))
        {
            return AuthenticateResult.NoResult();
        }

        const string prefix = "Bearer ";

        if (!authorization.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail(
                "Invalid authorization scheme.");
        }

        var token = authorization[prefix.Length..].Trim();

        if (string.IsNullOrWhiteSpace(token) ||
            token.Length > 512 ||
            token.Any(char.IsWhiteSpace))
        {
            return AuthenticateResult.Fail(
                "Invalid bearer token.");
        }

        var user = await _authService.ValidateSessionAsync(token);

        if (user is null)
        {
            return AuthenticateResult.Fail(
                "Invalid or expired session.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier,
                user.UserId.ToString()),

            new(ClaimTypes.Name, user.Name),

            new(ClaimTypes.Email, user.Email),

            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims, SchemeName);

        var principal = new ClaimsPrincipal(identity);

        return AuthenticateResult.Success(
            new AuthenticationTicket(
                principal, SchemeName));
    }
}
