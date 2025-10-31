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

        // Henter alle rapporter
        public async Task<IActionResult> RegistrarView()
            {
                var reports = await _context.ObstacleReports
                    .Include(r => r.Obstacle)  // kobler til hinderdetaljer
                    .OrderByDescending(r => r.Time_of_Submitted_Report)
                    .ToListAsync();

                return View(reports);
            }

            // Viser detaljer om én rapport
            public async Task<IActionResult> ReportDetails(int id)
            {
                var report = await _context.ObstacleReports
                    .Include(r => r.Obstacle)
                    .FirstOrDefaultAsync(r => r.Report_Id == id);

                if (report == null)
                    return NotFound();

            var vm = new ObstacleReportViewModel
            {
                ReportId = report.Report_Id,
                TimeOfSubmittedReport = report.Time_of_Submitted_Report,
                ObstacleId = report.ObstacleId,
                ObstacleName = report.Obstacle?.ObstacleName,
                ObstacleType = report.Obstacle?.ObstacleType,
                ObstacleWidth = report.Obstacle?.ObstacleWidth ?? 0,
                ObstacleDescription = report.Obstacle?.ObstacleDescription,
                Latitude = report.Obstacle?.Latitude ?? 0,
                Longitude = report.Obstacle?.Longitude ?? 0,
                ReportStatus = report.ReportStatus
            };

            return View(report);
        }

    }
}
