using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using NRL_PROJECT.Models.ViewModels;

namespace NRL_PROJECT.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly NRL_Db_Context _context;

        public AdminController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            NRL_Db_Context context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }


        // ------------------------------------------------------------
        //  DASHBOARD
        // ------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var admins = 0;
            var pilots = 0;
            var registrars = 0;
            var externalorgs = 0;
            var noRole = 0;

            foreach (var user in await _userManager.Users.ToListAsync())
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin")) admins++;
                else if (roles.Contains("Pilot")) pilots++;
                else if (roles.Contains("Registrar")) registrars++;
                else if (roles.Contains("ExternalOrg")) externalorgs++;
                else noRole++;
            }

            ViewBag.TotalUsers = totalUsers;
            ViewBag.Admins = admins;
            ViewBag.Pilots = pilots;
            ViewBag.Registrars = registrars;
            ViewBag.ExternalOrgs = externalorgs;
            ViewBag.NoRole = noRole;

            return View();
        }



        // ------------------------------------------------------------
        //  MANAGE USERS (GET)
        // ------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users
                
                .ToListAsync();

            var userViewModels = new List<UserManagementViewModel>();

            int adminCount = 0;
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin")) adminCount++;
            }
            ViewBag.AdminCount = adminCount;

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userViewModels.Add(new UserManagementViewModel
                {
                    UserID = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,

                    OrgID = user.OrgID,
                    OrgName = user.Organisation?.OrgName,

                    CurrentRole = roles.FirstOrDefault() ?? "No Role",
                    AvailableRoles = new List<string>
                    {
                        "Admin", "Pilot", "Registrar", "ExternalOrg"
                    }
                });
            }

            // Send organisasjoner til dropdown
            ViewBag.Organisations = new List<Organisation>
        {
            new Organisation { OrgID = 1, OrgName = "NRL" },
            new Organisation { OrgID = 2, OrgName = "Politiet" },
            new Organisation { OrgID = 3, OrgName = "Forsvaret" },
            new Organisation { OrgID = 4, OrgName = "Norsk Luftambulanse" }
            
        };

            return View(userViewModels);
        }



        // ------------------------------------------------------------
        //  ASSIGN ROLE
        // ------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string UserID, string role)
        {
            var user = await _userManager.FindByIdAsync(UserID);
            if (user == null)
            {
                TempData["Error"] = "Bruker finnes ikke.";
                return RedirectToAction("ManageUsers");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!string.IsNullOrEmpty(role) && role != "No Role")
                await _userManager.AddToRoleAsync(user, role);

            TempData["Success"] = $"Rolle oppdatert for {user.UserName}.";
            return RedirectToAction("ManageUsers");
        }



        // ------------------------------------------------------------
        //  DELETE USER
        // ------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string UserID)
        {
            var user = await _userManager.FindByIdAsync(UserID);
            if (user == null)
            {
                TempData["Error"] = "Bruker ikke funnet.";
                return RedirectToAction("ManageUsers");
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Hindre sletting av siste Admin
            if (roles.Contains("Admin"))
            {
                int adminCount = 0;
                foreach (var u in await _userManager.Users.ToListAsync())
                {
                    var r = await _userManager.GetRolesAsync(u);
                    if (r.Contains("Admin")) adminCount++;
                }

                if (adminCount <= 1)
                {
                    TempData["Error"] = "Kan ikke slette siste administrator.";
                    return RedirectToAction("ManageUsers");
                }
            }

            var result = await _userManager.DeleteAsync(user);
            TempData["Success"] = result.Succeeded ?
                $"Bruker {user.UserName} ble slettet." :
                "Kunne ikke slette bruker.";

            return RedirectToAction("ManageUsers");
        }



        // ------------------------------------------------------------
        //  CREATE USER
        // ------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Ugyldige data.";
                return RedirectToAction("ManageUsers");
            }

            var user = new User
            {
                UserName = string.IsNullOrWhiteSpace(model.UserName)
                    ? model.Email
                    : model.UserName,

                Email = model.Email,
                EmailConfirmed = true,
                FirstName = model.FirstName,
                LastName = model.LastName,

                OrgID = model.OrgID  // ← 🚀 Viktig!
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction("ManageUsers");
            }

            if (!string.IsNullOrEmpty(model.RoleName) && model.RoleName != "No Role")
            {
                await _userManager.AddToRoleAsync(user, model.RoleName);
            }

            TempData["Success"] = "Bruker opprettet!";
            return RedirectToAction("ManageUsers");
        }

    }
}
