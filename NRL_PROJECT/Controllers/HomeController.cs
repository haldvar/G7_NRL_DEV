using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using System.Diagnostics;
using System.Text.Json;

namespace NRL_PROJECT.Controllers
{
    /// <summary>
    /// Public-facing pages controller.
    /// - Index shows basic application stats.
    /// - About, Privacy and Error pages are served here.
    /// </summary>
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
        // Index / Startpage
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
        // Static pages
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
