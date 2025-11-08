using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// KONFIGURER TJENESTER (Dependency Injection)
// ------------------------------------------------------------

builder.Services.AddControllersWithViews();

// Registrer databasekontekst (Entity Framework + MySQL)
builder.Services.AddDbContext<NRL_Db_Context>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Konfigurer Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Passordinstillinger
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
    
    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<NRL_Db_Context>()
.AddDefaultTokenProviders();

// Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

/*
// TESTING: Use in-memory database
builder.Services.AddDbContext<NRL_Db_Context>(options =>
   options.UseInMemoryDatabase("TestDb"));
*/

// ------------------------------------------------------------
// BYGG APPEN
// ------------------------------------------------------------
var app = builder.Build();

// ------------------------------------------------------------
// DATABASE MIGRATION & SEEDING
// ------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    try
    {
        // 1. Run migrations
        var db = services.GetRequiredService<NRL_Db_Context>();
        db.Database.Migrate();
        
        // 2. Seed Identity roles
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = { "Admin", "Pilot", "Registrar" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"[SEED] Created role: {role}");
            }
        }
        
        // 3. Seed default admin user
        var userManager = services.GetRequiredService<UserManager<User>>();
        await SeedDefaultAdmin(userManager);
        
        // 4. Seed test data (AccessLevels, Organisations, etc.)
        await SeedTestData(db, userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// ------------------------------------------------------------
// KONFIGURER MIDDLEWARE
// ------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// CRITICAL: Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
);

app.Run();

// ------------------------------------------------------------
// SEED DEFAULT ADMIN
// ------------------------------------------------------------
async Task SeedDefaultAdmin(UserManager<User> userManager)
{
    var adminEmail = "admin@nrl.no";
    
    Console.WriteLine($"[SEED] Checking for admin user: {adminEmail}");
    var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
    
    if (existingAdmin == null)
    {
        Console.WriteLine("[SEED] Admin not found, creating new admin user...");
        
        var admin = new User
        {
            UserName = "admin",
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(admin, "Admin@123!");
        
        if (result.Succeeded)
        {
            Console.WriteLine("[SEED] ✅ Admin user created successfully!");
            
            var roleResult = await userManager.AddToRoleAsync(admin, "Admin");
            
            if (roleResult.Succeeded)
            {
                Console.WriteLine("[SEED] ✅ Admin role assigned successfully!");
            }
            else
            {
                Console.WriteLine("[SEED] ❌ Failed to assign Admin role:");
                foreach (var error in roleResult.Errors)
                {
                    Console.WriteLine($"[SEED]   - {error.Description}");
                }
            }
        }
        else
        {
            Console.WriteLine("[SEED] ❌ Failed to create admin user:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"[SEED]   - {error.Description}");
            }
        }
    }
    else
    {
        Console.WriteLine($"[SEED] Admin user already exists: {existingAdmin.UserName}");
        
        var hasAdminRole = await userManager.IsInRoleAsync(existingAdmin, "Admin");
        
        if (!hasAdminRole)
        {
            Console.WriteLine("[SEED] ⚠️  Admin user exists but doesn't have Admin role, fixing...");
            
            var currentRoles = await userManager.GetRolesAsync(existingAdmin);
            if (currentRoles.Any())
            {
                await userManager.RemoveFromRolesAsync(existingAdmin, currentRoles);
                Console.WriteLine($"[SEED] Removed old roles: {string.Join(", ", currentRoles)}");
            }
            
            var roleResult = await userManager.AddToRoleAsync(existingAdmin, "Admin");
            
            if (roleResult.Succeeded)
            {
                Console.WriteLine("[SEED] ✅ Admin role assigned to existing user!");
            }
        }
        else
        {
            Console.WriteLine("[SEED] ✅ Admin user already has Admin role.");
        }
    }
}

// ------------------------------------------------------------
// SEED TEST DATA
// ------------------------------------------------------------
async Task SeedTestData(NRL_Db_Context context, UserManager<User> userManager)
{
    Console.WriteLine("[SEED] Starting test data seeding...");
    
    // Seed AccessLevels
    if (!context.AccessLevels.Any())
    {
        context.AccessLevels.AddRange(
            new AccessLevel { AccessLevelName = "AdminLevel" },
            new AccessLevel { AccessLevelName = "UserLevel" }
        );
        await context.SaveChangesAsync();
        Console.WriteLine("[SEED] ✅ AccessLevels created");
    }

    // Seed UserRoles (your custom table)
    if (!context.UserRoles.Any())
    {
        var adminLevel = context.AccessLevels.First(a => a.AccessLevelName == "AdminLevel").AccessLevelID;
        var userLevel = context.AccessLevels.First(a => a.AccessLevelName == "UserLevel").AccessLevelID;

        context.UserRoles.AddRange(
            new UserRole { RoleName = UserRoleType.Admin, AccessLevelID = adminLevel },
            new UserRole { RoleName = UserRoleType.Pilot, AccessLevelID = userLevel },
            new UserRole { RoleName = UserRoleType.Registrar, AccessLevelID = userLevel }
        );
        await context.SaveChangesAsync();
        Console.WriteLine("[SEED] ✅ UserRoles created");
    }

    // Seed Organisations
    if (!context.Organisations.Any())
    {
        context.Organisations.AddRange(
            new Organisation { OrgName = "TestOrg1", OrgContactEmail = "contact@testorg1.com" },
            new Organisation { OrgName = "TestOrg2", OrgContactEmail = "contact@testorg2.com" }
        );
        await context.SaveChangesAsync();
        Console.WriteLine("[SEED] ✅ Organisations created");
    }

    // Seed Test Users with Identity
    await SeedTestUsers(context, userManager);

    // Seed MapData
    if (!context.MapDatas.Any())
    {
        var map1 = new MapData
        {
            GeometryType = "Point",
            MapZoomLevel = 12,
            GeoJsonCoordinates = @"{ ""type"": ""Point"", ""coordinates"": [5.3221, 60.3913] }",
            Coordinates = new List<MapCoordinate>
            {
                new MapCoordinate { Latitude = 60.3913, Longitude = 5.3221, OrderIndex = 0 }
            }
        };

        var map2 = new MapData
        {
            GeometryType = "LineString",
            MapZoomLevel = 12,
            GeoJsonCoordinates = @"{ ""type"": ""LineString"", ""coordinates"": [[10.7522, 59.9139], [11.7522, 58.9139]]}",
            Coordinates = new List<MapCoordinate>
            {
                new MapCoordinate { Latitude = 59.9139, Longitude = 10.7522, OrderIndex = 0 },
                new MapCoordinate { Latitude = 58.9139, Longitude = 11.7522, OrderIndex = 1 }
            }
        };

        context.MapDatas.AddRange(map1, map2);
        await context.SaveChangesAsync();
        Console.WriteLine("[SEED] ✅ MapData created");
    }

    // Seed Obstacles
    if (!context.Obstacles.Any())
    {
        var map1 = context.MapDatas.Include(m => m.Coordinates).First();
        var map2 = context.MapDatas.Include(m => m.Coordinates).Skip(1).First();

        context.Obstacles.AddRange(
            new ObstacleData
            {
                ObstacleType = "Tree",
                ObstacleHeight = 5,
                ObstacleWidth = 2,
                ObstacleComment = "Fallen tree near path",
                MapData = map1
            },
            new ObstacleData
            {
                ObstacleType = "Fence",
                ObstacleHeight = 1.5,
                ObstacleWidth = 10,
                ObstacleComment = "New fence blocking small road",
                MapData = map2
            }
        );
        await context.SaveChangesAsync();
        Console.WriteLine("[SEED] ✅ Obstacles created");
    }
    
    Console.WriteLine("[SEED] Test data seeding complete!");
}

// Seed test users using Identity
async Task SeedTestUsers(NRL_Db_Context context, UserManager<User> userManager)
{
    var org1 = context.Organisations.First(o => o.OrgName == "TestOrg1").OrgID;
    var pilotRole = context.UserRoles.First(r => r.RoleName == UserRoleType.Pilot).RoleID;
    var registrarRole = context.UserRoles.First(r => r.RoleName == UserRoleType.Registrar).RoleID;

    // Create test pilot
    var pilot = await userManager.FindByEmailAsync("john.doe@example.com");
    if (pilot == null)
    {
        pilot = new User
        {
            UserName = "johndoe",
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe",
            OrgID = org1,
            RoleID = pilotRole,
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(pilot, "Test@123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(pilot, "Pilot");
            Console.WriteLine("[SEED] ✅ Test pilot created");
        }
    }

    // Create test registrar
    var registrar = await userManager.FindByEmailAsync("jane.smith@example.com");
    if (registrar == null)
    {
        registrar = new User
        {
            UserName = "janesmith",
            Email = "jane.smith@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            OrgID = org1,
            RoleID = registrarRole,
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(registrar, "Test@123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(registrar, "Registrar");
            Console.WriteLine("[SEED] ✅ Test registrar created");
        }
    }
}
