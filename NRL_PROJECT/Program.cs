using Microsoft.EntityFrameworkCore;
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
//
 builder.Services.AddDbContext<NRL_Db_Context>(options =>
    options.UseInMemoryDatabase("TestDb"));



// ------------------------------------------------------------
// BYGG APPEN
// ------------------------------------------------------------
var app = builder.Build();


// ------------------------------------------------------------
// TESTDATA
// ------------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NRL_Db_Context>();

    // --- 1️⃣ Seed AccessLevels ---
    if (!context.AccessLevels.Any())
    {
        context.AccessLevels.AddRange(
            new AccessLevel { AccessLevelName = "AdminLevel" },
            new AccessLevel { AccessLevelName = "UserLevel" }
        );
        context.SaveChanges();
    }

    // --- 2️⃣ Seed UserRoles ---
    if (!context.UserRoles.Any())
    {
        var adminLevel = context.AccessLevels.First(a => a.AccessLevelName == "AdminLevel").AccessLevelID;
        var userLevel = context.AccessLevels.First(a => a.AccessLevelName == "UserLevel").AccessLevelID;

        context.UserRoles.AddRange(
            new UserRole { RoleName = UserRoleType.Admin, AccessLevelID = adminLevel },
            new UserRole { RoleName = UserRoleType.Pilot, AccessLevelID = userLevel },
            new UserRole { RoleName = UserRoleType.Registrar, AccessLevelID = userLevel }
        );
        context.SaveChanges();
    }

    // --- 3️⃣ Seed Organisations ---
    if (!context.Organisations.Any())
    {
        context.Organisations.AddRange(
        new Organisation { OrgName = "TestOrg1", OrgContactEmail = "contact@testorg1.com" },
        new Organisation { OrgName = "TestOrg2", OrgContactEmail = "contact@testorg2.com" }
  );
        context.SaveChanges();
    }

    // --- 4️⃣ Seed Users ---
    if (!context.Users.Any())
    {
        var org1 = context.Organisations.First(o => o.OrgName == "TestOrg1").OrgID;
        var org2 = context.Organisations.First(o => o.OrgName == "TestOrg2").OrgID;
        var adminRole = context.UserRoles.First(r => r.RoleName == UserRoleType.Admin).RoleID;
        var pilotRole = context.UserRoles.First(r => r.RoleName == UserRoleType.Pilot).RoleID;
        var registrarRole = context.UserRoles.First(r => r.RoleName == UserRoleType.Registrar).RoleID;

        context.Users.AddRange(
            new User { FirstName = "Admin", LastName = "User", Email = "admin@example.com", PasswordHash = "dummyhash1", OrgID = org1, RoleID = adminRole },
            new User { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", PasswordHash = "dummyhash2", OrgID = org1, RoleID = pilotRole },
            new User { FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", PasswordHash = "dummyhash3", OrgID = org1, RoleID = registrarRole },
            new User { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", PasswordHash = "dummyhash4", OrgID = org2, RoleID = pilotRole },
            new User { FirstName = "Bob", LastName = "Brown", Email = "bob.brown@example.com", PasswordHash = "dummyhash5", OrgID = org2, RoleID = registrarRole }
        );
        context.SaveChanges();
    }

    // --- 5️⃣ Seed MapData ---
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
            GeoJsonCoordinates = @"{ ""type"": ""LineString"", ""coordinates"": [
            [10.7522, 59.9139],
            [11.7522, 58.9139]
        ]}",
            Coordinates = new List<MapCoordinate>
        {
            new MapCoordinate { Latitude = 59.9139, Longitude = 10.7522, OrderIndex = 0 },
            new MapCoordinate { Latitude = 58.9139, Longitude = 11.7522, OrderIndex = 1 }
        }
        };

        context.MapDatas.AddRange(map1, map2);
        context.SaveChanges();
    }

    // --- 6️⃣ Seed ObstacleData ---
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

        context.SaveChanges();
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

// Aktiver eventuell autorisasjon (hvis prosjektet bruker det)
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

