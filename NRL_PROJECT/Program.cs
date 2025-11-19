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

// Legg til støtte for MVC (Controllers + Views)
// + Gjør alle sidene/funksjonene utilgjengelig by default, utenom der [AllowAnonymous] finnes (Login)
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// hide server header
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

// Database configuration (Entity Framework + MySQL)
// KOMMENTERES UT UNDER TESTING:

 builder.Services.AddDbContext<NRL_Db_Context>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()); // retry logic - robustness
});

// BRUK DENNE (in-memory database i stedet for MySQL) VED TESTING: 
/*
 builder.Services.AddDbContext<NRL_Db_Context>(options =>
    options.UseInMemoryDatabase("TestDb"));
*/

//-------------------------------------------------------------
// Identity configuration
//-------------------------------------------------------------

 builder.Services.AddIdentity<User, IdentityRole>(options =>
     {
         // Lockout settings
         options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
         options.Lockout.MaxFailedAccessAttempts = 5;
         options.Lockout.AllowedForNewUsers = false;
         // Sign in settings
         options.SignIn.RequireConfirmedPhoneNumber = false;
         options.SignIn.RequireConfirmedEmail = false;
         options.SignIn.RequireConfirmedAccount = false;
         // User settings
         options.User.RequireUniqueEmail = true;
         // Password settings
         options.Password.RequireDigit = true;
         options.Password.RequireLowercase = true;
         options.Password.RequireUppercase = true;
         options.Password.RequireNonAlphanumeric = true;
         options.Password.RequiredLength = 8;
     })
     .AddEntityFrameworkStores<NRL_Db_Context>()
     .AddDefaultTokenProviders();

// Configure application cookie
 builder.Services.ConfigureApplicationCookie(options =>
 {
     options.LoginPath = "/Account/Login";
     options.LogoutPath = "/Account/Logout";
     options.AccessDeniedPath = "/Account/AccessDenied";
     options.ExpireTimeSpan = TimeSpan.FromHours(2);
     options.SlidingExpiration = true;
 });

// Registrer e-posttjeneste (dummy for testing)

 builder.Services.AddTransient<IEmailSender, AuthMessageSender>();


// ------------------------------------------------------------
// BYGG APPEN
// ------------------------------------------------------------
var app = builder.Build();

// // Content security policy CSP
// app.Use(async (context, next) =>
// {
//     context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
//     context.Response.Headers.Add("X-Frame-Options", "DENY");
//     context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
//     context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
//     context.Response.Headers.Add("Referrer-Policy", "no-referrer");
//     
//     // Klæsjer med _LoginLayout
//     //context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline';");
//     
//     // Add other headers as needed
//     await next();
// });

// ------------------------------------------------------------
// SEEDING
// ------------------------------------------------------------


using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedAsync(scope.ServiceProvider, builder.Configuration);
}
// ------------------------------------------------------------
// KONFIGURER MIDDLEWARE (HTTP request pipeline)
// ------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    // Bruk en egen feilhåndteringsside i produksjon
    app.UseExceptionHandler("/Home/Error");

    // Aktiver HSTS (sikkerhetsheader for HTTPS)
    app.UseHsts();
}

// Tving all trafikk til HTTPS
app.UseHttpsRedirection();

// Gjør wwwroot-innhold tilgjengelig (CSS, JS, bilder osv.)
app.UseStaticFiles();

// Aktiver ruting (slik at /Home/Index m.m. fungerer)
app.UseRouting();

// Legger til Authentication
app.UseAuthentication();

// Aktiver eventuell autorisasjon (hvis prosjektet bruker det)
app.UseAuthorization();

// ------------------------------------------------------------
// KONFIGURER STANDARD RUTE (MVC)
// ------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
);


// ------------------------------------------------------------
// KJØR APPEN
// ------------------------------------------------------------

app.Run();

//-------------------------------------------------------------
// EPOST TJENESTE IMPLEMENTASJON
//-------------------------------------------------------------

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
