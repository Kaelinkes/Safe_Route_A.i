
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SafeRouteAI.Application.Auth;
using Xunit;

namespace SafeRouteAI.API.Tests;

public class AuthApiTests
{
    [Fact]
    public async Task Register_ValidUser_Returns201AndSafeProfile()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await Register(client);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var json = await ReadJson(response);

        var root = json.RootElement;

        Assert.Equal("Test User", root.GetProperty("name").GetString());
        Assert.Equal("test@example.com", root.GetProperty("email").GetString());
        Assert.Equal("User", root.GetProperty("role").GetString());
        Assert.True(root.GetProperty("userId").GetInt32() > 0);

        var body = root.GetRawText();

        Assert.DoesNotContain("passwordHash", body,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", body,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var first = await Register(client);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await Register(client);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new
            {
                name = "Test User",
                email = "test@example.com",
                password = "weak"
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsBearerToken()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var registration = await Register(client);
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        var response = await Login(client);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = await ReadJson(response);

        var root = json.RootElement;

        Assert.Equal("Bearer",
            root.GetProperty("tokenType").GetString());

        Assert.Equal(28800,
            root.GetProperty("expiresIn").GetInt32());

        Assert.False(string.IsNullOrWhiteSpace(
            root.GetProperty("token").GetString()));

        Assert.Equal("test@example.com",
            root.GetProperty("user")
                .GetProperty("email").GetString());

        Assert.DoesNotContain(
            "passwordHash",
            root.GetRawText(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_InvalidPassword_Returns401()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var registration = await Register(client);
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new
            {
                email = "test@example.com",
                password = "WrongPassword123"
            });

        Assert.Equal(HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsProfile()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var token = await RegisterAndLogin(client);

        using var request = new HttpRequestMessage(
            HttpMethod.Get, "/api/v1/auth/me");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = await ReadJson(response);

        Assert.Equal("test@example.com",
            json.RootElement.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Me_WithInvalidToken_Returns401()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        using var request = new HttpRequestMessage(
            HttpMethod.Get, "/api/v1/auth/me");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", "invalid-session-token");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesToken()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var token = await RegisterAndLogin(client);

        using var logoutRequest = new HttpRequestMessage(
            HttpMethod.Post, "/api/v1/auth/logout");

        logoutRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var logoutResponse = await client.SendAsync(logoutRequest);

        Assert.Equal(HttpStatusCode.NoContent,
            logoutResponse.StatusCode);

        using var meRequest = new HttpRequestMessage(
            HttpMethod.Get, "/api/v1/auth/me");

        meRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var meResponse = await client.SendAsync(meRequest);

        Assert.Equal(HttpStatusCode.Unauthorized,
            meResponse.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IUserStore>();
                    services.AddSingleton<IUserStore, InMemoryUserStore>();
                });
            });
    }

    private static Task<HttpResponseMessage> Register(
        HttpClient client)
    {
        return client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new
            {
                name = "Test User",
                email = "test@example.com",
                password = "Password123"
            });
    }

    private static Task<HttpResponseMessage> Login(
        HttpClient client)
    {
        return client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new
            {
                email = "test@example.com",
                password = "Password123"
            });
    }

    private static async Task<string> RegisterAndLogin(
        HttpClient client)
    {
        var registration = await Register(client);

        Assert.Equal(HttpStatusCode.Created,
            registration.StatusCode);

        var login = await Login(client);

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        using var json = await ReadJson(login);

        return json.RootElement
            .GetProperty("token")
            .GetString()!;
    }

    private static async Task<JsonDocument> ReadJson(
        HttpResponseMessage response)
    {
        return JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
    }

    private sealed class InMemoryUserStore : IUserStore
    {
        private readonly ConcurrentDictionary<string, UserAccount>
            _usersByEmail = new();

        private readonly ConcurrentDictionary<int, UserAccount>
            _usersById = new();

        private readonly ConcurrentDictionary<string, SessionInfo>
            _sessions = new();

        private int _nextUserId;

        public Task<UserAccount?> FindByEmailAsync(
            string normalizedEmail)
        {
            _usersByEmail.TryGetValue(
                normalizedEmail, out var user);

            return Task.FromResult(user);
        }

        public Task<UserAccount?> FindByIdAsync(int userId)
        {
            _usersById.TryGetValue(userId, out var user);

            return Task.FromResult(user);
        }

        public Task AddAsync(UserAccount user)
        {
            user.UserId = Interlocked.Increment(
                ref _nextUserId);

            _usersById[user.UserId] = user;

            _usersByEmail[
                user.Email.Trim().ToLowerInvariant()] = user;

            return Task.CompletedTask;
        }

        public Task UpdateAsync(UserAccount user)
        {
            _usersById[user.UserId] = user;

            _usersByEmail[
                user.Email.Trim().ToLowerInvariant()] = user;

            return Task.CompletedTask;
        }

        public Task LogAttemptAsync(
            int? userId,
            string emailTried,
            bool success,
            string? failureReason,
            string? ip)
        {
            return Task.CompletedTask;
        }

        public Task AddSessionAsync(
            int userId,
            string tokenHash,
            DateTime expiresAt)
        {
            _sessions[tokenHash] = new SessionInfo(
                userId, expiresAt, null);

            return Task.CompletedTask;
        }

        public Task<SessionInfo?> FindSessionAsync(
            string tokenHash)
        {
            _sessions.TryGetValue(tokenHash, out var session);

            return Task.FromResult(session);
        }

        public Task RevokeSessionAsync(string tokenHash)
        {
            if (_sessions.TryGetValue(
                    tokenHash, out var session))
            {
                _sessions[tokenHash] = session with
                {
                    RevokedAt = DateTime.UtcNow
                };
            }

            return Task.CompletedTask;
        }
    }
}
