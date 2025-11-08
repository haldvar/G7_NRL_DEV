using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using BackendStatus = NRL_PROJECT.Models.ObstacleReportData.EnumTypes;
using UiStatus = NRL_PROJECT.Models.EnumStatus;
using System.Linq;
using System.Net.NetworkInformation;
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
            private static UiStatus MapToUi(BackendStatus backendStatus)
            {
                return backendStatus switch
                {
                    BackendStatus.New => UiStatus.Ny,
                    BackendStatus.Open => UiStatus.Venter,
                    BackendStatus.InProgress => UiStatus.UnderBehandling,
                    BackendStatus.Resolved => UiStatus.Godkjent,
                    BackendStatus.Closed => UiStatus.Godkjent,
                    BackendStatus.Deleted => UiStatus.Avvist,
                    _ => UiStatus.Ny
                };
        }
        //POST: /Registrar/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? comment)
        {
            var report = await _context.ObstacleReports
                .FirstOrDefaultAsync(r => r.ObstacleReportID == id);

            if (report == null) return NotFound();

            
            var newBackendStatus = status switch
            {
                "UnderBehandling" => BackendStatus.InProgress,
                "Godkjent" => BackendStatus.Resolved,   
                "Avvist" => BackendStatus.Deleted,    
                _ => BackendStatus.Open        // “Ny/Venter”
            };

            report.ObstacleReportStatus = newBackendStatus;
            report.ObstacleReportComment = (comment ?? "").Trim();

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ReportDetails), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> ReportDetails(int id)
        {
            var report = await _context.ObstacleReports
                .Include(r => r.Obstacle)
                .Include(r => r.User)
                .Include(r => r.MapData)
                    .ThenInclude(m => m.Coordinates)
                .FirstOrDefaultAsync(r => r.ObstacleReportID == id);

            if (report == null)
                return NotFound();

            // ---- Koordinater ----
            var firstCoord = report.MapData?.Coordinates?
                .OrderBy(c => c.MapDataID)
                .FirstOrDefault();

            var lat = firstCoord?.Latitude ?? 0;
            var lng = firstCoord?.Longitude ?? 0;

            var reportedLocation = firstCoord != null
                ? $"{lat},{lng}"
                : string.Empty;

            var geoJson = report.MapData?.GeoJsonCoordinates ?? string.Empty;

            // ---- Bygg ViewModel null-sikkert ----
            var vm = new ObstacleReportViewModel
            {
                // Innsender
                ReportId = report.ObstacleReportID,
                TimeOfSubmittedReport = report.ObstacleReportDate,

                // Hindring
                ObstacleID = report.Obstacle?.ObstacleId ?? 0,
                ObstacleType = report.Obstacle?.ObstacleType ?? "",
                ObstacleComment = report.Obstacle?.ObstacleComment ?? "",
                ObstacleHeight = report.Obstacle?.ObstacleHeight ?? 0,
                ObstacleWidth = report.Obstacle?.ObstacleWidth ?? 0,

                // Lokasjon
                Latitude = lat,
                Longitude = lng,
                Reported_Location = reportedLocation,
                GeoJsonCoordinates = geoJson,

                // Status
                ReportStatus = MapToUi(report.ObstacleReportStatus),

                // Bruker
                UserId = report.UserID ?? "",  // Changed: UserID is now string
                UserName = report.User != null
                    ? $"{(report.User.FirstName ?? "").Trim()} {(report.User.LastName ?? "").Trim()}".Trim()
                    : "Ukjent"
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
      .OrderByDescending(r => r.ObstacleReportDate)
      .Select(r => new ObstacleReportViewModel
      {
          ReportId = r.ObstacleReportID,

          // DateTime? -> DateTime
          TimeOfSubmittedReport = r.ObstacleReportDate,

          // Relasjon kan være null
          ObstacleID = r.Obstacle != null ? r.Obstacle.ObstacleId : 0,
          ObstacleType = r.Obstacle != null ? (r.Obstacle.ObstacleType ?? "") : "",
          ObstacleComment = r.Obstacle != null ? (r.Obstacle.ObstacleComment ?? "") : "",

          // Koordinater: MapData kan være null, og lista kan være tom
          Latitude =
              (r.MapData != null && r.MapData.Coordinates.Any())
                  ? r.MapData.Coordinates
                      .OrderBy(c => c.CoordinateId)
                      .Select(c => (double?)c.Latitude)
                      .FirstOrDefault() ?? 0
                  : 0,

          Longitude =
              (r.MapData != null && r.MapData.Coordinates.Any())
                  ? r.MapData.Coordinates
                      .OrderBy(c => c.CoordinateId)
                      .Select(c => (double?)c.Longitude)
                      .FirstOrDefault() ?? 0
                  : 0,

          // Status/kommentar
          ReportStatus = MapToUi(r.ObstacleReportStatus),       
          StatusComment = r.ObstacleReportComment ?? "",

          // Bruker
          UserId = r.UserID ?? "",  // Changed: UserID is now string
          UserName = r.User != null
              ? $"{(r.User.FirstName ?? "").Trim()} {(r.User.LastName ?? "").Trim()}".Trim()
              : "Ukjent"
      })
      .ToListAsync();


            ViewBag.SelectedStatus = status ?? "Alle";
            return View(model);
        }


    }

}

