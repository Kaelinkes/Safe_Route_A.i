using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace SafeRouteAI.Application.Auth;

public class AuthService(IUserStore store, IPasswordHasher hasher, TimeProvider clock)
{
    private const int MaxFailedLogins = 5;
    private const string BadLogin = "Invalid email or password.";
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<AuthResult> RegisterAsync(string name, string email, string password)
    {
        name = name?.Trim() ?? "";
        email = email?.Trim() ?? "";

        if (name.Length is 0 or > 100)
        {
            return new(false, "Name is required (max 100 characters).");
        }

        if (email.Length > 150 || !MailAddress.TryCreate(email, out var address) || address.Address != email)
        {
            return new(false, "Enter a valid email address.");
        }

        if (!IsStrong(password))
        {
            return new(false, "Password must be 8-128 characters with at least one letter and one digit.");
        }

        if (await store.FindByEmailAsync(Normalize(email)) is not null)
        {
            return new(false, "An account with this email already exists.");
        }

        var user = new UserAccount { Name = name, Email = email, PasswordHash = hasher.Hash(password) };
        await store.AddAsync(user);
        return new(true, User: user);
    }

    public async Task<AuthResult> LoginAsync(string email, string password, string? ip = null)
    {
        var user = await store.FindByEmailAsync(Normalize(email));

        if (user is null)
        {
            return await Fail(null, email, "UnknownUser", BadLogin, ip);
        }

        if (user.Status != AccountStatus.Active)
        {
            return await Fail(user.UserId, email, "AccountDisabled", BadLogin, ip);
        }

        if (user.LockedUntil > Now)
        {
            return await Fail(user.UserId, email, "Locked", "Account temporarily locked. Try again later.", ip);
        }

        if (!hasher.Verify(password ?? "", user.PasswordHash))
        {
            user.FailedLogins++;
            if (user.FailedLogins >= MaxFailedLogins)
            {
                user.LockedUntil = Now + LockDuration;
                user.FailedLogins = 0;
            }

            await store.UpdateAsync(user);
            return await Fail(user.UserId, email, "BadPassword", BadLogin, ip);
        }

        user.FailedLogins = 0;
        user.LockedUntil = null;
        user.LastLoginAt = Now;
        await store.UpdateAsync(user);

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        await store.AddSessionAsync(user.UserId, HashToken(token), Now + SessionLifetime);
        await store.LogAttemptAsync(user.UserId, email, true, null, ip);
        return new(true, User: user, Token: token);
    }

    public async Task<UserAccount?> ValidateSessionAsync(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        var session = await store.FindSessionAsync(HashToken(token));
        if (session is null || session.RevokedAt is not null || session.ExpiresAt <= Now)
        {
            return null;
        }

        var user = await store.FindByIdAsync(session.UserId);
        return user is { Status: AccountStatus.Active } ? user : null;
    }

    public Task LogoutAsync(string token) => store.RevokeSessionAsync(HashToken(token));

    private async Task<AuthResult> Fail(
        int? userId,
        string email,
        string reason,
        string message,
        string? ip)
    {
        await store.LogAttemptAsync(userId, email, false, reason, ip);
        return new(false, message);
    }

    private static bool IsStrong(string? password) =>
        password is { Length: >= 8 and <= 128 } &&
        password.Any(char.IsLetter) &&
        password.Any(char.IsDigit);

    private static string Normalize(string? email) => (email ?? "").Trim().ToLowerInvariant();

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
