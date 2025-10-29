using Microsoft.AspNetCore.Mvc;

namespace NRL_PROJECT.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
