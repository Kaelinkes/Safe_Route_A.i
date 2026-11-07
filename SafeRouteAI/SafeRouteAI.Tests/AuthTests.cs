using SafeRouteAI.Application.Auth;
using SafeRouteAI.Infrastructure.Services;

public class AuthTests
{
    private class FakeHasher : IPasswordHasher
    {
        public string Hash(string p) => "h:" + p;
        public bool Verify(string p, string h) => h == "h:" + p;
    }

    private class FakeClock(DateTime start) : TimeProvider
    {
        public DateTime Now { get; set; } = start;
        public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(Now, DateTimeKind.Utc));
    }

    private class FakeStore : IUserStore
    {
        public List<UserAccount> Users { get; } = new();
        private readonly Dictionary<string, SessionInfo> _sessions = new();

        public Task<UserAccount?> FindByEmailAsync(string e) =>
            Task.FromResult(Users.FirstOrDefault(u => u.Email.ToLowerInvariant() == e));
        public Task<UserAccount?> FindByIdAsync(int id) => Task.FromResult(Users.FirstOrDefault(u => u.UserId == id));
        public Task AddAsync(UserAccount u) { u.UserId = Users.Count + 1; Users.Add(u); return Task.CompletedTask; }
        public Task UpdateAsync(UserAccount u) => Task.CompletedTask;
        public Task LogAttemptAsync(int? id, string email, bool ok, string? reason, string? ip) => Task.CompletedTask;
        public Task AddSessionAsync(int id, string hash, DateTime exp) { _sessions[hash] = new(id, exp, null); return Task.CompletedTask; }
        public Task<SessionInfo?> FindSessionAsync(string hash) => Task.FromResult(_sessions.GetValueOrDefault(hash));
        public Task RevokeSessionAsync(string hash)
        {
            if (_sessions.TryGetValue(hash, out var s)) _sessions[hash] = s with { RevokedAt = DateTime.UtcNow };
            return Task.CompletedTask;
        }
    }

    private static (AuthService Auth, FakeStore Store, FakeClock Clock) Create()
    {
        var store = new FakeStore();
        var clock = new FakeClock(new DateTime(2026, 11, 5, 8, 0, 0));
        return (new AuthService(store, new FakeHasher(), clock), store, clock);
    }

    [Theory]
    [InlineData("short1")]
    [InlineData("nodigitshere")]
    [InlineData("12345678")]
    public async Task Weak_passwords_are_rejected(string password)
    {
        var (auth, _, _) = Create();

        Assert.False((await auth.RegisterAsync("Thabang", "t@example.com", password)).Success);
    }

    [Fact]
    public async Task Register_stores_a_hash_and_a_normal_user_role()
    {
        var (auth, store, _) = Create();

        Assert.True((await auth.RegisterAsync("Thabang", "T@Example.com", "Password1")).Success);

        var user = Assert.Single(store.Users);
        Assert.NotEqual("Password1", user.PasswordHash);
        Assert.Equal(Roles.User, user.Role);
    }

    [Fact]
    public async Task Duplicate_email_is_rejected_ignoring_case()
    {
        var (auth, _, _) = Create();
        await auth.RegisterAsync("A", "t@example.com", "Password1");

        Assert.False((await auth.RegisterAsync("B", "T@EXAMPLE.COM", "Password1")).Success);
    }

    [Fact]
    public async Task Correct_login_returns_a_token()
    {
        var (auth, _, _) = Create();
        await auth.RegisterAsync("Thabang", "t@example.com", "Password1");

        var result = await auth.LoginAsync("t@example.com", "Password1");

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public async Task Unknown_email_and_wrong_password_give_the_same_message()
    {
        var (auth, _, _) = Create();
        await auth.RegisterAsync("Thabang", "t@example.com", "Password1");

        var wrong = await auth.LoginAsync("t@example.com", "Wrong123");
        var unknown = await auth.LoginAsync("nobody@example.com", "Password1");

        Assert.Equal(wrong.Error, unknown.Error);
    }

    [Fact]
    public async Task Five_failures_lock_the_account_for_fifteen_minutes()
    {
        var (auth, _, clock) = Create();
        await auth.RegisterAsync("Thabang", "t@example.com", "Password1");

        for (var i = 0; i < 5; i++) await auth.LoginAsync("t@example.com", "Wrong123");

        Assert.False((await auth.LoginAsync("t@example.com", "Password1")).Success); // still locked
        clock.Now = clock.Now.AddMinutes(16);
        Assert.True((await auth.LoginAsync("t@example.com", "Password1")).Success);
    }

    [Fact]
    public async Task Session_is_valid_until_logout()
    {
        var (auth, _, _) = Create();
        await auth.RegisterAsync("Thabang", "t@example.com", "Password1");
        var token = (await auth.LoginAsync("t@example.com", "Password1")).Token!;

        Assert.NotNull(await auth.ValidateSessionAsync(token));
        await auth.LogoutAsync(token);
        Assert.Null(await auth.ValidateSessionAsync(token));
    }

    [Fact]
    public async Task Session_expires_after_eight_hours()
    {
        var (auth, _, clock) = Create();
        await auth.RegisterAsync("Thabang", "t@example.com", "Password1");
        var token = (await auth.LoginAsync("t@example.com", "Password1")).Token!;

        clock.Now = clock.Now.AddHours(9);

        Assert.Null(await auth.ValidateSessionAsync(token));
    }

    [Fact]
    public void Real_hasher_salts_and_verifies()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var a = hasher.Hash("Password1");

        Assert.NotEqual(a, hasher.Hash("Password1"));
        Assert.True(hasher.Verify("Password1", a));
        Assert.False(hasher.Verify("Wrong123", a));
    }
}