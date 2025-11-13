using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
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
        [Authorize(Roles = "Admin,Registrar")]
        public class RegistrarController : Controller
        {
            private readonly NRL_Db_Context _context;

        public RegistrarController(NRL_Db_Context context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
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
        private static readonly List<string> PredefinedObstacleTypes = new()
            {
                "Radio/Mobilmast",
                "Mast/Tårn",
                "Vindmølle",
                "Lyktestolpe",
                "Høyspentledning",
                "Bygning/Konstruksjon",
                "Kran",
                "Annet"
            };
        private readonly UserManager<User> _userManager;  // bruk din User-type
       
        private async Task<List<(string Id, string Name)>> GetRegistrarsAsync()
        {
            var regs = await _userManager.GetUsersInRoleAsync("Registrar");
            return regs.OrderBy(u => u.UserName)
                       .Select(u => (u.Id, u.UserName))
                       .ToList();
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
                    .ThenInclude(u => u.Organisation)
                .Include(r => r.MapData)
                    .ThenInclude(m => m.Coordinates)
                .FirstOrDefaultAsync(r => r.ObstacleReportID == id);

            if (report == null)
                return NotFound();
            var OrgName = "Ukjent";

            // ---- Koordinater ----
            var orderedCoords = report.MapData?.Coordinates?
                .OrderBy(c => c.CoordinateId)   
                .ToList();

            // Første punkt for kompatibilitet (Latitude/Longitude i VM)
            var first = orderedCoords?.FirstOrDefault();
            var lat = first?.Latitude ?? 0;
            var lng = first?.Longitude ?? 0;

            // Lag en liste av [lat, lng]
            var latLngPairs = orderedCoords != null
                ? orderedCoords.Select(c => new[] { c.Latitude, c.Longitude }).ToList()
                : new List<double[]>();

            // JSON -> Model.GeoJsonCoordinates
            var json = JsonSerializer.Serialize(latLngPairs,
                new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.Never });

            var reportedLocation = first != null ? $"{lat},{lng}" : string.Empty;
            var registrars = await GetRegistrarsAsync();

            ViewBag.Registrars = registrars
                .Select(x => new SelectListItem
                {
                    Value = x.Id,
                    Text = x.Name,
                    Selected = (x.Id == report.ReviewedByUserID) // nåværende eier
                })
                .ToList();

            var typeList = await _context.Obstacles
                .Select(o => o.ObstacleType)
                .Where(t => t != null && t != "")
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            // ---- Bygg ViewModel null-sikkert ----
            var vm = new ObstacleReportViewModel
            {
                // Innsender
                ObstacleReportID = report.ObstacleReportID,
                TimeOfSubmittedReport = report.ObstacleReportDate,

                // Hindring
                ObstacleID = report.Obstacle?.ObstacleID ?? 0,
                ObstacleType = report.Obstacle?.ObstacleType ?? "",
                ObstacleComment = report.Obstacle?.ObstacleComment ?? "",
                ObstacleHeight = report.Obstacle?.ObstacleHeight ?? 0,
                ObstacleWidth = report.Obstacle?.ObstacleWidth ?? 0,

                // Lokasjon
                Latitude = lat,
                Longitude = lng,
                Reported_Location = reportedLocation,
                GeoJsonCoordinates = json,

                // Status
                ReportStatus = MapToUi(report.ObstacleReportStatus),

                // Bruker
                UserID = report.User?.Id ?? "",
                UserName = report.User != null
                    ? $"{(report.User.FirstName ?? "").Trim()} {(report.User.LastName ?? "").Trim()}".Trim()
                    : "Ukjent",
                OrgName = report.User?.Organisation != null
                    ? (report.User.Organisation.OrgName ?? "Ukjent")
                    : "Ukjent",
                AssignedRegistrarUserID = report.ReviewedByUserID
            };

            ViewBag.ObstacleTypes = PredefinedObstacleTypes
                .Select(t => new SelectListItem { Value = t, Text = t, Selected = (t == vm.ObstacleType) })
                .ToList();

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateObstacleData(int id, string? obstacleType, double? obstacleHeight)
        {
            var report = await _context.ObstacleReports
                .Include(r => r.Obstacle)   // <- viktig
                .FirstOrDefaultAsync(r => r.ObstacleReportID == id);

            if (report == null)
            {
                TempData["Error"] = "Rapporten finnes ikke.";
                return RedirectToAction(nameof(RegistrarView));
            }

            // enkel validering
            if (obstacleHeight is < 0 or > 10000)
            {
                TempData["Error"] = "Ugyldig høyde (0–10 000).";
                return RedirectToAction(nameof(ReportDetails), new { id });
            }

            // Opprett Obstacle hvis den mangler (sikkerhetsnett)
            if (report.Obstacle == null)
                report.Obstacle = new ObstacleData();

            // Hvis bruker velger “Annet” og skriver tekst selv, lar vi feltet være fritt
            if (!string.IsNullOrWhiteSpace(obstacleType))
                report.Obstacle.ObstacleType = obstacleType.Trim();

            if (obstacleHeight.HasValue)
                report.Obstacle.ObstacleHeight = obstacleHeight.Value;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Hindring oppdatert.";
            return RedirectToAction(nameof(ReportDetails), new { id });
        }


        //Transfer report to another registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferReport(int id, string transferToUserId)
        {
            if (string.IsNullOrWhiteSpace(transferToUserId))
            {
                TempData["Error"] = "Velg en registerfører.";
                return RedirectToAction(nameof(ReportDetails), new { id });
            }

            var report = await _context.ObstacleReports
                .FirstOrDefaultAsync(r => r.ObstacleReportID == id);

            if (report == null)
            {
                TempData["Error"] = "Rapporten finnes ikke.";
                return RedirectToAction(nameof(RegistrarView));
            }

            // ✅ Sikkerhetsjekk: sørg for at valgt bruker faktisk ER registerfører
            var targetUser = await _userManager.FindByIdAsync(transferToUserId);
            if (targetUser == null || !(await _userManager.IsInRoleAsync(targetUser, "Registrar")))
            {
                TempData["Error"] = "Valgt bruker er ikke registerfører.";
                return RedirectToAction(nameof(ReportDetails), new { id });
            }

            // ✅ Alt OK — overfør rapporten
            report.ReviewedByUserID = transferToUserId;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rapporten er overført.";
            return RedirectToAction(nameof(ReportDetails), new { id });
        }


        // Gets all reports
        [HttpGet]
        public async Task<IActionResult> RegistrarView(string? status = "Alle", string? q = null)
        {
            var query = _context.ObstacleReports
                .Include(r => r.Obstacle)
                .Include(r => r.User)
                    .ThenInclude(u => u.Organisation)
                .Include(r => r.Reviewer)
                .Include(r => r.MapData)
                   .ThenInclude(m => m.Coordinates)
                .AsQueryable();

            // Rens eventuelle "Alle (2)" -> "Alle"
            if (!string.IsNullOrWhiteSpace(status) &&
                status.StartsWith("Alle", StringComparison.OrdinalIgnoreCase))
            {
                status = "Alle";
            }

            if (!string.IsNullOrWhiteSpace(status) &&
                !string.Equals(status, "Alle", StringComparison.OrdinalIgnoreCase))
            {
                EnumTypes? filterStatus = status switch
                {
                    "Ny" => EnumTypes.New,
                    "Venter" => EnumTypes.Open,
                    "UnderBehandling" => EnumTypes.InProgress,
                    "Godkjent" => EnumTypes.Resolved,
                    "Avvist" => EnumTypes.Deleted,
                    _ => null
                };

                if (filterStatus.HasValue)
                {
                    query = query.Where(r => r.ObstacleReportStatus == filterStatus.Value);
                }
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
          ObstacleReportID = r.ObstacleReportID,

          // DateTime? -> DateTime
          TimeOfSubmittedReport = r.ObstacleReportDate,

          // Relasjon kan være null
          ObstacleID = r.Obstacle != null ? r.Obstacle.ObstacleID : 0,
          ObstacleType = r.Obstacle != null ? (r.Obstacle.ObstacleType ?? "") : "",
          ObstacleComment = r.Obstacle != null ? (r.Obstacle.ObstacleComment ?? "") : "",
          ObstacleHeight = r.Obstacle != null ? r.Obstacle.ObstacleHeight : 0,

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
          UserID = r.UserID ?? "",
          UserName = r.User != null
              ? $"{(r.User.FirstName ?? "").Trim()} {(r.User.LastName ?? "").Trim()}".Trim()
              : "Ukjent",
          OrgName = r.User != null && r.User.Organisation != null
            ? (r.User.Organisation.OrgName ?? "Ukjent")
            : "Ukjent",

          AssignedRegistrarUserID = r.Reviewer != null
            ? r.Reviewer.UserName
            : null,

      })
      .ToListAsync();


            ViewBag.SelectedStatus = status ?? "Alle";
            return View(model);
        }


    }

}

