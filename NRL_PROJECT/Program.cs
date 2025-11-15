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

// Content security policy CSP
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    
    // Klæsjer med _LoginLayout
    //context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline';");
    
    // Add other headers as needed
    await next();
});

// DB-migrasjoner flyttet hit!
// ------------------------------------------------------------
// AUTOMATISK DATABASEMIGRERING VED OPPSTART
// - DENNE KOMMENTERES OGSÅ UT VED TESTING
// ------------------------------------------------------------
// Apply migrations at startup (Migrations + Seed Data)

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    try
    {
        // 1. Apply migrations
        var dbContext = services.GetRequiredService<NRL_Db_Context>();
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        
        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("✓ Database migrations applied successfully.");
        }
        else
        {
            Console.WriteLine("✓ Database is up to date.");
        }

        // 2. Create roles
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] { "Admin", "Pilot", "Registrar", "ExternalOrg" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (roleResult.Succeeded)
                {
                    Console.WriteLine($"✓ Role '{roleName}' created successfully.");
                }
                else
                {
                    throw new Exception($"Failed to create role '{roleName}': " +
                        $"{string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }
        }

        // 3. Create default admin user - HARDCODED FOR EXAM
        var userManager = services.GetRequiredService<UserManager<User>>();
        
        string adminEmail = builder.Configuration["AdminUser:Email"];
        string adminPassword = builder.Configuration["AdminUser:Password"];
        
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new User 
            { 
                UserName = adminEmail, 
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                OrgName = "NRL_Test"
            };


            var createResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (createResult.Succeeded)
            {
                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                
                if (roleResult.Succeeded)
                {
                    Console.WriteLine($"Admin user created: {adminEmail}");
                }
                else
                {
                    throw new Exception($"Failed to assign Admin role: " +
                        $"{string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                throw new Exception($"Failed to create admin user: " +
                    $"{string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine($"Admin user already exists: {adminEmail}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization failed: {ex.Message}");
        throw;
    }
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
