using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using System.Diagnostics;
using System.Text.Json;
using System.IO;
using Microsoft.AspNetCore.Http;


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
        public async Task<IActionResult> SubmitObstacleWithLocation(ObstacleReportData model)
        {
            if (!ModelState.IsValid)
            {
                return View("ObstacleAndMapForm", model);
            }

            // ------------------------------
            //  LAGRING AV BILDE
            // ------------------------------
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                // Opprett mappe for opplastede bilder hvis den ikke finnes
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Gi bildet et unikt filnavn
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lagre bildet fysisk på serveren
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                // Lagre sti til bildet i databasen
                model.ObstacleImageURL = "/uploads/" + uniqueFileName;
            }

            // ------------------------------
            //  LAGRING AV RAPPORT
            // ------------------------------
            model.ObstacleReportDate = DateTime.UtcNow;
            model.ObstacleReportStatus = ObstacleReportData.EnumTypes.New;

            _context.ObstacleReports.Add(model);
            await _context.SaveChangesAsync();

            // Etter lagring: tilbake til forsiden
            return RedirectToAction("Index");
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

       
    }
}
