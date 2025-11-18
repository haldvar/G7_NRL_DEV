using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using NRL_PROJECT.Models.ViewModels;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;
using static NRL_PROJECT.Models.ObstacleReportData;
using BackendStatus = NRL_PROJECT.Models.ObstacleReportData.EnumTypes;
using EnumStatus = NRL_PROJECT.Models.ObstacleReportData.EnumTypes;
using UiStatus = NRL_PROJECT.Models.EnumStatus;

namespace NRL_PROJECT.Controllers

{
    [Authorize(Roles = "ExternalOrg, Admin, Registrar")]
    public class OrgController : Controller
    {
        private readonly NRL_Db_Context _context;
        private readonly UserManager<User> _userManager;

        public OrgController(NRL_Db_Context context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // === ORG VIEW ===
        public async Task<IActionResult> OrgView()
        {
            var user = await _userManager.Users
                .Include(u => u.Organisation)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user == null)
                return Unauthorized();

            // Hvis bruker har rolle som gir tilgang, men MANGLER organisasjon
            if (user.Organisation == null)
            {
                // Vis view — men uten link til rapporter
                return View(user);
            }

            return View(user);
        }

        // === ORG REPORT VIEW ===
        public async Task<IActionResult> OrgReportView(
            string search,
            string status,
            string type,
            string sortField = "date",
            string sortDir = "desc")
        {
            var user = await _userManager.Users
                .Include(u => u.Organisation)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user == null || user.Organisation == null)
                return Unauthorized();

            var query = _context.ObstacleReports
                .Include(r => r.SubmittedByUser)
                    .ThenInclude(u => u.Organisation)
                .Include(r => r.MapData)
                    .ThenInclude(m => m.Coordinates)
                .Include(r => r.Obstacle)
                .Where(r => r.SubmittedByUser.Organisation.OrgID == user.OrgID)
                .AsQueryable();


            // -----------------------------
            // 🔍 SØK
            // -----------------------------
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r =>
                    r.Obstacle.ObstacleType.Contains(search) ||
                    r.ObstacleReportStatus.ToString().Contains(search) ||
                    r.SubmittedByUser.Email.Contains(search) ||
                    r.SubmittedByUser.OrgName.Contains(search) ||
                    r.MapData.CoordinateSummary.Contains(search)
                );
            }

            // -----------------------------
            // 🎚️ FILTER: STATUS
            // -----------------------------
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse(status, out ObstacleReportData.EnumTypes statusEnum))
                {
                    query = query.Where(r => r.ObstacleReportStatus == statusEnum);
                }
            }

            // -----------------------------
            // 🛑 FILTER: TYPE
            // -----------------------------
            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(r => r.Obstacle.ObstacleType == type);

            // -----------------------------
            // 🔽 SORTERING
            // -----------------------------
            query = sortField switch
            {
                "type" => (sortDir == "asc")
                    ? query.OrderBy(r => r.Obstacle.ObstacleType)
                    : query.OrderByDescending(r => r.Obstacle.ObstacleType),

                "status" => (sortDir == "asc")
                    ? query.OrderBy(r => r.ObstacleReportStatus)
                    : query.OrderByDescending(r => r.ObstacleReportStatus),

                "email" => (sortDir == "asc")
                    ? query.OrderBy(r => r.SubmittedByUser.Email)
                    : query.OrderByDescending(r => r.SubmittedByUser.Email),

                "org" => (sortDir == "asc")
                    ? query.OrderBy(r => r.SubmittedByUser.OrgName)
                    : query.OrderByDescending(r => r.SubmittedByUser.OrgName),

                _ => (sortDir == "asc")
                    ? query.OrderBy(r => r.ObstacleReportDate)
                    : query.OrderByDescending(r => r.ObstacleReportDate)
            };

            // Lag liste over alle unike obstacle types til dropdown i view
            ViewBag.Types = await _context.Obstacles
                .Select(o => o.ObstacleType)
                .Distinct()
                .ToListAsync();

            // Send sorteringsinfo tilbake til view
            ViewBag.SortField = sortField;
            ViewBag.SortDir = sortDir;

            var reports = await query
    .Select(r => new OrgExternalViewModel
    {
        ObstacleReportID = r.ObstacleReportID,
        ObstacleReportDate = r.ObstacleReportDate,
        ObstacleReportStatus = r.ObstacleReportStatus,

        // Innmelder
        UserName = r.SubmittedByUser.UserName,
        Email = r.SubmittedByUser.Email,
        OrgName = r.SubmittedByUser.Organisation.OrgName,

        // Hindring
        ObstacleID = r.Obstacle.ObstacleID,
        ObstacleType = r.Obstacle.ObstacleType,

        //  BILDESTØTTE
        ObstacleImageURL = r.Obstacle.ObstacleImageURL,

        // Kart
        CoordinateSummary = r.MapData.CoordinateSummary
    })
    .ToListAsync();

            return View(reports);
        }

    }
}
