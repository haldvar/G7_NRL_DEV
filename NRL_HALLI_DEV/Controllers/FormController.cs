using Microsoft.AspNetCore.Mvc;

namespace NRL_HALLI_DEV.Controllers;

public class FormController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}