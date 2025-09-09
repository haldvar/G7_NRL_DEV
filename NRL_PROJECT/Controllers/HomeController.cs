using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NRL_PROJECT.Models;
using MySqlConnector;

namespace NRL_PROJECT.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    // public HomeController(ILogger<HomeController> logger)
    // {
    //     _logger = logger;
    // }
    
    private readonly string _connectionString;

    public HomeController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            return Content("Connected to MariaDB successfully!");
        }
        catch (Exception ex)
        {
            return Content("Failed to connect to MariaDB:" + ex.Message);
        }
    }

    // public IActionResult Index()
    // {
    //     return View();
    // }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}