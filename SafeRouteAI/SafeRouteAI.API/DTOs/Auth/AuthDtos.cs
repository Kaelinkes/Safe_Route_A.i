
using System.ComponentModel.DataAnnotations;
using SafeRouteAI.Application.Auth;

namespace SafeRouteAI.API.DTOs.Auth;

public sealed class RegisterRequest
{
    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

public sealed class LoginRequest
{
    [Required]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

public sealed record UserProfileResponse(
    int UserId,
    string Name,
    string Email,
    string Role)
{
    public static UserProfileResponse FromUser(UserAccount user) =>
        new(
            user.UserId,
            user.Name,
            user.Email,
            user.Role);
}

public sealed record LoginResponse(
    string Token,
    string TokenType,
    int ExpiresIn,
    UserProfileResponse User);
