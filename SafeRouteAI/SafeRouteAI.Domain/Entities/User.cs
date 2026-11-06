
namespace SafeRouteAI.Domain.Entities;

public class User
{
    public int UserID { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime DateRegistered { get; set; }
}
