using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using System.Diagnostics;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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

        // Called when accessing the root page ("/")
        public async Task<IActionResult> Index()
        {
            try
            {
                var reportCount = await _context.ObstacleReports.CountAsync();
                ViewBag.ReportCount = reportCount;
                return View();
            }
            catch (Exception ex)
            {
                return Content("Failed to connect to database via EF: " + ex.Message);
            }
        }


        // Called when clicking "Register obstacle" link in Index view
        [HttpGet]
        public ActionResult ObstacleDataForm()
        {
            return View();
        }

        // Called when pressing the "Submit data" button in ObstacleDataForm view
       
        [HttpGet]
        public async Task<IActionResult> ObstacleOverview(int id)
        {
            var obstacle = await _context.Obstacles.FindAsync(id);
            if (obstacle == null)
            {
                return NotFound();
            }

            return View(obstacle);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitObstacle(ObstacleData obstacledata)
        {
            if (!ModelState.IsValid)
            {
                return View("ObstacleDataForm", obstacledata);
            }

            _context.Obstacles.Add(obstacledata);
            await _context.SaveChangesAsync();

            return RedirectToAction("ObstacleOverview", new { id = obstacledata.ObstacleId });
        }



        // Displays the About view with disabled caching
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult About()
        {
            return View();
        }

        // Displays the Privacy view 
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Privacy()
        {
            return View();
        }

        // Displays the Error view with request ID for diagnostics
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Returns test obstacle data in GeoJSON format
        public IActionResult GetObstacles()
        {
            var geojson = new
            {
                type = "FeatureCollection",
                features = new[]
                {
                    new {
                        type = "Feature",
                        geometry = new {
                            type = "Point",
                            coordinates = new[] { 8.233, 58.333 }
                        },
                        properties = new {
                            name = "Test obstacle"
                        }
                    }
                }
            };

            return Json(geojson);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitObstacleWithLocation(ObstacleReportViewModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(model.GeoJsonCoordinates))
            {
                ModelState.AddModelError("", "Location must be selected on the map.");
                return View("ObstacleAndMapForm", model);
            }

            var obstacle = new ObstacleData
            {
                ObstacleId = model.ObstacleId,
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

            var report = new ObstacleReportData
            {
                Reported_Item = model.ObstacleType,
                Reported_Location = ParseCoordinates(model.GeoJsonCoordinates),
                Time_of_Submitted_Report = DateTime.UtcNow,
                ObstacleId = obstacle.ObstacleId // kobling!
            };

            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

            return RedirectToAction("ReportListOverview");
        }

        
        [HttpGet]
        public IActionResult ObstacleAndMapForm()
        {
            return View();
        }

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
            var report = new ObstacleReportData
            {
                Reported_Item = model.ObstacleType,
                Reported_Location = ParseCoordinates(model.GeoJsonCoordinates),
                Time_of_Submitted_Report = DateTime.UtcNow
            };

            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

            return RedirectToAction("ReportListOverview", "Home");
        }

        private string ParseCoordinates(string geoJson)
        {
            try
            {
                var coords = JsonSerializer.Deserialize<double[]>(geoJson);
                return coords != null && coords.Length == 2 ? $"{coords[1]},{coords[0]}" : "Unknown";
            }
            catch
            {
                return "Invalid";
            }
        }
    }
}
