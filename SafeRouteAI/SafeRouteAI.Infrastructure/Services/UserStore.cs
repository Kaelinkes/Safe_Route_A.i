using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SafeRouteAI.Application.Auth;
using SafeRouteAI.Domain.Entities;
using SafeRouteAI.Infrastructure.Data;

namespace SafeRouteAI.Infrastructure.Services;

public record LoginState(int FailedLogins, DateTime? LockedUntil, DateTime? LastLoginAt);

// The users table has no lockout or session columns, so these are kept in memory
public class AuthMemoryState
{
    public ConcurrentDictionary<int, LoginState> Logins { get; } = new();
    public ConcurrentDictionary<string, SessionInfo> Sessions { get; } = new();
}

public class UserStore(AppDbContext db, AuthMemoryState state, ILogger<UserStore> log) : IUserStore
{
    public async Task<UserAccount?> FindByEmailAsync(string normalizedEmail)
    {
        var u = await db.Set<User>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);
        return u is null ? null : ToAccount(u);
    }

    public async Task<UserAccount?> FindByIdAsync(int userId)
    {
        var u = await db.Set<User>().AsNoTracking().FirstOrDefaultAsync(x => x.UserID == userId);
        return u is null ? null : ToAccount(u);
    }

    public async Task AddAsync(UserAccount user)
    {
        var entity = new User
        {
            Name = user.Name,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            Role = user.Role,
            DateRegistered = DateTime.UtcNow
        };
        db.Set<User>().Add(entity);
        await db.SaveChangesAsync();
        user.UserId = entity.UserID;
    }

    // After sign-up only the lockout counters change, and they live in memory
    public Task UpdateAsync(UserAccount user)
    {
        state.Logins[user.UserId] = new LoginState(user.FailedLogins, user.LockedUntil, user.LastLoginAt);
        return Task.CompletedTask;
    }

    public Task LogAttemptAsync(int? userId, string emailTried, bool success, string? failureReason, string? ip)
    {
        log.LogInformation("Login attempt: user {UserId}, success {Success}, reason {Reason}", userId, success, failureReason);
        return Task.CompletedTask;
    }

    public Task AddSessionAsync(int userId, string tokenHash, DateTime expiresAt)
    {
        state.Sessions[tokenHash] = new SessionInfo(userId, expiresAt, null);
        return Task.CompletedTask;
    }

    public Task<SessionInfo?> FindSessionAsync(string tokenHash)
    {
        state.Sessions.TryGetValue(tokenHash, out var session);
        return Task.FromResult(session);
    }

    public Task RevokeSessionAsync(string tokenHash)
    {
        if (state.Sessions.TryGetValue(tokenHash, out var s))
            state.Sessions[tokenHash] = s with { RevokedAt = DateTime.UtcNow };
        return Task.CompletedTask;
    }

    private UserAccount ToAccount(User u)
    {
        state.Logins.TryGetValue(u.UserID, out var s);
        return new UserAccount
        {
            UserId = u.UserID,
            Name = u.Name,
            Email = u.Email,
            PasswordHash = u.PasswordHash,
            Role = u.Role,
            FailedLogins = s?.FailedLogins ?? 0,
            LockedUntil = s?.LockedUntil,
            LastLoginAt = s?.LastLoginAt
        };
    }
}