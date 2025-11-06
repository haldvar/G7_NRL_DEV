using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace NRL_PROJECT.Controllers
{
    public class HomeController : Controller
    {
        //  -------
        //  START: LOGIN IDENTITY
        
        /*
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
    
        public HomeController(
            ILogger<HomeController> logger,
            UserManager<User> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RegisterAreaChange(string geoJson, string description)
        {
            try
            {
                if (string.IsNullOrEmpty(geoJson) || string.IsNullOrEmpty(description))
                {
                    return BadRequest("Ugyldig data.");
                }

                var user = await _userManager.GetUserAsync(AUser);
                var userId = auser.Id;
                
                // Save to database using Dapper
                _geoChangeService.AddGeoChange(description, geoJson, userId);
                
                // Redirect to the overview of changes
                return RedirectToAction("AreaChangeOverview");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering area change.");
                throw;
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AreaChangeOverview()
        {
            var auser = await _userManager.GetUserAsync(AUser);
            var userId = auser.Id;

            var changes = _geoChangeService.GetAllGeoChanges(userId);
            return View(changes);
        }

        //New action methods for UpdateOverview feature
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UpdateOverView()
        {
            try
            {
                var user = await _userManager.GetUserAsync(AUser);
                var userId = auser.Id;

                var allChanges = _geoChangeService.GetAllGeoChanges(userId);
                return View(allChanges);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GeoChanges in UpdateOverview.");
                return View("Error.");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation($"Edit GET action called with id={id}");
            
            var auser = await _userManager.GetUserAsync(AUser);
            var userId = auser.Id;

            var geoChange = _geoChangeService.GetGeoChangeById(id, userId);
            if (geoChange == null)
            {
                _logger.LogWarning($"GeoChange with id={id} not found for userId={userId}");
                return NotFound();
            }
            return View(geoChange);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(GeoChange model)
        {
            //Remove validation for UserId since it is set programmatically
            ModelState.Remove("UserId");
            
            //Get the current logged-in user
            var auser = await _userManager.GetUserAsync(AUser);
            
            // Set the UserId programmatically
            model.UserId = auser.Id;
            
            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState is valid. Updating GeoChange.");
                
                // Proceed with updating the geo change
                _geoChangeService.UpdateGeoChange(model.Id, model.Description, model.GeoJson, auser.Id);
                return RedirectToAction("UpdateOverview");
            }
            else
            {
                _logger.LogWarning("ModelState is invalid.");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogWarning(error.ErrorMessage);
                    }
                }
            }
            
            return View(model);
        }
        
        [Authorize]
        [HttpGet]
        public async Task>IActionResult> Delete(int id)
        {
            var auser = await _userManager.GetUserAsync(AUser);
            var userId = auser.Id;
            
            var geoChange = _geoChangeService.GetGeoChangeById(id, userId);
            if (geoChange == null)
            {
                return NotFound();
            }
            return View(geoChange);
        }
        
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var auser = await _userManager.GetUserAsync(AUser);
            var userId = auser.Id;
            
            _geoChangeService.DeleteGeoChange(id, userId);
            return RedirectToAction("UpdateOverview");
        }
        
        */
        
        //  END: LOGIN IDENTITY
        //  -------
        
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

        /*[HttpGet]
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
        */
       

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

       
    }
}
