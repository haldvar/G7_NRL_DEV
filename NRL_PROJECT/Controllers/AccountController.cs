using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NRL_PROJECT.Models;
using NRL_PROJECT.Models.ViewModels;

namespace NRL_PROJECT.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        
        public AccountController(UserManager<User> userManager,
            SignInManager<User> signInManager) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        
        //GET /Account/Register (Metode - Registrering av brukere)
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        
        //POST /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Success message, redirect to login.
                    TempData["RegistrationSuccess"] = "Fullført registrering - Vent litt";
                    return RedirectToAction("RegistrationSuccess");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(model);
        }
        
        // GET /Account/RegistrationSuccess
        [HttpGet]
        public IActionResult RegistrationSuccess()
        {
            return View();
        }
        
        // GET /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(
                        userName: user.UserName,
                        password: model.Password,
                        isPersistent: false,
                        lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        // Redirect to RegisterAreaChange view
                        return RedirectToAction("RegisterAreaChange", "Home");
                    }
                }
                
                ModelState.AddModelError("", "Ugyldig login");
            }
            return View(model);
        }
        
        //POST /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
