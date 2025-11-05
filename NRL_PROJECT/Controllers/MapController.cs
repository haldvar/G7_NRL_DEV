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
                Longitude = 0,
                Latitude = 0,
                MapData = new MapData()
            };

            return View(new ObstacleReportData());
        }

        // POST: /Map/SubmitObstacleWithLocation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitObstacleWithLocation(ObstacleReportData model)
        {
            //  Validering av input
            if (string.IsNullOrWhiteSpace(model.ObstacleReportComment))
            {
                ModelState.AddModelError("ObstacleReportComment", "Du må skrive en kommentar.");
            }

            if (model.MapData == null || string.IsNullOrWhiteSpace(model.MapData.GeoJsonCoordinates))
            {
                ModelState.AddModelError("MapData", "Du må plassere et punkt på kartet.");
            }

            if (!ModelState.IsValid)
            {
                return View("ObstacleAndMapForm", model);
            }

            //  Opprett og lagre hinder (ObstacleData)
            var obstacle = new ObstacleData
            {
                ObstacleType = "Ukjent", // evt. hent fra ViewModel
                ObstacleHeight = 0,
                ObstacleWidth = 0,
                Latitude = model.MapData?.Latitude ?? 0,
                Longitude = model.MapData?.Longitude ?? 0,
                ObstacleComment = model.ObstacleReportComment,
                MapData = model.MapData ?? new MapData { GeoJsonCoordinates = "" }
            };

            _context.Obstacles.Add(obstacle);
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

            //  Opprett rapport-oppføring
            var report = new ObstacleReportData
            {
                ObstacleID = obstacle.ObstacleId,
                UserID = null, // kan fylles inn senere hvis dere har innlogging
                ReviewedByUserID = null,
                ObstacleReportComment = "Her skal Registerfører kunne skrive inn kommentar.",
                ObstacleReportDate = DateTime.UtcNow,
                ObstacleReportStatus = ObstacleReportData.EnumTypes.New,
                MapDataID = obstacle.MapData.MapDataID,
                ObstacleImageURL = imagePath
            };

            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

            //  Ferdig
            TempData["Success"] = "Rapporten ble sendt inn!";
            return RedirectToAction("ReportListOverview", "Map");
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
                Latitude = obstacle.Latitude,
                Longitude = obstacle.Longitude,
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
                ObstacleImageURL = ""
            };


            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

           return RedirectToAction(nameof(ReportListOverview));
        }

        // GET: /Map/GetObstacles
        [HttpGet]
        public async Task<IActionResult> GetObstacles()
        {
            var obstacles = await _context.Obstacles.ToListAsync();

            var geojson = new
            {
                type = "FeatureCollection",
                features = obstacles.Select(o => new
                {
                    type = "Feature",
                    geometry = new
                    {
                        type = "Point",
                        coordinates = new[] { o.Longitude, o.Latitude }
                    },
                    properties = new
                    {
                        id = o.ObstacleId,
                        type = o.ObstacleType,
                        height = o.ObstacleHeight,
                        width = o.ObstacleWidth,
                        comment = o.ObstacleComment
                    }
                })
            };

            return Json(geojson);
        }

        // GET: /Map/MapView
        [HttpGet]
        public IActionResult MapView()
        {
            var defaultMapData = new MapData
            {
                Latitude = 60.3913,
                Longitude = 5.3221,
                MapZoomLevel = 12
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
