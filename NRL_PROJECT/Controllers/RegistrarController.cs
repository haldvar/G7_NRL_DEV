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
                ReportId = report.Report_Id,
                TimeOfSubmittedReport = report.Time_of_Submitted_Report,
                ObstacleID = report.ObstacleReportID,
                ObstacleType = report.Obstacle?.ObstacleType ?? "",
                ObstacleDescription = report.Obstacle?.ObstacleComment ?? "",
                Latitude = report.Obstacle?.Latitude ?? 0,
                Longitude = report.Obstacle?.Longitude ?? 0,
                ReportStatus = report.ReportStatus,   // enum
                UserId = report.User.RoleID,    // eller .Id hvis det er det du bruker
                UserName = $"{report.User.FirstName ?? ""} {report.User.LastName ?? ""}".Trim(),
                GeoJsonCoordinates = report.Reported_Location
            };

            return View(vm); // Views/Registrar/ReportDetails.cshtml (model: ObstacleReportViewModel)
        }

        // Henter alle rapporter
        [HttpGet]
        public async Task<IActionResult> RegistrarView(string? status = null, string? q = null)
        {
            var query = _context.ObstacleReports
                .Include(r => r.Obstacle)   // hinderdetaljer
                .Include(r => r.User)       // innsender
                .AsQueryable();

            // --------- Filter på status (querystring) ----------
            // status kommer som string (f.eks. "Godkjent") -> parse til enum én gang
            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<ReportStatus>(status, ignoreCase: true, out var parsedStatus))
            {
                query = query.Where(r => r.ReportStatus == parsedStatus);
            }

            // --------- Fritekstsøk ----------
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(r =>
                    r.Report_Id.ToString().Contains(q) ||
                    (r.Obstacle.ObstacleType ?? "").Contains(q) ||
                    ((r.User.FirstName ?? "") + " " + (r.User.LastName ?? "")).Contains(q) ||
                    (r.User.FirstName?? "").Contains(q)
                );
            }

            // --------- Projiser til ViewModel ----------
            var model = await query
                .OrderByDescending(r => r.Time_of_Submitted_Report)
                .Select(r => new ObstacleReportViewModel
                {
                    ReportId = r.Report_Id,
                    TimeOfSubmittedReport = r.Time_of_Submitted_Report,
                    ObstacleID = r.ObstacleReportID,
                    ObstacleType = r.Obstacle != null ? r.Obstacle.ObstacleType : "",
                    ObstacleDescription = r.Obstacle != null ? r.Obstacle.ObstacleComment : "",
                    Latitude = r.Obstacle != null ? r.Obstacle.Latitude : 0,
                    Longitude = r.Obstacle != null ? r.Obstacle.Longitude : 0,

                    // Entitetens status er enum -> ViewModelens enum (direkte)
                    ReportStatus = r.ReportStatus,

                    UserId = r.User.RoleID,
                    UserName = $"{r.User.FirstName ?? ""} {r.User.LastName ?? ""}".Trim()
                })
                .ToListAsync();

            return View(model); // List<ObstacleReportViewModel>
        }

    }
}
