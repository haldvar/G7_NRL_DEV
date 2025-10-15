using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NRL_HALLI_DEV.Models;
using MySqlConnector;

namespace NRL_HALLI_DEV.Controllers;

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
            return View();
        }
        catch (Exception e)
        {
            return Content("Failed to connect to MariaDB: " + e.Message);
        }
    }

    // public IActionResult Index()
    // {
    //     return View();
    // }
     
    [HttpGet]
    public ActionResult DataForm()
    {
        return View();
    }

    [HttpPost]
    public IActionResult DataForm(ObstacleData obstacledata)
    {
        return View("Overview", obstacledata);
    }
    
    public IActionResult Privacy()
    {
        return View();
    }
    
    public IActionResult Map()
    {
        return View();
    }
    
    public IActionResult Map_experiment()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}