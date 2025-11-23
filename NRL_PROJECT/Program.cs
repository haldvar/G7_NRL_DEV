using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------
// Service registration (Dependency Injection)
// -----------------------------------------------------------------

// Add MVC (Controllers + Views)
// Apply a global authorization policy that requires authenticated users
// unless [AllowAnonymous] is used on specific actions.
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// Hide server header for slightly improved security posture
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

// -----------------------------------------------------------------
// Database configuration (EF Core + MySQL)
// -----------------------------------------------------------------
// NOTE: The MySQL registration is active. Switch to the in-memory option
// below only during tests as indicated.
builder.Services.AddDbContext<NRL_Db_Context>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()); // retry logic for robustness
});

// BRING THIS BLOCK BACK DURING UNIT/INTEGRATION TESTING if you prefer an in-memory DB:
/*
builder.Services.AddDbContext<NRL_Db_Context>(options =>
    options.UseInMemoryDatabase("TestDb"));
*/

// -----------------------------------------------------------------
// Identity configuration
// -----------------------------------------------------------------
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = false;

        // Sign-in settings
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedAccount = false;

        // User settings
        options.User.RequireUniqueEmail = true;

        // Password complexity
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<NRL_Db_Context>()
    .AddDefaultTokenProviders();

// Configure application cookie paths and expiration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
});

// Register a transient email sender (dummy implementation used in Program.cs)
builder.Services.AddTransient<IEmailSender, AuthMessageSender>();

// -----------------------------------------------------------------
// Build the app
// -----------------------------------------------------------------
var app = builder.Build();

// -----------------------------------------------------------------
// Seeding (run within a scoped service provider)
// -----------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedAsync(scope.ServiceProvider, builder.Configuration);
}

// -----------------------------------------------------------------
// Security headers middleware
// - Adds a small set of HTTP headers to improve security posture.
// - Keep CSP restrictive but allow the CDN/script sources used by the app.
// -----------------------------------------------------------------
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");

    // Content-Security-Policy: restrict as tightly as possible while allowing necessary CDNs and tile providers.
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.tailwindcss.com https://unpkg.com https://rawcdn.githack.com; " +
        "style-src 'self' 'unsafe-inline' https://unpkg.com; " +
        "img-src 'self' data: https://wms.geonorge.no https://*.tile.openstreetmap.org https://unpkg.com;");
    await next();
});

// -----------------------------------------------------------------
// Middleware pipeline configuration
// -----------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    // Use custom error handling in production
    app.UseExceptionHandler("/Home/Error");

    // Enable HSTS in non-development environments
    app.UseHsts();
}

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Serve static files from wwwroot (CSS, JS, images)
app.UseStaticFiles();

// Routing
app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// -----------------------------------------------------------------
// MVC route configuration
// -----------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
);

// -----------------------------------------------------------------
// Run the application
// -----------------------------------------------------------------
app.Run();

// -----------------------------------------------------------------
// Simple email sender used for development/testing (console sink).
// Keeps IEmailSender registered above functional without external SMTP.
// -----------------------------------------------------------------
public class AuthMessageSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine(email);
        Console.WriteLine(subject);
        Console.WriteLine(htmlMessage);
        return Task.CompletedTask;
    }
}
