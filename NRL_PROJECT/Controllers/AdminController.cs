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

        // DASHBOARD
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var admins = 0;
            var pilots = 0;
            var registrars = 0;
            var external = 0;
            var noRole = 0;

            foreach (var user in await _userManager.Users.ToListAsync())
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin")) admins++;
                else if (roles.Contains("Pilot")) pilots++;
                else if (roles.Contains("Registrar")) registrars++;
                else if (roles.Contains("ExternalOrg")) external++;
                else noRole++;
            }

            ViewBag.TotalUsers = totalUsers;
            ViewBag.Admins = admins;
            ViewBag.Pilots = pilots;
            ViewBag.Registrars = registrars;
            ViewBag.ExternalOrgs = external;
            ViewBag.NoRole = noRole;

            return View();
        }

        // MANAGE USERS
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users
                .Include(u => u.Organisation)
                .ToListAsync();

            var vmList = new List<UserManagementViewModel>();

            int adminCount = 0;

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin")) adminCount++;

                vmList.Add(new UserManagementViewModel
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

            ViewBag.AdminCount = adminCount;

            // DROPDOWN FOR ORGANISATIONS
            ViewBag.Organizations = await _context.Organisations
                .OrderBy(o => o.OrgName)
                .Select(o => new SelectListItem
                {
                    Value = o.OrgID.ToString(),
                    Text = o.OrgName
                })
                .AsNoTracking()
                .ToListAsync();

            return View(vmList);
        }

        // CREATE USER
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
            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(model.RoleName) && model.RoleName != "No Role")
                    await _userManager.AddToRoleAsync(user, model.RoleName);

                TempData["Success"] = "Bruker opprettet.";
            }
            else
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("ManageUsers");
        }

        // STANDARD POST (ikke AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrganisation(string OrgName, string OrgContactEmail)
        {
            if (string.IsNullOrWhiteSpace(OrgName))
            {
                TempData["Error"] = "Organisasjonsnavn må fylles inn.";
                return RedirectToAction("ManageUsers");
            }

            var exists = await _context.Organisations.AnyAsync(o => o.OrgName == OrgName.Trim());
            if (exists)
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

        // AJAX ENDPOINT
        [HttpPost]
        [Route("Admin/CreateOrganisationAjax")]
        public async Task<IActionResult> CreateOrganisationAjax([FromBody] Organisation model)
        {
            if (string.IsNullOrWhiteSpace(model.OrgName))
                return Json(new { success = false, message = "Organisasjonsnavn må fylles inn." });

            var exists = await _context.Organisations.AnyAsync(o => o.OrgName == model.OrgName);
            if (exists)
                return Json(new { success = false, message = "Organisasjonen finnes allerede." });

            _context.Organisations.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { success = true, orgId = model.OrgID, orgName = model.OrgName });
        }
    }
}
