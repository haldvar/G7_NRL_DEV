using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// KONFIGURER TJENESTER (Dependency Injection)
// ------------------------------------------------------------

// Legg til støtte for MVC (Controllers + Views)
builder.Services.AddControllersWithViews();


// Registrer databasekontekst (Entity Framework + MySQL)

// KOMMENTERES UT UNDER TESTING:

/*
 builder.Services.AddDbContext<NRL_Db_Context>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});
*/


// BRUK DENNE (in-memory database i stedet for MySQL) VED TESTING: 

builder.Services.AddDbContext<NRL_Db_Context>(options =>
   options.UseInMemoryDatabase("TestDb"));

//-------------------------------------------------------------
// IDENTITY OG AUTHENTICATION SETUP
//-------------------------------------------------------------

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<NRL_Db_Context>() // EF
    .AddDefaultTokenProviders();

// Registrer e-posttjeneste (dummy for testing)
builder.Services.AddTransient<IEmailSender, AuthMessageSender>();


// ------------------------------------------------------------
// BYGG APPEN
// ------------------------------------------------------------
var app = builder.Build();

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

// Aktiver eventuell autorisasjon
app.UseAuthorization();

// ------------------------------------------------------------
// KONFIGURER STANDARD RUTE (MVC)
// ------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// ------------------------------------------------------------
// AUTOMATISK DATABASEMIGRERING VED OPPSTART
// - DENNE KOMMENTERES OGSÅ UT VED TESTING
// ------------------------------------------------------------

/*
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NRL_Db_Context>();
    db.Database.Migrate(); // Oppretter/oppdaterer databasen hvis nødvendig
}
*/

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
