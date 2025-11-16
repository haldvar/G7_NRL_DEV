using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Models;

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

        // 1. Rollenavn
        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Pilot", "Registrar", "ExternalOrg" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Organisasjoner
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

        // 3. Adminbruker
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

        // 4. Demo Pilots
        private static async Task SeedDemoPilots(UserManager<User> userManager, NRL_Db_Context context)
        {
            if (await userManager.Users.AnyAsync(u => u.UserName.StartsWith("pilot")))
                return;

            // Hent organisasjonene som finnes i databasen
            var organisations = await context.Organisations.OrderBy(o => o.OrgID).ToListAsync();

            if (!organisations.Any())
                return; // fail safe – skal aldri skje

            int orgCount = organisations.Count;

            for (int i = 1; i <= 10; i++)
            {
                // Finn organisasjon basert på pilotnummer
                // rullerer mellom 1,2,3,4,1,2,3,4 …
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
                else
                    Console.WriteLine($"Pilot{i} ble IKKE laget: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }


        // 5. Demo Registrars
        private static async Task SeedDemoRegistrars(UserManager<User> userManager, NRL_Db_Context context)
        {
            if (await userManager.Users.AnyAsync(u => u.UserName.StartsWith("reg")))
                return;

            for (int i = 1; i <= 5; i++)
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

        // 6. Demo ExternalOrg Users
        private static async Task SeedDemoExternalOrgUsers(UserManager<User> userManager, NRL_Db_Context context)
        {
            if (await userManager.Users.AnyAsync(u => u.UserName.StartsWith("ext")))
                return;

            var orgs = await context.Organisations.ToListAsync();

            foreach (var org in orgs)
            {
                for (int i = 1; i <= 2; i++)
                {
                    var clean = org.OrgName.Replace(" ", "").Replace("ø", "o").Replace("å", "a").Replace("æ", "ae");

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

        // 7. Demo Obstacle Reports
        private static async Task SeedDemoReports(NRL_Db_Context context, UserManager<User> userManager)
        {
            if (await context.ObstacleReports.AnyAsync())
                return;

            var random = new Random();
            var pilots = await userManager.GetUsersInRoleAsync("Pilot");

            for (int i = 1; i <= 20; i++)
            {
                var reporter = pilots[random.Next(pilots.Count)];

                // Coordinates
                var coords = new List<MapCoordinate>
                {
                    new MapCoordinate { Latitude = 59.90 + random.NextDouble()/100, Longitude = 10.75 + random.NextDouble()/100 },
                    new MapCoordinate { Latitude = 59.90 + random.NextDouble()/100, Longitude = 10.75 + random.NextDouble()/100 },
                    new MapCoordinate { Latitude = 59.90 + random.NextDouble()/100, Longitude = 10.75 + random.NextDouble()/100 }
                };

                // MapData
                var mapData = new MapData { Coordinates = coords };
                context.MapDatas.Add(mapData);
                await context.SaveChangesAsync();

                // ObstacleData
                var obstacle = new ObstacleData
                {
                    ObstacleType = (new[] { "Mast", "Kran", "Bygning", "Tårn" })[random.Next(4)],
                    ObstacleHeight = random.Next(15, 120),
                    ObstacleWidth = random.Next(3, 15),
                    ObstacleComment = "Demo hindring"
                };
                context.Obstacles.Add(obstacle);
                await context.SaveChangesAsync();

                // Summary
                var first = coords.First();
                string summary = $"{first.Latitude:F4}, {first.Longitude:F4} (+{coords.Count - 1} punkter)";

                // Report
                var report = new ObstacleReportData
                {
                    ObstacleID = obstacle.ObstacleID,
                    MapDataID = mapData.MapDataID,
                    ObstacleReportDate = DateTime.Now.AddDays(-random.Next(30)),
                    ObstacleReportStatus = ObstacleReportData.EnumTypes.Open,

                    SubmittedByUserId = reporter.Id,
                    UserName = reporter.UserName,

                    ObstacleReportComment = "Dette er en demo-rapport.",
                    CoordinateSummary = summary
                };

                context.ObstacleReports.Add(report);
                await context.SaveChangesAsync();
            }
        }
    }
}
