using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using static NRL_PROJECT.Models.ObstacleReportData;
using BackendStatus = NRL_PROJECT.Models.ObstacleReportData.EnumTypes;
using UiStatus = NRL_PROJECT.Models.EnumStatus;

namespace NRL_PROJECT.Controllers
{
    [Authorize(Roles = "Admin,Registrar")]
    public class RegistrarController : Controller
    {
        private readonly NRL_Db_Context _context;
        private readonly UserManager<User> _userManager;

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

        private static readonly List<String> PredefinedObstacleTypes = new()
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

        private async Task<List<(string Id, string Name)>> GetRegistrarsAsync()
        {
            var regs = await _userManager.GetUsersInRoleAsync("Registrar");
            return regs.OrderBy(u => u.UserName)
                       .Select(u => (u.Id, u.UserName))
                       .ToList();
        }

        // ---------------------------------------------------------------------
        // POST: UpdateStatus
        // ---------------------------------------------------------------------
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
                _ => BackendStatus.Open
            };

            report.ObstacleReportStatus = newBackendStatus;
            report.ObstacleReportComment = (comment ?? "").Trim();

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ReportDetails), new { id });
        }

        // ---------------------------------------------------------------------
        // GET: ReportDetails
        // ---------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> ReportDetails(int id)
        {
            var report = await _context.ObstacleReports
                .Include(r => r.SubmittedByUser)
                    .ThenInclude(u => u.Organisation)
                .Include(r => r.Obstacle)
                .Include(r => r.Reviewer)
                .Include(r => r.MapData)
                    .ThenInclude(m => m.Coordinates)
                .FirstOrDefaultAsync(r => r.ObstacleReportID == id);

            if (report == null) return NotFound();

            // Coordinates
            var orderedCoords = report.MapData?.Coordinates?
                .OrderBy(c => c.CoordinateId)
                .ToList();

            var first = orderedCoords?.FirstOrDefault();
            var lat = first?.Latitude ?? 0;
            var lng = first?.Longitude ?? 0;

            var latLngPairs = orderedCoords != null
                ? orderedCoords.Select(c => new[] { c.Latitude, c.Longitude }).ToList()
                : new List<double[]>();

            var json = JsonSerializer.Serialize(latLngPairs,
                new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.Never });

            var reportedLocation = first != null ? $"{lat},{lng}" : string.Empty;

            // Registrars list
            var registrars = await GetRegistrarsAsync();
            ViewBag.Registrars = registrars
                .Select(x => new SelectListItem
                {
                    Value = x.Id,
                    Text = x.Name,
                    Selected = (x.Id == report.ReviewedByUserID)
                })
                .ToList();

            // Build ViewModel
            var vm = new ObstacleReportViewModel
            {
                ObstacleReportID = report.ObstacleReportID,
                ObstacleReportDate = report.ObstacleReportDate,

                ObstacleID = report.Obstacle?.ObstacleID ?? 0,
                ObstacleType = report.Obstacle?.ObstacleType ?? "",
                ObstacleComment = report.Obstacle?.ObstacleComment ?? "",
                ObstacleHeight = report.Obstacle?.ObstacleHeight ?? 0,
                ObstacleWidth = report.Obstacle?.ObstacleWidth ?? 0,
                ObstacleImageURL = report.Obstacle?.ObstacleImageURL,   // ⭐ INFO FRA OBSTACLE

                MapData = report.MapData,
                Latitude = lat,
                Longitude = lng,
                Reported_Location = reportedLocation,
                GeoJsonCoordinates = json,

                ReportStatus = MapToUi(report.ObstacleReportStatus),
                ObstacleReportComment = report.ObstacleReportComment ?? "",

                UserName = report.SubmittedByUser?.UserName ?? "Ukjent",
                OrgName = report.SubmittedByUser?.Organisation?.OrgName ?? "Ukjent",

                AssignedRegistrarUserID = report.ReviewedByUserID,
                ReviewerName = report.Reviewer?.UserName
            };

            ViewBag.ObstacleTypes = PredefinedObstacleTypes
                .Select(t => new SelectListItem { Value = t, Text = t, Selected = (t == vm.ObstacleType) })
                .ToList();

            return View(vm);
        }

        // ---------------------------------------------------------------------
        // POST: UpdateObstacleData
        // ---------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateObstacleData(int id, string? obstacleType, double? obstacleHeight)
        {
            var report = await _context.ObstacleReports
                .Include(r => r.Obstacle)
                .FirstOrDefaultAsync(r => r.ObstacleReportID == id);

            if (report == null)
            {
                TempData["Error"] = "Rapporten finnes ikke.";
                return RedirectToAction(nameof(RegistrarView));
            }

            if (obstacleHeight is < 0 or > 10000)
            {
                TempData["Error"] = "Ugyldig høyde (0–10 000).";
                return RedirectToAction(nameof(ReportDetails), new { id });
            }

            if (report.Obstacle == null)
                report.Obstacle = new ObstacleData();

            if (!string.IsNullOrWhiteSpace(obstacleType))
                report.Obstacle.ObstacleType = obstacleType.Trim();

            if (obstacleHeight.HasValue)
                report.Obstacle.ObstacleHeight = obstacleHeight.Value;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Hindring oppdatert.";
            return RedirectToAction(nameof(ReportDetails), new { id });
        }

        // ---------------------------------------------------------------------
        // POST: TransferReport
        // ---------------------------------------------------------------------
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

            var targetUser = await _userManager.FindByIdAsync(transferToUserId);
            if (targetUser == null || !(await _userManager.IsInRoleAsync(targetUser, "Registrar")))
            {
                TempData["Error"] = "Valgt bruker er ikke registerfører.";
                return RedirectToAction(nameof(ReportDetails), new { id });
            }

            report.ReviewedByUserID = transferToUserId;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rapporten er overført.";
            return RedirectToAction(nameof(ReportDetails), new { id });
        }

        // ---------------------------------------------------------------------
        // GET: RegistrarView (overview)
        // ---------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> RegistrarView(string? status = "Alle", string? q = null)
        {
            var query = _context.ObstacleReports
                .Include(r => r.Obstacle)
                .Include(r => r.SubmittedByUser)
                    .ThenInclude(u => u.Organisation)
                .Include(r => r.Reviewer)
                .Include(r => r.MapData)
                    .ThenInclude(m => m.Coordinates)
                .AsQueryable();

            // Statusfilter
            if (!string.IsNullOrWhiteSpace(status) &&
                !status.Equals("Alle", StringComparison.OrdinalIgnoreCase))
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
                    query = query.Where(r => r.ObstacleReportStatus == filterStatus.Value);
            }

            // Søkeord
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();

                query = query.Where(r =>
                    r.ObstacleReportID.ToString().Contains(q) ||
                    (r.Obstacle != null ? r.Obstacle.ObstacleType : "").Contains(q) ||
                    (
                        (r.SubmittedByUser != null ? r.SubmittedByUser.FirstName : "") + " " +
                        (r.SubmittedByUser != null ? r.SubmittedByUser.LastName : "")
                    ).Contains(q)
                );
            }

            // SELECT → ViewModel
            var model = await query
                .OrderByDescending(r => r.ObstacleReportDate)
                .Select(r => new ObstacleReportViewModel
                {
                    ObstacleReportID = r.ObstacleReportID,
                    ObstacleReportDate = r.ObstacleReportDate,

                    ObstacleID = r.Obstacle != null ? r.Obstacle.ObstacleID : 0,
                    ObstacleType = r.Obstacle != null ? (r.Obstacle.ObstacleType ?? "") : "",
                    ObstacleComment = r.Obstacle != null ? (r.Obstacle.ObstacleComment ?? "") : "",
                    ObstacleHeight = r.Obstacle != null ? r.Obstacle.ObstacleHeight : 0,
                    ObstacleImageURL = r.Obstacle != null ? r.Obstacle.ObstacleImageURL : null,   // ⭐ FOR LISTEVISNING

                    Latitude = (r.MapData != null && r.MapData.Coordinates.Any())
                        ? r.MapData.Coordinates
                            .OrderBy(c => c.CoordinateId)
                            .Select(c => (double?)c.Latitude)
                            .FirstOrDefault() ?? 0
                        : 0,

                    Longitude = (r.MapData != null && r.MapData.Coordinates.Any())
                        ? r.MapData.Coordinates
                            .OrderBy(c => c.CoordinateId)
                            .Select(c => (double?)c.Longitude)
                            .FirstOrDefault() ?? 0
                        : 0,

                    ReportStatus = MapToUi(r.ObstacleReportStatus),
                    ObstacleReportComment = r.ObstacleReportComment ?? "",

                    UserName = r.SubmittedByUser != null
                        ? r.SubmittedByUser.UserName
                        : "Ukjent",

                    OrgName = r.SubmittedByUser != null &&
                              r.SubmittedByUser.Organisation != null
                              ? r.SubmittedByUser.Organisation.OrgName
                              : "Ukjent",

                    AssignedRegistrarUserID = r.Reviewer != null ? r.Reviewer.Id : null,
                    ReviewerName = r.Reviewer != null ? r.Reviewer.UserName : null
                })
                .ToListAsync();

            ViewBag.SelectedStatus = status ?? "Alle";
            return View(model);
        }
    }
}
