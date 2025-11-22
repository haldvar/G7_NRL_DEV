using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using System.Text.Json;

namespace NRL_PROJECT.Controllers
{
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

        // GET: /Map/ObstacleAndMapForm
        [HttpGet]
        public IActionResult ObstacleAndMapForm()
        {
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

        // POST: /Map/SubmitObstacleWithLocation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitObstacleWithLocation(ObstacleData model)
        {
            // Sikre at MapData alltid finnes
            model.MapData ??= new MapData();

            if (string.IsNullOrWhiteSpace(model.ObstacleComment))
                model.ObstacleComment = "No comment registred";

            if (string.IsNullOrWhiteSpace(model.MapData.GeoJsonCoordinates))
            {
                ModelState.AddModelError("", "Du må minst angi ett punkt på kartet.");
                return View("ObstacleAndMapForm", model);
            }

            // ================================
            // 1) Parse GeoJSON
            // ================================
            var coords = new List<MapCoordinate>();

            using (var doc = JsonDocument.Parse(model.MapData.GeoJsonCoordinates))
            {
                var root = doc.RootElement;

                // FIX 🚨: Lagre ren GeoJSON (slik MapConfirmation forventer)
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

            // ================================
            // 2) Lagre koordinatene
            // ================================
            model.MapData.Coordinates = coords;
            model.MapData.MapZoomLevel = model.MapData.MapZoomLevel != 0
                ? model.MapData.MapZoomLevel
                : 13;

            // ================================
            // 3) Lagre MapData
            // ================================
            _context.MapDatas.Add(model.MapData);
            await _context.SaveChangesAsync();

            // ================================
            // 4) Håndter bildeopplasting
            // ================================
            string? imagePath = null;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);

                imagePath = "/uploads/" + uniqueFileName;
            }

            // ================================
            // 5) Lagre Obstacle
            // ================================
            model.ObstacleImageURL = imagePath;

            _context.Obstacles.Add(model);
            await _context.SaveChangesAsync();

            // ================================
            // 6) Lagre rapport
            // ================================
            var currentUserId = _userManager.GetUserId(User);
            var currentUser = await _userManager.GetUserAsync(User);

            var report = new ObstacleReportData
            {
                ObstacleID = model.ObstacleID,
                SubmittedByUserId = currentUserId, // 🔁 Bruker SubmittedByUserId i stedet for UserId
                ObstacleReportComment = "Her skal Registerfører kunne skrive inn kommentar.",
                ObstacleReportDate = DateTime.UtcNow.AddHours(1),
                ObstacleReportStatus = ObstacleReportData.EnumTypes.New,
                MapDataID = model.MapData?.MapDataID,
                CoordinateSummary = model.MapData?.CoordinateSummary ?? string.Empty
            };


            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

            // ================================
            // 7) Til MapConfirmation
            // ================================
            return View("MapConfirmation", model.MapData);
        }




        // GET: /Map/ReportListOverview
        [HttpGet]
        public async Task<IActionResult> ReportListOverview()
        {
            var reports = await _context.ObstacleReports
            .Include(r => r.Obstacle)
            .Include(r => r.SubmittedByUser)      // 🔁 bruker innsenderen
            .Include(r => r.Reviewer)
            .Include(r => r.MapData)
                .ThenInclude(m => m.Coordinates)
            .ToListAsync();


            return View(reports);
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
                // Latitude = obstacle.Latitude,
                // Longitude = obstacle.Longitude,
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
                
                SubmittedByUserId = null,   // ingen innlogget bruker
                ReviewedByUserID = null,
            };


            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

           return RedirectToAction(nameof(ReportListOverview));
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

        // GET: /Map/MapView
        public async Task<IActionResult> MapView(int? id)
        {
            // Load the MapData to use as base model (if id provided), otherwise create empty
            MapData model;
            if (id.HasValue)
            {
                model = await _context.MapDatas
                    .Include(m => m.Coordinates)
                    .FirstOrDefaultAsync(m => m.MapDataID == id.Value) ?? new MapData();
            }
            else
            {
                model = new MapData();
            }

            // Populate ObstacleReports (load reports that have MapData with geometry)
            var reportsWithGeometry = await _context.ObstacleReports
                .Include(r => r.MapData)
                .Where(r => r.MapData != null && !string.IsNullOrWhiteSpace(r.MapData.GeoJsonCoordinates))
                .ToListAsync();

            // Attach for the view (so Model.ObstacleReports is available to the Razor)
            model.ObstacleReports = reportsWithGeometry;

            return View(model);
        }


        // POST: /Map/Submit
        [HttpPost]
        public IActionResult Submit(MapData mapdata)
        {
            if (!ModelState.IsValid)
                return View("MapView", mapdata);

            return View("MapConfirmation", mapdata);
        }

        // GET: /Map/MapConfirmation
        [HttpGet]
        public IActionResult MapConfirmation()
        {
            return View();
        }
    }
}
