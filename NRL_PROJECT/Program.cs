using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// KONFIGURER TJENESTER (Dependency Injection)
// ------------------------------------------------------------

// MVC + Global Authorize filter (krever login, unntatt [AllowAnonymous])
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// Skjul serverheader for sikkerhet
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

// Database: MySQL (prod)
builder.Services.AddDbContext<NRL_Db_Context>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure());
});

// In-memory DB (kun til testing – av)
/*
builder.Services.AddDbContext<NRL_Db_Context>(options =>
    options.UseInMemoryDatabase("TestDb"));
*/

// ------------------------------------------------------------
// Identity-konfigurasjon
// ------------------------------------------------------------
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Sign-in
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;

    // Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = false;

    // Users
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<NRL_Db_Context>()
.AddDefaultTokenProviders();

// Cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
});

// Dummy e-posttjeneste (brukes under utvikling)
builder.Services.AddTransient<IEmailSender, AuthMessageSender>();

// ------------------------------------------------------------
// BYGG APPEN
// ------------------------------------------------------------
var app = builder.Build();

// ------------------------------------------------------------
// DATABASE-SEEDING (DataSeeder.cs)
// ------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedAsync(scope.ServiceProvider, builder.Configuration);
}

// ------------------------------------------------------------
// SIKKERHETSHEADERE
// ------------------------------------------------------------
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");

    await next();
});

// ------------------------------------------------------------
// HTTP-Pipeline
// ------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ------------------------------------------------------------
// STANDARD RUTE
// ------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
);

// ------------------------------------------------------------
// KJØR APPEN
// ------------------------------------------------------------
app.Run();

// ------------------------------------------------------------
// EPOST TJENESTE IMPLEMENTASJON
// ------------------------------------------------------------
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
