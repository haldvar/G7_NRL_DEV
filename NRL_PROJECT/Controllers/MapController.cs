using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace NRL_PROJECT.Controllers
{
    /// <summary>
    /// Handles map pages and operations:
    /// - Renders map views and map confirmation.
    /// - Accepts obstacle submissions with GeoJSON, saves MapData, Obstacle and ObstacleReport.
    /// - Provides data endpoints for obstacles and report listings.
    /// </summary>
    public class MapController : Controller
    {
        private readonly NRL_Db_Context _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<User> _userManager;

        public MapController(NRL_Db_Context context, IWebHostEnvironment environment, UserManager<User> userManager)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
        }

        // ---------------------------
        // Simple GET views
        // ---------------------------

        // GET: /Map/ObstacleAndMapForm
        [HttpGet]
        public IActionResult ObstacleAndMapForm()
        {
            // Provide a default ObstacleData with MapData defaults for the form
            var model = new ObstacleData
            {
                MapData = new MapData
                {
                    GeometryType = "Point",
                    MapZoomLevel = 13
                }
            };

            return View(model);
        }

        // GET: /Map/MapView
        [Authorize]
        public async Task<IActionResult> MapView(int? id)
        {
            // 1) Load all reports with GeoJSON + MapData + User info
            var reportsWithGeometry = await _context.ObstacleReports
                .Include(r => r.MapData)
                    .ThenInclude(m => m.Coordinates)
                .Include(r => r.SubmittedByUser)
                .Include(r => r.Obstacle)
                .Where(r => r.MapData != null &&
                    !string.IsNullOrWhiteSpace(r.MapData.GeoJsonCoordinates))
                .ToListAsync();

            // 2) Create a MapData to function as ViewModel
            var model = new MapData
            {
                ObstacleReports = reportsWithGeometry,
                Coordinates = new List<MapCoordinate>()
            };

            // 3) Place initial view based on first report
            var firstReport = reportsWithGeometry.FirstOrDefault();
            if (firstReport != null && firstReport.MapData?.Coordinates?.Any() == true)
            {
                var firstCoord = firstReport.MapData.Coordinates
                    .OrderBy(c => c.OrderIndex)
                    .First();

                model.Coordinates.Add(new MapCoordinate
                {
                    Latitude = firstCoord.Latitude,
                    Longitude = firstCoord.Longitude,
                    OrderIndex = 0
                });

                model.MapZoomLevel = firstReport.MapData.MapZoomLevel != 0 ? firstReport.MapData.MapZoomLevel : 12;
            }
            else
            {
                // fallback: norge
                model.MapZoomLevel = 6;
                model.Coordinates.Add(new MapCoordinate
                {
                    Latitude = 64.0,
                    Longitude = 11.0,
                    OrderIndex = 0
                });
            }

            return View(model);
        }

        // GET: /Map/MapConfirmation
        [HttpGet]
        public IActionResult MapConfirmation()
        {
            return View();
        }

        // ---------------------------
        // Submission flows (POST)
        // ---------------------------

        // POST: /Map/Submit
        [HttpPost]
        public IActionResult Submit(MapData mapdata)
        {
            if (!ModelState.IsValid)
                return View("MapView", mapdata);

            return View("MapConfirmation", mapdata);
        }

        // POST: /Map/SubmitObstacleWithLocation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitObstacleWithLocation(ObstacleData model)
        {
            // Ensure MapData exists
            model.MapData ??= new MapData();

            if (string.IsNullOrWhiteSpace(model.ObstacleComment))
                model.ObstacleComment = "Ingen kommentar registrert";

            if (string.IsNullOrWhiteSpace(model.MapData.GeoJsonCoordinates))
            {
                ModelState.AddModelError("", "Du må minst angi ett punkt på kartet.");
                return View("ObstacleAndMapForm", model);
            }

            // 1) Parse GeoJSON geometry and build MapCoordinate list
            var coords = new List<MapCoordinate>();

            using (var doc = JsonDocument.Parse(model.MapData.GeoJsonCoordinates))
            {
                var root = doc.RootElement;

                // Store clean GeoJSON text (MapConfirmation expects this)
                model.MapData.GeoJsonCoordinates = root.GetRawText();

                var type = root.GetProperty("type").GetString();

                if (type == "Point")
                {
                    var arr = root.GetProperty("coordinates").EnumerateArray().ToList();

                    coords.Add(new MapCoordinate
                    {
                        Longitude = arr[0].GetDouble(),
                        Latitude = arr[1].GetDouble(),
                        OrderIndex = 0
                    });

                    model.MapData.GeometryType = "Point";
                }
                else if (type == "LineString")
                {
                    var arr = root.GetProperty("coordinates").EnumerateArray().ToList();

                    for (int i = 0; i < arr.Count; i++)
                    {
                        var pair = arr[i].EnumerateArray().ToList();

                        coords.Add(new MapCoordinate
                        {
                            Longitude = pair[0].GetDouble(),
                            Latitude = pair[1].GetDouble(),
                            OrderIndex = i
                        });
                    }

                    model.MapData.GeometryType = "LineString";
                }
                else
                {
                    ModelState.AddModelError("", $"Ugyldig eller ustøttet GeoJSON-type: {type}");
                    return View("ObstacleAndMapForm", model);
                }
            }

            // 2) Assign coordinates and zoom
            model.MapData.Coordinates = coords;
            model.MapData.MapZoomLevel = model.MapData.MapZoomLevel != 0 ? model.MapData.MapZoomLevel : 13;

       // 3) VALIDATE IMAGE FIRST (before saving anything)
string? imagePath = null;

if (model.ImageFile != null && model.ImageFile.Length > 0)
{
    // Validation: allowed extensions
    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".heic", ".webp" };
    var extension = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();

    if (!allowedExtensions.Contains(extension))
    {
        ModelState.AddModelError("ImageFile", "Kun bildefiler (.jpg, .jpeg, .png, .heic, .webp) er tillatt");
        return View("ObstacleAndMapForm", model);
    }

    // Validation: max file size (5 MB)
    if (model.ImageFile.Length > 5 * 1024 * 1024)
    {
        ModelState.AddModelError("ImageFile", "Bildet må være mindre enn 5MB");
        return View("ObstacleAndMapForm", model);
    }

    // Saving logic
    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

    if (!Directory.Exists(uploadsFolder))
        Directory.CreateDirectory(uploadsFolder);

    var uniqueFileName = Guid.NewGuid() + extension;
    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

    using var stream = new FileStream(filePath, FileMode.Create);
    await model.ImageFile.CopyToAsync(stream);

    imagePath = "/uploads/" + uniqueFileName;
}

// 4) NOW Save MapData (only after image validation passes)
_context.MapDatas.Add(model.MapData);
await _context.SaveChangesAsync();

            // 5) Save Obstacle
            model.ObstacleImageURL = imagePath;
            _context.Obstacles.Add(model);
            await _context.SaveChangesAsync();

            // 6) Create and save report
            var currentUserId = _userManager.GetUserId(User);
            var currentUser = await _userManager.GetUserAsync(User);

            var report = new ObstacleReportData
            {
                ObstacleID = model.ObstacleID,
                SubmittedByUserId = currentUserId,
                ObstacleReportComment = null,
                ObstacleReportDate = DateTime.UtcNow.AddHours(1),
                ObstacleReportStatus = ObstacleReportData.EnumTypes.New,
                MapDataID = model.MapData?.MapDataID,
                CoordinateSummary = model.MapData?.CoordinateSummary ?? string.Empty
            };

            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

            // 7) Show confirmation view with saved MapData
            return View("MapConfirmation", model.MapData);
        }

        // POST: /Map/SubmitObstacleReport
        [HttpPost]
        public async Task<IActionResult> SubmitObstacleReport(int ObstacleID, string geoJson)
        {
            var obstacle = await _context.Obstacles.FindAsync(ObstacleID);
            if (obstacle == null)
                return NotFound();

            var mapData = new MapData
            {
                GeoJsonCoordinates = geoJson,
                MapZoomLevel = 13
            };

            _context.MapDatas.Add(mapData);
            await _context.SaveChangesAsync();

            var report = new ObstacleReportData
            {
                ObstacleID = obstacle.ObstacleID,
                ObstacleReportComment = "New report submitted.",
                ObstacleReportDate = DateTime.UtcNow.AddHours(1),
                ObstacleReportStatus = ObstacleReportData.EnumTypes.New,
                MapDataID = mapData.MapDataID,
                SubmittedByUserId = null,
                ReviewedByUserID = null,
            };

            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ReportListOverview));
        }

        // ---------------------------
        // Data and listing endpoints
        // ---------------------------

        // GET: /Map/ReportListOverview
        [HttpGet]
        public async Task<IActionResult> ReportListOverview()
        {
            var reports = await _context.ObstacleReports
                .Include(r => r.Obstacle)
                .Include(r => r.SubmittedByUser)
                .Include(r => r.Reviewer)
                .Include(r => r.MapData)
                    .ThenInclude(m => m.Coordinates)
                .OrderByDescending(r => r.ObstacleReportDate)
                .ToListAsync();

            return View(reports);
        }

        // GET: /Map/MyReports
        [HttpGet]
        public async Task<IActionResult> MyReports()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var reports = await _context.ObstacleReports
                .Include(r => r.Obstacle)
                .Include(r => r.SubmittedByUser)
                .Include(r => r.Reviewer)
                .Include(r => r.MapData)
                    .ThenInclude(m => m.Coordinates)
                .Where(r => r.SubmittedByUserId == userId)
                .OrderByDescending(r => r.ObstacleReportDate)
                .ToListAsync();

            return View("MyReports", reports);
        }

        // GET: /Map/GetObstacles
        [HttpGet]
        public async Task<IActionResult> GetObstacles()
        {
            var obstacles = await _context.Obstacles
                .Include(o => o.MapData)
                    .ThenInclude(m => m.Coordinates)
                .ToListAsync();

            var features = obstacles.Select(o =>
            {
                object geometry;

                if (o.MapData.GeometryType == "LineString")
                {
                    geometry = new
                    {
                        type = "LineString",
                        coordinates = o.MapData.Coordinates
                            .OrderBy(c => c.OrderIndex)
                            .Select(c => new[] { c.Longitude, c.Latitude })
                    };
                }
                else
                {
                    var c = o.MapData.Coordinates.First();
                    geometry = new
                    {
                        type = "Point",
                        coordinates = new[] { c.Longitude, c.Latitude }
                    };
                }

                return new
                {
                    type = "Feature",
                    geometry,
                    properties = new
                    {
                        id = o.ObstacleID,
                        type = o.ObstacleType,
                        height = o.ObstacleHeight,
                        width = o.ObstacleWidth,
                        comment = o.ObstacleComment
                    }
                };
            });

            var geojson = new
            {
                type = "FeatureCollection",
                features
            };

            return Json(geojson);
        }
    }
}
