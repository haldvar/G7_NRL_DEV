using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;

namespace NRL_PROJECT.Controllers
{
    public class HomeController : Controller
    {
        // Logger for diagnostics and logging
        private readonly ILogger<HomeController> _logger;

        // Entity Framework DbContext for database operations
        

        // Connection string for manual MySQL access

        // Constructor: injects DbContext and configuration
        public HomeController()
        {
            
        }

        // Called when accessing the root page ("/")
        public async Task<IActionResult> Index()
        {
            return View();
        }

        // Called when clicking "Register obstacle" link in Index view
        [HttpGet]
        public ActionResult ObstacleDataForm()
        {
            return View();
        }

        // Called when submitting the "Submit data" button in ObstacleDataForm view
        [HttpPost]
        public ActionResult ObstacleDataForm(ObstacleData obstacledata)
        {
            if (!ModelState.IsValid)
            {
                return View(obstacledata); // return form with validation errors
            }

            return View("ObstacleOverview", obstacledata);
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

        /*
        // Alternative constructor with logger (commented out)
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Alternative constructor with only config (commented out)
        public HomeController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        // Manual MySqlConnection registration (commented out)
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddSingleton(new MySqlConnection(connectionString));
        */
    }
}
