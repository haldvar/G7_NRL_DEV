using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NRL_PROJECT.Models;
using MySqlConnector;


namespace NRL_PROJECT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /*public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        */

        private readonly string _connectionString;

        public HomeController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        // blir kalt etter at vi trykker på "Register obstacle" lenken i Index-viewet
        [HttpGet]
        public ActionResult DataForm()
        {
            return View();
        }

        // blir kalt etter at vi trykker på "Submit data" knappen i DataForm-viewet
        [HttpPost]
        public ActionResult DataForm(ObstacleData obstacledata)
        {
            if (!ModelState.IsValid)
            {
                return View(obstacledata); // returner skjema med feilmeldinger
            }

            return View("ObstacleOverview", obstacledata);
        }



        // blir kalt etter at vi trykker på "Register Map stuff" lenken i Index-viewet
        [HttpGet]
        public ActionResult MapForm()
        {
            return View();
        }

        // blir kalt etter at vi trykker på "Submit Map stuff" knappen i SirkusForm-viewet
        [HttpPost]
        public ActionResult MapForm(MapData mapdata)
        {
            return View("MapOverview", mapdata);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();
                //return Content("Connected to MariaDB successfully!");
                //return View("Index","test");
                return View();
            }

            catch (Exception ex)
            {
                return Content("Failed to connect to MariaDB: " + ex.Message);
            }
        }

    }
}