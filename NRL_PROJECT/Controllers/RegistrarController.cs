using Microsoft.AspNetCore.Mvc;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using EnumStatus = NRL_PROJECT.Models.ObstacleReportData.EnumTypes;

namespace NRL_PROJECT.Controllers
    {
        public class RegistrarController : Controller
        {
            private readonly NRL_Db_Context _context;

            public RegistrarController(NRL_Db_Context context)
            {
                _context = context;
            }

        //POST: /Registrar/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, EnumStatus status, string? comment)
        {
            var report = await _context.ObstacleReports
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ObstacleReportID == id);

            if (report == null) return NotFound();

            report.ObstacleReportStatus = status;
            if (!string.IsNullOrWhiteSpace(comment))
                report.ObstacleReportComment = comment.Trim();

            var userName = User?.Identity?.Name;
            var reviewer = await _context.Users.FirstOrDefaultAsync(u => u.FirstName  == userName);
            if (reviewer != null)
            {
                report.ReviewedByUserID = reviewer.UserID;
                report.Reviewer = reviewer;
            }

            await _context.SaveChangesAsync();

            TempData["StatusMsg"] = $"Status satt til «{status}».";
            return RedirectToAction(nameof(ReportDetails), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> ReportDetails(int id)
        {
            var report = await _context.ObstacleReports
                .Include(r => r.Obstacle)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ObstacleReportID == id);

            if (report == null) return NotFound();

            var firstCoord = report.MapData?.Coordinates?
    .OrderBy(c => c.MapDataID)   // bruk den sorteringen du har (ID/Order)
    .FirstOrDefault();

            var lat = firstCoord?.Latitude ?? 0;
            var lng = firstCoord?.Longitude ?? 0;

            var reportedLocation = firstCoord != null
                ? $"{lat},{lng}"
                : string.Empty;

            var geoJson = report.MapData?.GeoJsonCoordinates ?? string.Empty;

            var vm = new ObstacleReportViewModel
            {
                // innsendning
                ReportId = report.ObstacleReportID,
                TimeOfSubmittedReport = report.ObstacleReportDate,

                // hinder
                ObstacleID = report.ObstacleReportID,          // evt. report.ObstacleId hvis du vil vise hinder-ID
                ObstacleType = report.Obstacle?.ObstacleType,
                ObstacleComment = report.Obstacle?.ObstacleComment,
                ObstacleHeight = report.Obstacle?.ObstacleHeight,
                ObstacleWidth = report.Obstacle?.ObstacleWidth,

                // lokasjon
                Latitude = lat,
                Longitude = lng,
                Reported_Location = reportedLocation,
                GeoJsonCoordinates = geoJson,

                // status
                ReportStatus = report.ObstacleReportStatus,

                // innsender
                UserId = report.User.RoleID,                   // evt. report.UserID hvis VM forventer bruker-ID
                UserName = $"{report.User?.FirstName ?? ""} {report.User?.LastName ?? ""}".Trim()
            };

            return View(vm);
        }

        // Henter alle rapporter
        [HttpGet]
        public async Task<IActionResult> RegistrarView(string? status = "Alle", string? q = null)
        {
            var query = _context.ObstacleReports
                .Include(r => r.Obstacle)
                .Include(r => r.User)
                .Include(r => r.MapData)
                   .ThenInclude(m => m.Coordinates)
                .AsQueryable();

            // Rens eventuelle "Alle (2)" -> "Alle"
            if (!string.IsNullOrWhiteSpace(status) &&
                status.StartsWith("Alle", StringComparison.OrdinalIgnoreCase))
            {
                status = "Alle";
            }

            // Bare filtrer hvis status er IKKE "Alle" og faktisk matcher enum
            if (!string.IsNullOrWhiteSpace(status) &&
                !string.Equals(status, "Alle", StringComparison.OrdinalIgnoreCase) &&
                Enum.TryParse<EnumStatus>(status, ignoreCase: true, out var parsed))
            {
                query = query.Where(r => r.ObstacleReportStatus == parsed);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();

                query = query.Where(r =>
                    r.ObstacleReportID.ToString().Contains(q) ||

                    // ObstacleType trygt når r.Obstacle kan være null
                    (r.Obstacle != null ? r.Obstacle.ObstacleType : "").Contains(q) ||

                    // Fornavn + etternavn trygt når r.User kan være null
                    (
                        (r.User != null ? r.User.FirstName : "") + " " +
                        (r.User != null ? r.User.LastName : "")
                    ).Contains(q)
                );
            }

            var model = await query
                 .OrderByDescending(r => r.ObstacleReportDate) // <- riktig dato-felt
                 .Select(r => new ObstacleReportViewModel
                 {
                    ReportId = r.ObstacleReportID,
                    TimeOfSubmittedReport = r.ObstacleReportDate,

                    ObstacleID = r.Obstacle.ObstacleId,
                    ObstacleType = r.Obstacle != null ? r.Obstacle.ObstacleType : "",
                    ObstacleComment = r.Obstacle != null ? r.Obstacle.ObstacleComment : "",
                    Latitude = (r.MapData != null)
                        ? r.MapData.Coordinates
                            .OrderBy(c => c.CoordinateId)
                            .Select(c => (double?)c.Latitude)
                            .FirstOrDefault() ?? 0
                        : 0,

                   Longitude = (r.MapData != null)
                        ? r.MapData.Coordinates
                            .OrderBy(c => c.CoordinateId)
                            .Select(c => (double?)c.Longitude)
                            .FirstOrDefault() ?? 0
                        : 0,

                    ReportStatus = r.ObstacleReportStatus,
                    StatusComment = r.ObstacleReportComment,     // hvis VM har dette feltet

                    UserId = r.User.RoleID,                      // eller r.UserID – avhengig av VM
                    UserName = $"{r.User.FirstName ?? ""} {r.User.LastName ?? ""}".Trim()
                })
                .ToListAsync();

            ViewBag.SelectedStatus = status ?? "Alle";
            return View(model);
        }


    }

}

