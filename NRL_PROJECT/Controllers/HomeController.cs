using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using System.Diagnostics;
using System.Text.Json;

namespace NRL_PROJECT.Controllers
{
    public class HomeController : Controller
    {
        private readonly NRL_Db_Context _context;
        private readonly string _connectionString;

        public HomeController(NRL_Db_Context context, IConfiguration config)
        {
            _context = context;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        // ------------------------------
        //  INDEX / STARTPAGE
        // ------------------------------
        public async Task<IActionResult> Index()
        {
            try
            {
                ViewBag.ReportCount = await _context.ObstacleReports.CountAsync();
                return View();
            }
            catch (Exception ex)
            {
                return Content($"Failed to connect to database via EF: {ex.Message}");
            }
        }

        // ------------------------------
        //  OBSTACLE HANDLING
        // ------------------------------

        [HttpGet]
        public IActionResult ObstacleDataForm() => View();

        [HttpPost]
        public async Task<IActionResult> SubmitObstacle(ObstacleData obstacle)
        {
            if (!ModelState.IsValid)
                return View("ObstacleDataForm", obstacle);

            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ObstacleOverview), new { id = obstacle.ObstacleId });
        }

        [HttpGet]
        public async Task<IActionResult> ObstacleOverview(int id)
        {
            var obstacle = await _context.Obstacles.FindAsync(id);
            if (obstacle == null)
                return NotFound();

            return View(obstacle);
        }

        [HttpGet]
        public IActionResult ObstacleAndMapForm() => View();

        [HttpPost]
        public async Task<IActionResult> SubmitObstacleWithLocation(ObstacleReportViewModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(model.GeoJsonCoordinates))
            {
                ModelState.AddModelError("", "Location must be selected on the map.");
                return View("ObstacleAndMapForm", model);
            }

            // Opprett hinder
            var obstacle = new ObstacleData
            {
                ObstacleName = model.ObstacleName,
                ObstacleType = model.ObstacleType,
                ObstacleHeight = model.ObstacleHeight,
                ObstacleWidth = model.ObstacleWidth,
                ObstacleDescription = model.ObstacleDescription,
                Longitude = model.Longitude,
                Latitude = model.Latitude
            };

            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            // Opprett rapport koblet til hinderet
            var report = new ObstacleReportData
            {
                Reported_Item = model.ObstacleType,
                Reported_Location = ParseCoordinates(model.GeoJsonCoordinates),
                Time_of_Submitted_Report = DateTime.UtcNow,
                ObstacleId = obstacle.ObstacleId
            };

            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ReportListOverview));
        }

        // ------------------------------
        //  REPORT HANDLING
        // ------------------------------

        [HttpGet]
        public async Task<IActionResult> ReportListOverview()
        {
            var reports = await _context.ObstacleReports
                .Include(r => r.Obstacle)
                .ToListAsync();

            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitObstacleReport(ObstacleReportViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(ReportListOverview));

            var report = new ObstacleReportData
            {
                Reported_Item = model.ObstacleType,
                Reported_Location = ParseCoordinates(model.GeoJsonCoordinates),
                Time_of_Submitted_Report = DateTime.UtcNow
            };

            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ReportListOverview));
        }

        // ------------------------------
        //  STATIC PAGES
        // ------------------------------
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult About() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Privacy() => View();

        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        // ------------------------------
        //  API / UTILITIES
        // ------------------------------

        [HttpGet]
        public async Task<IActionResult> GetObstacles()
        {
            // 1️⃣ Hent alle hinder fra databasen
            var obstacles = await _context.Obstacles.ToListAsync();

            // 2️⃣ Bygg GeoJSON-struktur
            var geojson = new
            {
                type = "FeatureCollection",
                features = obstacles.Select(o => new
                {
                    type = "Feature",
                    geometry = new
                    {
                        type = "Point",
                        coordinates = new[] { o.Longitude, o.Latitude }
                    },
                    properties = new
                    {
                        id = o.ObstacleId,
                        name = o.ObstacleName,
                        type = o.ObstacleType,
                        description = o.ObstacleDescription
                    }
                })
            };

            // 3️⃣ Returner som JSON
            return Json(geojson);
        }


        private static string ParseCoordinates(string geoJson)
        {
            try
            {
                var coords = JsonSerializer.Deserialize<double[]>(geoJson);
                return coords is { Length: 2 } ? $"{coords[1]},{coords[0]}" : "Unknown";
            }
            catch
            {
                return "Invalid";
            }
        }
    }
}
