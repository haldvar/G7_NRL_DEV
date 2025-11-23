using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NRL_PROJECT.Models;
using NRL_PROJECT.Models.ViewModels;

namespace NRL_PROJECT.Controllers
{           
    /// <summary>
    /// AccountController
    /// - Handles registration, login, logout, password reset and two-factor flows.
    /// - Methods are grouped: ctor, public auth endpoints (GET/POST pairs), helpers.
    /// </summary>

    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        // Constructor
        public AccountController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        // ------------------------------------------------------------
        // Public pages / actions (GET / POST grouped by feature)
        // ------------------------------------------------------------

        // Access denied view
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ---------- Register ----------
        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    LockoutEnd = null,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(1, "Brukeren din har blitt opprettet!");
                    return RedirectToAction("Login", "Account");
                }

                // Log the identity errors and add to ModelState
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"Registration error: {error.Code} - {error.Description}");
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                // Log ModelState errors for easier debugging
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError($"ModelState error: {error.ErrorMessage}");
                    }
                }
            }
            return View(model);
        }

        // ---------- Login ----------
        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Ugyldig login.");
                    return View(model);
                }

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, "Du må bekrefte e-posten før du kan logge inn.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName, // use username internally
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation(2, "Brukeren er logget inn.");
                    return await RedirectToLocal(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, model.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning(3, "Brukeren er låst, prøv igjen senere.");
                    return View("Lockout");
                }

                ModelState.AddModelError(string.Empty, "Ugyldig login.");
            }

            return View(model);
        }

        // ---------- Logout ----------
        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "Brukeren er logget ut.");
            return RedirectToAction("Login", "Account");
        }

        // ---------- Email confirmation ----------
        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string UserID, string code)
        {
            if (UserID == null || code == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(UserID);
            if (user == null)
            {
                return View("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        // ---------- Forgot password ----------
        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                {
                    // Don't reveal whether user exists or is confirmed
                    return View("ForgotPasswordConfirmation");
                }
            }
            return View(model);
        }

        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // ---------- Reset password ----------
        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation), "Account");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation), "Account");
            }
            AddErrors(result);
            return View();
        }

        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // ---------- Two-factor / SendCode / Verify flows ----------
        // GET: /Account/SendCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            if (model.SelectedProvider == "Authenticator")
            {
                return RedirectToAction(nameof(VerifyAuthenticatorCode), new { model.ReturnUrl, model.RememberMe });
            }

            // Generate the token and send it
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
            if (string.IsNullOrWhiteSpace(code))
            {
                return View("Error");
            }

            var message = "Din sikkerhetskode er: " + code;
            if (model.SelectedProvider == "Email")
            {
                await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Sikkerhetskode", message);
            }

            return RedirectToAction(nameof(VerifyCode),
                new { Provider = model.SelectedProvider, model.ReturnUrl, model.RememberMe });
        }

        // GET: /Account/VerifyCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            return View(new VerifyCodeViewModel
                { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
            if (result.Succeeded)
            {
                return await RedirectToLocal(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning(7, "Brukeren er låst.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Ugyldig kode.");
                return View(model);
            }
        }

        // GET: /Account/VerifyAuthenticatorCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyAuthenticatorCode(bool rememberMe, string returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            return View(new VerifyAuthenticatorCodeViewModel { ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        // POST: /Account/VerifyAuthenticatorCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAuthenticatorCode(VerifyAuthenticatorCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, model.RememberMe, model.RememberBrowser);
            if (result.Succeeded)
            {
                return await RedirectToLocal(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning(7, "Brukeren er låst ut.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Ugyldig kode.");
                return View(model);
            }
        }

        // ---------- Recovery code ----------
        // GET: /Account/UseRecoveryCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> UseRecoveryCode(string returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            return View(new UseRecoveryCodeViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/UseRecoveryCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UseRecoveryCode(UseRecoveryCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(model.Code);
            if (result.Succeeded)
            {
                return await RedirectToLocal(model.ReturnUrl);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Ugyldig kode.");
                return View(model);
            }
        }


        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
            
        private Task<User> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        private async Task<IActionResult> RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                // Check user role and redirect accordingly
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("ManageUsers", "Admin");
                        
                    }
                    else if (roles.Contains("Registrar"))
                    {
                        return RedirectToAction("RegistrarView", "Registrar");
                    }
                    else if (roles.Contains("Pilot"))
                    {
                        return RedirectToAction("ObstacleAndMapForm", "Map");
                    }
                    else if (roles.Contains("ExternalOrg"))
                    {
                        return RedirectToAction("OrgView", "Org");
                    }
                }
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
