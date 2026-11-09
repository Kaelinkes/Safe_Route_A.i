using SafeRouteAI.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = false;
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpClient<SafeRouteApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);

    var configuredAddress = builder.Configuration["SafeRouteApi:BaseAddress"];
    if (Uri.TryCreate(configuredAddress, UriKind.Absolute, out var baseAddress) &&
        (baseAddress.Scheme == Uri.UriSchemeHttp ||
         baseAddress.Scheme == Uri.UriSchemeHttps))
    {
        var normalizedAddress = baseAddress.AbsoluteUri.TrimEnd('/') + "/";
        client.BaseAddress = new Uri(normalizedAddress, UriKind.Absolute);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
