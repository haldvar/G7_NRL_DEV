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
        private readonly NRL_PROJECT.Data.NRL_Db_Context _context;

        public AdminController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            NRL_PROJECT.Data.NRL_Db_Context context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // ---------------------------------------------------------------------
        // DASHBOARD
        // ---------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var allUsers = await _userManager.Users.ToListAsync();

            int admins = 0;
            int pilots = 0;
            int registrars = 0;
            int external = 0;
            int noRole = 0;

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin")) admins++;
                else if (roles.Contains("Pilot")) pilots++;
                else if (roles.Contains("Registrar")) registrars++;
                else if (roles.Contains("ExternalOrg")) external++;
                else noRole++;
            }

            ViewBag.TotalUsers = allUsers.Count;
            ViewBag.Admins = admins;
            ViewBag.Pilots = pilots;
            ViewBag.Registrars = registrars;
            ViewBag.ExternalOrgs = external;
            ViewBag.NoRole = noRole;

            return View();
        }

        // ---------------------------------------------------------------------
        // MANAGE USERS
        // ---------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            // 1. LAST ORGANISASJONENE (før vi lager viewmodels!)
            var orgList = await _context.Organisations
                .OrderBy(o => o.OrgName)
                .Select(o => new SelectListItem
                {
                    Value = o.OrgID.ToString(),
                    Text = o.OrgName
                })
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Organizations = orgList;

            // 2. LAST BRUKERE
            var users = await _userManager.Users
                .Include(u => u.Organisation)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            var vmList = new List<UserManagementViewModel>();
            int adminCount = 0;

            // 3. LAG VIEWMODELS
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                    adminCount++;

                vmList.Add(new UserManagementViewModel
                {
                    UserID = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    OrgName = user.Organisation?.OrgName,
                    OrgID = user.OrgID,
                    CurrentRole = roles.FirstOrDefault() ?? "No Role",
                    AvailableRoles = new List<string> { "Admin", "Pilot", "Registrar", "ExternalOrg" },
                    AvailableOrganizations = orgList
                });
            }

            ViewBag.AdminCount = adminCount;

            return View(vmList);
        }

        // ---------------------------------------------------------------------
        // CREATE USER
        // ---------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(RegisterViewModel model, int OrgId)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("ManageUsers");

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
                OrgID = org.OrgID,
                OrgName = org.OrgName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction("ManageUsers");
            }

            if (!string.IsNullOrWhiteSpace(model.RoleName) && model.RoleName != "No Role")
                await _userManager.AddToRoleAsync(user, model.RoleName);

            TempData["Success"] = "Bruker opprettet.";
            return RedirectToAction("ManageUsers");
        }

        // ---------------------------------------------------------------------
        // CREATE ORGANISATION (POST)
        // ---------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrganisation(string OrgName, string OrgContactEmail)
        {
            if (string.IsNullOrWhiteSpace(OrgName))
            {
                TempData["Error"] = "Organisasjonsnavn må fylles inn.";
                return RedirectToAction("ManageUsers");
            }

            if (await _context.Organisations.AnyAsync(o => o.OrgName == OrgName.Trim()))
            {
                TempData["Error"] = "Organisasjonen finnes allerede.";
                return RedirectToAction("ManageUsers");
            }

            var org = new Organisation
            {
                OrgName = OrgName.Trim(),
                OrgContactEmail = (OrgContactEmail ?? "").Trim()
            };

            _context.Organisations.Add(org);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Organisasjon \"{OrgName}\" er opprettet.";
            return RedirectToAction("ManageUsers");
        }

        // ---------------------------------------------------------------------
        // AJAX: CREATE ORGANISATION
        // ---------------------------------------------------------------------
        [HttpPost]
        [Route("Admin/CreateOrganisationAjax")]
        public async Task<IActionResult> CreateOrganisationAjax([FromBody] Organisation model)
        {
            if (string.IsNullOrWhiteSpace(model.OrgName))
                return Json(new { success = false, message = "Organisasjonsnavn må fylles inn." });

            if (await _context.Organisations.AnyAsync(o => o.OrgName == model.OrgName))
                return Json(new { success = false, message = "Organisasjonen finnes allerede." });

            _context.Organisations.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { success = true, orgId = model.OrgID, orgName = model.OrgName });
        }

        // ---------------------------------------------------------------------
        // ASSIGN ORGANISATION TO USER
        // ---------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignOrganisation(string UserID, int OrgID)
        {
            var user = await _userManager.FindByIdAsync(UserID);
            if (user == null)
            {
                TempData["Error"] = "Bruker ble ikke funnet.";
                return RedirectToAction("ManageUsers");
            }

            var org = await _context.Organisations.FindAsync(OrgID);
            if (org == null)
            {
                TempData["Error"] = "Organisasjon ikke funnet.";
                return RedirectToAction("ManageUsers");
            }

            user.OrgID = OrgID;
            user.OrgName = org.OrgName;

            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"Organisasjon oppdatert for {user.UserName}.";
            return RedirectToAction("ManageUsers");
        }

        // ---------------------------------------------------------------------
        // ASSIGN ROLE
        // ---------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string UserID, string role)
        {
            var user = await _userManager.FindByIdAsync(UserID);
            if (user == null)
            {
                TempData["Error"] = "Bruker ikke funnet.";
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

        // ---------------------------------------------------------------------
        // DELETE USER
        // ---------------------------------------------------------------------
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
                int adminCount = 0;
                foreach (var u in await _userManager.Users.ToListAsync())
                {
                    var rs = await _userManager.GetRolesAsync(u);
                    if (rs.Contains("Admin"))
                        adminCount++;
                }

                if (adminCount <= 1)
                {
                    TempData["Error"] = "Kan ikke slette siste admin.";
                    return RedirectToAction("ManageUsers");
                }
            }

            await _userManager.DeleteAsync(user);

            TempData["Success"] = $"Bruker {user.UserName} ble slettet.";
            return RedirectToAction("ManageUsers");
        }
    }
}
