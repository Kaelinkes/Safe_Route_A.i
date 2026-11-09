using System.ComponentModel.DataAnnotations;

namespace SafeRouteAI.Web.Models.Auth;

public sealed class RegisterViewModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = "";

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = "";

    [Required]
    [StringLength(128, MinimumLength = 8)]
    [RegularExpression(
        @"^(?=.*\p{L})(?=.*\d).+$",
        ErrorMessage = "Password must contain at least one letter and one digit.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required]
    [Compare(nameof(Password))]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";

    public bool ResponsibleUse { get; set; }
}
