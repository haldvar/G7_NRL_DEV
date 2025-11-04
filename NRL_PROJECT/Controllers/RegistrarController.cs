using Microsoft.AspNetCore.Mvc;
using NRL_PROJECT.Data;
using Microsoft.AspNetCore.Mvc;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> UpdateStatus(int id, ReportStatus status, string? comment)
        {
            var report = await _context.ObstacleReports.FirstOrDefaultAsync(r => r.Report_Id == id);
            if (report == null) return NotFound();

            report.ReportStatus = status;
            report.StatusComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
            report.StatusChangedAt = DateTime.UtcNow;              // du kan bruke local time om ønskelig
            report.HandledBy = User?.Identity?.Name ?? "Registrar"; // senere: faktisk bruker

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
                .FirstOrDefaultAsync(r => r.Report_Id == id);

            if (report == null) return NotFound();

            var vm = new ObstacleReportViewModel
            {
                // innsending
                ReportId = report.Report_Id,
                TimeOfSubmittedReport = report.Time_of_Submitted_Report,

                // hinder
                ObstacleID = report.ObstacleReportID,
                ObstacleType = report.Obstacle?.ObstacleType,
                ObstacleComment = report.Obstacle?.ObstacleComment,
                ObstacleHeight = report.Obstacle?.ObstacleHeight,
                ObstacleWidth = report.Obstacle?.ObstacleWidth,

                // lokasjon – bruk tall om de finnes, ellers la Reported_Location være med
                Latitude = report.Obstacle?.Latitude ?? 0,
                Longitude = report.Obstacle?.Longitude ?? 0,
                Reported_Location = report.Reported_Location,         // ← denne kan du vise/parse i view
                GeoJsonCoordinates = report.Reported_Location,         // hvis du vil gjenbruke navnet

                // status
                ReportStatus = report.ReportStatus,

                // innsender
                UserId = report.User.RoleID,              
                UserName = $"{report.User.FirstName ?? ""} {report.User.LastName ?? ""}".Trim()
            };

            return View(vm); // Views/Registrar/ReportDetails.cshtml
        }

        // Henter alle rapporter
        [HttpGet]
        public async Task<IActionResult> RegistrarView(string? status = "Alle", string? q = null)
        {
            var query = _context.ObstacleReports
                .Include(r => r.Obstacle)
                .Include(r => r.User)
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
                Enum.TryParse<ReportStatus>(status, ignoreCase: true, out var parsed))
            {
                query = query.Where(r => r.ReportStatus == parsed);
            }

            // (valgfritt) fritekstsøk
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(r =>
                    r.Report_Id.ToString().Contains(q) ||
                    (r.Obstacle.ObstacleType ?? "").Contains(q) ||
                    ((r.User.FirstName ?? "") + " " + (r.User.LastName ?? "")).Contains(q) ||
                    (r.User.FirstName ?? "").Contains(q));
            }

            var model = await query
                .OrderByDescending(r => r.Time_of_Submitted_Report)
                .Select(r => new ObstacleReportViewModel
                {
                    ReportId = r.Report_Id,
                    TimeOfSubmittedReport = r.Time_of_Submitted_Report,
                    ObstacleID = r.ObstacleReportID,
                    ObstacleType = r.Obstacle != null ? r.Obstacle.ObstacleType : "",
                    ObstacleComment = r.Obstacle != null ? r.Obstacle.ObstacleComment : "",
                    Latitude = r.Obstacle != null ? r.Obstacle.Latitude : 0,
                    Longitude = r.Obstacle != null ? r.Obstacle.Longitude : 0,
                    ReportStatus = r.ReportStatus,
                    StatusComment = r.StatusComment,
                    StatusChangedAt = r.StatusChangedAt,
                    HandledBy = r.HandledBy,
                    UserId = r.User.RoleID,
                    UserName = $"{r.User.FirstName ?? ""} {r.User.LastName ?? ""}".Trim()
                })
                .ToListAsync();

            ViewBag.SelectedStatus = status ?? "Alle";
            return View(model);
        }


    }

}

