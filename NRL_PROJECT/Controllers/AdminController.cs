using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Models;
using NRL_PROJECT.Models.ViewModels;

namespace NRL_PROJECT.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly NRL_PROJECT.Data.NRL_Db_Context _context; // ADD

        public AdminController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            NRL_PROJECT.Data.NRL_Db_Context context) // ADD
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context; // ADD
        }

        // GET: /Admin/Dashboard - Main landing page for admins
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Get statistics
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

        // GET: /Admin/ManageUsers
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            // load users (unchanged)
            var users = await _userManager.Users
                .Include(u => u.Organisation)
                .ToListAsync();

            // count admins (unchanged)
            var adminCount = 0;
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin")) adminCount++;
            }
            ViewBag.AdminCount = adminCount;

            // map VMs (unchanged)
            var userViewModels = new List<UserManagementViewModel>();
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
                    OrgName = user.Organisation?.OrgName,
                    CurrentRole = roles.FirstOrDefault() ?? "No Role",
                    AvailableRoles = new List<string> { "Admin", "Pilot", "Registrar", "ExternalOrg" }
                });
            }

            // ✅ FILL THE DROPDOWN ITEMS
            ViewBag.Organizations = await _context.Organisations
                .OrderBy(o => o.OrgName)
                .Select(o => new SelectListItem { Value = o.OrgID.ToString(), Text = o.OrgName })
                .AsNoTracking()
                .ToListAsync();

            return View(userViewModels);
        }



        // POST: /Admin/AssignRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string UserID, string role)
        {
            var user = await _userManager.FindByIdAsync(UserID);
            
            if (user == null)
            {
                return NotFound();
            }

            // Remove all existing roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // Add new role (if not "No Role")
            if (!string.IsNullOrEmpty(role) && role != "No Role")
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            TempData["Success"] = $"Rolle oppdatert for {user.UserName}";
            return RedirectToAction("ManageUsers");
        }

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
            if (roles.Contains("Admin"))
            {
                var allUsers = await _userManager.Users.ToListAsync();
                int adminCount = 0;

                foreach (var u in allUsers)
                {
                    var r = await _userManager.GetRolesAsync(u);
                    if (r.Contains("Admin"))
                        adminCount++;
                }

                if (adminCount <= 1)
                {
                    TempData["Error"] = "Du kan ikke slette den siste admin-brukeren.";
                    return RedirectToAction("ManageUsers");
                }
            }

            var result = await _userManager.DeleteAsync(user);
            TempData["Success"] = result.Succeeded ? $"Bruker {user.UserName} slettet." : "Kunne ikke slette bruker.";
            return RedirectToAction("ManageUsers");
        }
        
        
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(RegisterViewModel model, int OrgId)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("ManageUsers");

            // Validate org exists
            var org = await _context.Organisations.FindAsync(OrgId);
            if (org == null)
            {
                TempData["Error"] = "Ugyldig organisasjon.";
                return RedirectToAction("ManageUsers");
            }

            var user = new User
            {
                UserName = string.IsNullOrWhiteSpace(model.UserName) ? model.Email : model.UserName,
                Email = model.Email,
                EmailConfirmed = true,
                FirstName = model.FirstName,
                LastName = model.LastName,
                OrgID = org.OrgID,           // ✅ set FK
                OrgName = org.OrgName        // optional, if you still keep a shadow string
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.RoleName) && model.RoleName != "No Role")
                    await _userManager.AddToRoleAsync(user, model.RoleName);

                TempData["Success"] = "User created successfully.";
            }
            else
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("ManageUsers");
        }




        }
    }
