using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using NRL_PROJECT.Models.ViewModels;
using static NRL_PROJECT.Models.ObstacleReportData;

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

        // -------------------------------------------------------
        // ORG VIEW
        // -------------------------------------------------------
        public async Task<IActionResult> OrgView()
        {
            var user = await _userManager.Users
                .Include(u => u.Organisation)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            // Bruker finnes, men har ikke organisasjon
            if (user?.Organisation == null)
            {
                ViewBag.OrgName = "(ingen organisasjon tildelt – kontakt administrator)";
                return View(user);
            }

            // Bruker har organisasjon
            return View(user);
        }


        // -------------------------------------------------------
        // ORG REPORT VIEW
        // -------------------------------------------------------
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

            // Hvis bruker mangler organisasjon → vennlig feilmelding
            if (user?.Organisation == null)
            {
                TempData["Error"] = "Du er ikke tilknyttet en organisasjon ennå. Kontakt administrator.";
                return RedirectToAction("OrgView");
            }

            var orgName = user.Organisation.OrgName;

            var query = _context.ObstacleReports
                .Include(r => r.SubmittedByUser)
                    .ThenInclude(u => u.Organisation)
                .Include(r => r.MapData)
                .Include(r => r.Obstacle)
                .Where(r => r.SubmittedByUser.Organisation.OrgName == orgName)
                .AsQueryable();

            // SØK - kommentert ut pga ikke fungerende enda
            /*
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r =>
                    (r.Obstacle != null && r.Obstacle.ObstacleType.Contains(search)) ||
                    r.ObstacleReportStatus.ToString().Contains(search) ||
                    (r.SubmittedByUser != null && r.SubmittedByUser.UserName.Contains(search)) ||
                    (r.SubmittedByUser != null && r.SubmittedByUser.Email.Contains(search)) ||
                    (r.SubmittedByUser != null && r.SubmittedByUser.Organisation.OrgName.Contains(search)) ||
                    (r.MapData != null && r.MapData.CoordinateSummary.Contains(search))
                );
            }
            */

            // FILTER: STATUS
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse(status, out EnumTypes statusEnum))
                {
                    query = query.Where(r => r.ObstacleReportStatus == statusEnum);
                }
            }

            // FILTER: TYPE
            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(r => r.Obstacle.ObstacleType == type);

            // SORTERING
            query = sortField switch
            {
                "type" => sortDir == "asc"
                    ? query.OrderBy(r => r.Obstacle.ObstacleType)
                    : query.OrderByDescending(r => r.Obstacle.ObstacleType),

                "status" => sortDir == "asc"
                    ? query.OrderBy(r => r.ObstacleReportStatus)
                    : query.OrderByDescending(r => r.ObstacleReportStatus),

                "user" => sortDir == "asc"
                    ? query.OrderBy(r => r.SubmittedByUser.UserName)
                    : query.OrderByDescending(r => r.SubmittedByUser.UserName),

                "org" => sortDir == "asc"
                    ? query.OrderBy(r => r.SubmittedByUser.Organisation.OrgName)
                    : query.OrderByDescending(r => r.SubmittedByUser.Organisation.OrgName),

                _ => sortDir == "asc"
                    ? query.OrderBy(r => r.ObstacleReportDate)
                    : query.OrderByDescending(r => r.ObstacleReportDate)
            };

            // DROPDOWN-LISTE MED TYPER
            ViewBag.Types = await _context.Obstacles
                .Select(o => o.ObstacleType)
                .Distinct()
                .ToListAsync();

            ViewBag.SortField = sortField;
            ViewBag.SortDir = sortDir;

            // HENTER FAKTISKE ENTITETER
            var reportEntities = await query.ToListAsync();

            // MAPPER RESULTAT TIL VIEWMODEL
            var reports = reportEntities.Select(r => new OrgExternalViewModel
            {
                ObstacleReportID = r.ObstacleReportID,
                UserName = r.SubmittedByUser?.UserName,
                Email = r.SubmittedByUser?.Email,
                OrgName = r.SubmittedByUser?.Organisation?.OrgName,
                ObstacleID = r.ObstacleID,
                ObstacleType = r.Obstacle?.ObstacleType,
                ObstacleReportDate = r.ObstacleReportDate,
                ObstacleReportStatus = r.ObstacleReportStatus,
                CoordinateSummary = r.CoordinateSummary
            }).ToList();

            return View(reports);
        }
    }
}
