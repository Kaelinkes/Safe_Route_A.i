
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SafeRouteAI.API.DTOs.Auth;
using SafeRouteAI.Application.Auth;

namespace SafeRouteAI.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private const int SessionLifetimeSeconds = 8 * 60 * 60;

    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    // POST /api/v1/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(
            request.Name,
            request.Email,
            request.Password);

        if (!result.Success || result.User is null)
        {
            if (result.Error?.Contains(
                    "already exists",
                    StringComparison.OrdinalIgnoreCase) == true)
            {
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Account already exists",
                    detail: result.Error);
            }

            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid registration",
                detail: result.Error ?? "Registration failed.");
        }

        return StatusCode(
            StatusCodes.Status201Created,
            UserProfileResponse.FromUser(result.User));
    }

    // POST /api/v1/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var ipAddress =
            HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _authService.LoginAsync(
            request.Email,
            request.Password,
            ipAddress);

        if (!result.Success ||
            result.User is null ||
            string.IsNullOrEmpty(result.Token))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication failed",
                detail: result.Error ?? "Invalid credentials.");
        }

        return Ok(new LoginResponse(
            Token: result.Token,
            TokenType: "Bearer",
            ExpiresIn: SessionLifetimeSeconds,
            User: UserProfileResponse.FromUser(result.User)));
    }

    // GET /api/v1/auth/me
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var token = GetBearerToken();

        if (token is null)
        {
            return Unauthorized();
        }

        var user = await _authService.ValidateSessionAsync(token);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(UserProfileResponse.FromUser(user));
    }

    // POST /api/v1/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = GetBearerToken();

        if (token is null)
        {
            return Unauthorized();
        }

        var user = await _authService.ValidateSessionAsync(token);

        if (user is null)
        {
            return Unauthorized();
        }

        await _authService.LogoutAsync(token);

        return NoContent();
    }

    private string? GetBearerToken()
    {
        if (!Request.Headers.TryGetValue(
                HeaderNames.Authorization,
                out var values))
        {
            return null;
        }

        if (values.Count != 1)
        {
            return null;
        }

        var authorization = values.ToString();

        const string prefix = "Bearer ";

        if (!authorization.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token = authorization[prefix.Length..].Trim();

        if (string.IsNullOrWhiteSpace(token) ||
            token.Contains(' ') ||
            token.Length > 512)
        {
            return null;
        }

        return token;
    }
}
