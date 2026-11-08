using System.ComponentModel.DataAnnotations;

namespace SafeRouteAI.Web.Models.Auth;

public sealed class LoginViewModel
{
    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
