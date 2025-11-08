using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public AdminController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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
            var noRole = 0;

            foreach (var user in await _userManager.Users.ToListAsync())
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin")) admins++;
                else if (roles.Contains("Pilot")) pilots++;
                else if (roles.Contains("Registrar")) registrars++;
                else noRole++;
            }

            ViewBag.TotalUsers = totalUsers;
            ViewBag.Admins = admins;
            ViewBag.Pilots = pilots;
            ViewBag.Registrars = registrars;
            ViewBag.NoRole = noRole;

            return View();
        }

        // GET: /Admin/ManageUsers
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            
            var userViewModels = new List<UserManagementViewModel>();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                userViewModels.Add(new UserManagementViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CurrentRole = roles.FirstOrDefault() ?? "No Role",
                    AvailableRoles = new List<string> { "Admin", "Pilot", "Registrar" }
                });
            }
            
            return View(userViewModels);
        }

        // POST: /Admin/AssignRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
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

        // POST: /Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            
            if (result.Succeeded)
            {
                TempData["Success"] = $"Bruker {user.UserName} slettet";
            }
            else
            {
                TempData["Error"] = "Kunne ikke slette bruker";
            }

            return RedirectToAction("ManageUsers");
        }
    }
}
