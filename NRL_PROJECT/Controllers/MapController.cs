using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace NRL_PROJECT.Controllers
{
    public class MapController : Controller
    {
        private readonly NRL_Db_Context _context;
        private readonly IWebHostEnvironment _environment;

        public MapController(NRL_Db_Context context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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

            return View(new ObstacleData());
        }

        // POST: /Map/SubmitObstacleWithLocation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitObstacleWithLocation(ObstacleData model, ObstacleReportData model2)
        {
            if (model.MapData == null)
                model.MapData = new MapData();

            if (string.IsNullOrWhiteSpace(model.MapData.GeoJsonCoordinates))
            {
                ModelState.AddModelError("", "Du må minst angi ett punkt på kartet.");
                return View("ObstacleAndMapForm", model);
            }

            // ✅ Parse GeoJSON og bygg koordinater
            var coords = new List<MapCoordinate>();
            using (var doc = JsonDocument.Parse(model.MapData.GeoJsonCoordinates))
            {
                var root = doc.RootElement;
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
            }

            model.MapData.Coordinates = coords;
            model.MapData.MapZoomLevel = model.MapData.MapZoomLevel != 0 ? model.MapData.MapZoomLevel : 13;

            // ✅ Lagre MapData
            _context.MapDatas.Add(model.MapData);
            await _context.SaveChangesAsync();
            
            //  Håndter bildeopplasting
            string? imagePath = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                imagePath = "/uploads/" + uniqueFileName;
            }

            

            // ✅ Lagre Obstacle
            var obstacle = new ObstacleData
            {
                ObstacleType = model.ObstacleType,
                ObstacleHeight = model.ObstacleHeight,
                ObstacleWidth = model.ObstacleWidth,
                ObstacleComment = model.ObstacleComment,
                ObstacleImageURL = imagePath,                 // ✅ ASSIGN THE URL
                MapData = model.MapData
            };
            if (obstacle.ObstacleComment == null)
            {
                obstacle.ObstacleComment = "Empty";
            }
            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            

            //  Opprett rapport-oppføring
            var report = new ObstacleReportData
            {
                ObstacleID = obstacle.ObstacleId,
                UserID = null, // ingen bruker koblet enda
                ReviewedByUserID = null,
                ObstacleReportComment = "Her skal Registerfører kunne skrive inn kommentar.",
                ObstacleReportDate = DateTime.UtcNow,
                ObstacleReportStatus = ObstacleReportData.EnumTypes.New,
                MapDataID = obstacle.MapData.MapDataID,
            };

            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

            return View("MapConfirmation", model.MapData);
        }


        // GET: /Map/ReportListOverview
        [HttpGet]
        public async Task<IActionResult> ReportListOverview()
        {
            var reports = await _context.ObstacleReports
                .Include(r => r.Obstacle)
                .Include(r => r.User)
                .Include(r => r.Reviewer)
                .Include(r => r.MapData)
                    .ThenInclude(m => m.Coordinates)
                .ToListAsync();

            return View(reports);
        }

        // POST: /Map/SubmitObstacleReport
        [HttpPost]
        public async Task<IActionResult> SubmitObstacleReport(int obstacleId, string geoJson)
        {
            var obstacle = await _context.Obstacles.FindAsync(obstacleId);
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
                ObstacleID = obstacle.ObstacleId,
                ObstacleReportComment = "New report submitted.",
                ObstacleReportDate = DateTime.UtcNow,
                ObstacleReportStatus = ObstacleReportData.EnumTypes.New,
                MapDataID = mapData.MapDataID,
                UserID = null,
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
                        id = o.ObstacleId,
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
        [HttpGet]
        public IActionResult MapView()
        {
            var defaultMapData = new MapData
            {
                GeometryType = "Point",
                MapZoomLevel = 12,
                Coordinates = new List<MapCoordinate>
                {
                    new MapCoordinate { Latitude = 60.3913, Longitude = 5.3221, OrderIndex = 0 }
                }
            };

            return View(defaultMapData);
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
