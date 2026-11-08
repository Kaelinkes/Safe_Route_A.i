using Microsoft.EntityFrameworkCore;
using SafeRouteAI.Application.Auth;
using SafeRouteAI.Domain.Entities;
using SafeRouteAI.Infrastructure.Data;

namespace SafeRouteAI.Infrastructure.Services;

public sealed class DevelopmentTestUserSeeder(
    AppDbContext db,
    IPasswordHasher passwordHasher)
{
    private static readonly (string Name, string Email, string Password, string Role)[]
        TestUsers =
        [
            ("SafeRoute Test User", "safertest.user@example.test", "TestUser123!", Roles.User),
            ("SafeRoute Test Admin", "safertest.admin@example.test", "TestAdmin123!", Roles.Admin)
        ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (name, email, password, role) in TestUsers)
        {
            var normalizedEmail = email.ToLowerInvariant();
            var exists = await db.Users.AnyAsync(
                user => user.Email.ToLower() == normalizedEmail,
                cancellationToken);

            if (exists)
            {
                continue;
            }

            db.Users.Add(new User
            {
                Name = name,
                Email = email,
                PasswordHash = passwordHasher.Hash(password),
                Role = role,
                DateRegistered = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
