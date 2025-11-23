using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Models;
using System.Text.Json;

namespace NRL_PROJECT.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<NRL_Db_Context>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            await context.Database.MigrateAsync();

            await SeedRoles(roleManager);
            await SeedOrganisations(context);
            await SeedAdminUser(userManager, config, context);
            await SeedDemoPilots(userManager, context);
            await SeedDemoRegistrars(userManager, context);
            await SeedDemoExternalOrgUsers(userManager, context);
            await SeedDemoReports(context, userManager);
        }

        // --------------------------------------------------------------------
        // 1. Roller
        // --------------------------------------------------------------------
        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Pilot", "Registrar", "ExternalOrg" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // --------------------------------------------------------------------
        // 2. Organisasjoner
        // --------------------------------------------------------------------
        private static async Task SeedOrganisations(NRL_Db_Context context)
        {
            if (await context.Organisations.AnyAsync())
                return;

            var organisations = new List<Organisation>
            {
                new Organisation { OrgID = 1, OrgName = "NRL", OrgContactEmail = "kontakt@nrl.no" },
                new Organisation { OrgID = 2, OrgName = "Politiet", OrgContactEmail = "kontakt@politiet.no" },
                new Organisation { OrgID = 3, OrgName = "Forsvaret", OrgContactEmail = "kontakt@forsvaret.no" },
                new Organisation { OrgID = 4, OrgName = "Norsk Luftambulanse", OrgContactEmail = "kontakt@nla.no" }
            };

            context.Organisations.AddRange(organisations);
            await context.SaveChangesAsync();
        }

        // --------------------------------------------------------------------
        // 3. Admin-bruker
        // --------------------------------------------------------------------
        private static async Task SeedAdminUser(
            UserManager<User> userManager,
            IConfiguration config,
            NRL_Db_Context context)
        {
            string adminEmail = config["AdminUser:Email"];
            string adminPassword = config["AdminUser:Password"];

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin != null)
                return;

            var adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                OrgID = 1
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (createResult.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // --------------------------------------------------------------------
        // 4. Piloter
        // --------------------------------------------------------------------
        private static async Task SeedDemoPilots(UserManager<User> userManager, NRL_Db_Context context)
        {
            if (await userManager.Users.AnyAsync(u => u.UserName.StartsWith("pilot")))
                return;

            var organisations = await context.Organisations.OrderBy(o => o.OrgID).ToListAsync();
            int orgCount = organisations.Count;

            for (int i = 1; i <= 3; i++)
            {
                var assignedOrg = organisations[(i - 1) % orgCount];

                var user = new User
                {
                    UserName = $"pilot{i}",
                    Email = $"pilot{i}@test.no",
                    EmailConfirmed = true,
                    FirstName = "Pilot",
                    LastName = $"{i}",
                    OrgID = assignedOrg.OrgID
                };

                var result = await userManager.CreateAsync(user, "Heisann1!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, "Pilot");
            }
        }

        // --------------------------------------------------------------------
        // 5. Registrare
        // --------------------------------------------------------------------
        private static async Task SeedDemoRegistrars(UserManager<User> userManager, NRL_Db_Context context)
        {
            if (await userManager.Users.AnyAsync(u => u.UserName.StartsWith("reg")))
                return;

            for (int i = 1; i <= 2; i++)
            {
                var user = new User
                {
                    UserName = $"reg{i}",
                    Email = $"reg{i}@test.no",
                    EmailConfirmed = true,
                    FirstName = "Registrar",
                    LastName = $"{i}",
                    OrgID = 1
                };

                var result = await userManager.CreateAsync(user, "Heisann1!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, "Registrar");
            }
        }

        // --------------------------------------------------------------------
        // 6. Eksterne organisasjonsbrukere
        // --------------------------------------------------------------------
        private static async Task SeedDemoExternalOrgUsers(UserManager<User> userManager, NRL_Db_Context context)
        {
            if (await userManager.Users.AnyAsync(u => u.UserName.StartsWith("ext")))
                return;

            var orgs = await context.Organisations.ToListAsync();

            foreach (var org in orgs)
            {
                var clean = org.OrgName.Replace(" ", "").Replace("ø", "o").Replace("å", "a").Replace("æ", "ae");

                for (int i = 1; i <= 2; i++)
                {
                    var user = new User
                    {
                        UserName = $"ext{i}_{clean}",
                        Email = $"ext{i}_{clean}@test.no",
                        EmailConfirmed = true,
                        FirstName = "Ekstern",
                        LastName = org.OrgName,
                        OrgID = org.OrgID
                    };

                    var result = await userManager.CreateAsync(user, "Heisann1!");
                    if (result.Succeeded)
                        await userManager.AddToRoleAsync(user, "ExternalOrg");
                }
            }
        }

        // --------------------------------------------------------------------
        // 7. Demo-hinderrapporter
        // --------------------------------------------------------------------
        private static async Task SeedDemoReports(NRL_Db_Context context, UserManager<User> userManager)
        {
            if (await context.ObstacleReports.AnyAsync())
                return;

            var random = new Random();
            var pilots = await userManager.GetUsersInRoleAsync("Pilot");
            var registrars = await userManager.GetUsersInRoleAsync("Registrar");

            string[] obstacleTypes =
            {
                "Radio-/Mobilmast",
                "Kran",
                "Bygning/Konstruksjon",
                "Vindmølle",
                "Høyspentlinje",
                "Tårn"
            };

            var statusValues = new[]
            {
                ObstacleReportData.EnumTypes.New,
                ObstacleReportData.EnumTypes.Open,
                ObstacleReportData.EnumTypes.InProgress,
                ObstacleReportData.EnumTypes.Resolved,
                ObstacleReportData.EnumTypes.Deleted
            };

           
            // Ulike områder i Agder (INGEN byer utenfor regionen)
            var baseLocations = new (double Lat, double Lng)[]
            {
                (58.2040, 8.0855), // Kjevik
                (58.1467, 7.9956), // Kristiansand vest
                (58.1700, 8.0500), // Tveit / Kjevik innflyging
                (58.2500, 8.0000), // Vennesla sør
                (58.3500, 7.9500), // Vennesla nord
                (58.3480, 8.5934), // Grimstad vest
                (58.3400, 8.6500), // Grimstad øst
                (58.2600, 8.3500), // Lillesand
                (58.1500, 8.3500), // Høvåg
                (58.1000, 7.7000), // Søgne indre
                (58.0300, 7.8000), // Søgne kyst
                (58.5800, 7.8000), // Evje
                (58.6700, 7.8200), // Byglandsfjord
                (58.8500, 7.3500), // Åseral
                (58.7000, 7.6000), // Hægeland
            };


            for (int i = 1; i <= 10; i++)
            {
                var reporter = pilots[random.Next(pilots.Count)];
                var registrar = registrars.Any() ? registrars[random.Next(registrars.Count)] : null;

                bool isLine = random.Next(0, 2) == 0;

                // Velg en tilfeldig base-lokasjon i Norge
                var baseLoc = baseLocations[random.Next(baseLocations.Length)];
                var baseLat = baseLoc.Lat;
                var baseLng = baseLoc.Lng;

                // ---------------------------
                // 1. Coordinates
                // ---------------------------
                List<MapCoordinate> coords;

                if (isLine)
                {
                    coords = new List<MapCoordinate>();

                    // Litt jitter rundt base-posisjon for linje
                    var startLat = baseLat + (random.NextDouble() - 0.5) / 100;  // ~±0.005°
                    var startLng = baseLng + (random.NextDouble() - 0.5) / 100;

                    for (int p = 0; p < 2; p++)
                    {
                        coords.Add(new MapCoordinate
                        {
                            Latitude = startLat + (p * 0.01),   // ca 1 km mellom punkter (litt overdrevet, men fint visuelt)
                            Longitude = startLng + (p * 0.015),
                            OrderIndex = p
                        });
                    }
                }
                else
                {
                    // Punkt nær base-posisjon med liten variasjon
                    coords = new List<MapCoordinate>
                    {
                        new MapCoordinate
                        {
                            Latitude = baseLat + (random.NextDouble() - 0.5)/100,
                            Longitude = baseLng + (random.NextDouble() - 0.5)/100,
                            OrderIndex = 0
                        }
                    };
                }

                // ---------------------------
                // 2. Build GeoJSON (compact)
                // ---------------------------
                string geoJson;

                if (isLine)
                {
                    geoJson = JsonSerializer.Serialize(new
                    {
                        type = "Feature",
                        geometry = new
                        {
                            type = "LineString",
                            coordinates = coords.Select(c => new[] { c.Longitude, c.Latitude })
                        }
                    });
                }
                else
                {
                    var p = coords.First();
                    geoJson = JsonSerializer.Serialize(new
                    {
                        type = "Feature",
                        geometry = new
                        {
                            type = "Point",
                            coordinates = new[] { p.Longitude, p.Latitude }
                        }
                    });
                }

                // ---------------------------
                // 3. MapData
                // ---------------------------
                var mapData = new MapData
                {
                    Coordinates = coords,
                    GeometryType = isLine ? "LineString" : "Point",
                    MapZoomLevel = 11, // Litt mer "regionalt" zoomnivå
                    GeoJsonCoordinates = geoJson
                };

                context.MapDatas.Add(mapData);
                await context.SaveChangesAsync();

                // ---------------------------
                // 4. Obstacle
                // ---------------------------
                var obstacle = new ObstacleData
                {
                    ObstacleType = obstacleTypes[random.Next(obstacleTypes.Length)],
                    ObstacleHeight = random.Next(20, 300),
                    ObstacleWidth = random.Next(2, 20),
                    ObstacleComment = random.Next(0, 4) == 0
                        ? "Automatisk generert hindring."
                        : "Rapportert av pilot."
                };

                context.Obstacles.Add(obstacle);
                await context.SaveChangesAsync();

                // ---------------------------
                // 5. Summary
                // ---------------------------
                var first = coords.First();
                string summary = isLine
                    ? $"{first.Latitude:F4}, {first.Longitude:F4} (linje med {coords.Count} punkter)"
                    : $"{first.Latitude:F4}, {first.Longitude:F4}";

                // ---------------------------
                // 6. Report
                // ---------------------------
                var report = new ObstacleReportData
                {
                    ObstacleID = obstacle.ObstacleID,
                    MapDataID = mapData.MapDataID,
                    SubmittedByUserId = reporter.Id,
                    ObstacleReportComment = isLine
                        ? "Mulig høyspentlinje observert."
                        : "Objekt observert og rapportert.",
                    CoordinateSummary = summary,
                    ObstacleReportStatus = statusValues[random.Next(statusValues.Length)],
                    ObstacleReportDate = DateTime.Now.AddDays(-random.Next(60)),
                    ReviewedByUserID = registrar?.Id
                };

                context.ObstacleReports.Add(report);
                await context.SaveChangesAsync();
            }
        }
    }
}
