namespace SafeRouteAI.Application.Auth;

public static class Roles
{
    public const string User = "User";
    public const string Admin = "Admin";
}

public static class AccountStatus
{
    public const string Active = "Active";
}

public class UserAccount
{
    public int UserId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = Roles.User;
    public string Status { get; set; } = AccountStatus.Active;
    public int FailedLogins { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public record SessionInfo(int UserId, DateTime ExpiresAt, DateTime? RevokedAt);

public record AuthResult(
    bool Success,
    string? Error = null,
    UserAccount? User = null,
    string? Token = null);

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IUserStore
{
    Task<UserAccount?> FindByEmailAsync(string normalizedEmail);
    Task<UserAccount?> FindByIdAsync(int userId);
    Task AddAsync(UserAccount user);
    Task UpdateAsync(UserAccount user);
    Task LogAttemptAsync(int? userId, string emailTried, bool success, string? failureReason, string? ip);
    Task AddSessionAsync(int userId, string tokenHash, DateTime expiresAt);
    Task<SessionInfo?> FindSessionAsync(string tokenHash);
    Task RevokeSessionAsync(string tokenHash);
}