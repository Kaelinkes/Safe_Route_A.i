using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeRouteAI.Web.Models.Api;
using SafeRouteAI.Web.Models.Auth;
using SafeRouteAI.Web.Services;

namespace SafeRouteAI.Web.Controllers;

public sealed class AccountController : Controller
{
    private const string AccessTokenClaim = "access_token";

    private readonly SafeRouteApiClient _apiClient;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SafeRouteApiClient apiClient,
        ILogger<AccountController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _apiClient.LoginAsync(
            new AuthLoginRequest(model.Email, model.Password),
            cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            if (result.Error?.StatusCode == StatusCodes.Status401Unauthorized)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The email address or password is incorrect.");
            }
            else
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The login service is unavailable. Please try again later.");
            }

            return View(model);
        }

        var login = result.Value;
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, login.User.UserId.ToString()),
            new Claim(ClaimTypes.Name, login.User.Name),
            new Claim(ClaimTypes.Email, login.User.Email),
            new Claim(ClaimTypes.Role, login.User.Role),
            new Claim(AccessTokenClaim, login.Token)
        ],
        CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(login.ExpiresIn)
            });

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
            Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register() => View();

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var token = User.FindFirstValue(AccessTokenClaim);
        if (!string.IsNullOrWhiteSpace(token))
        {
            var result = await _apiClient.LogoutAsync(token, cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    "The API session could not be revoked during Web logout: {Error}",
                    result.Error?.Title ?? "Unknown API error");
            }
        }

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied() => View();
}
