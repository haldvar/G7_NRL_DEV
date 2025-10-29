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

        public MapController(NRL_Db_Context context)
        {
            _context = context;
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

            return View(model);
        }

        // POST: /Map/SubmitObstacleWithLocation
        [HttpPost]
        public async Task<IActionResult> SubmitObstacleWithLocation(ObstacleData model)
        {
            // 1️⃣ Sjekk kartdata først
            if (model.MapData == null)
                model.MapData = new MapData();

            if (model.Longitude == 0 || model.Latitude == 0 || string.IsNullOrWhiteSpace(model.MapData.GeoJsonCoordinates))
            {
                ModelState.AddModelError("", "Location must be selected and drawn on the map.");
                return View("ObstacleAndMapForm", model);
            }

            // 2️⃣ Lagre kartdata først
            var mapData = new MapData
            {
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                MapZoomLevel = model.MapData.MapZoomLevel != 0 ? model.MapData.MapZoomLevel : 13,
                GeoJsonCoordinates = model.MapData.GeoJsonCoordinates
            };
            _context.MapDatas.Add(mapData);
            await _context.SaveChangesAsync();

            // 3️⃣ Lagre hinder (Obstacle)
            var obstacle = new ObstacleData
            {
                ObstacleType = model.ObstacleType,
                ObstacleHeight = model.ObstacleHeight,
                ObstacleWidth = model.ObstacleWidth,
                Longitude = model.Longitude,
                Latitude = model.Latitude,
                ObstacleComment = model.ObstacleComment,
                MapData = mapData
            };
            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            // 4️⃣ Opprett en tilknyttet rapport
            var report = new ObstacleReportData
            {
                ObstacleID = obstacle.ObstacleId,
                UserID = null, // ingen bruker koblet enda
                ReviewedByUserID = null,
                ObstacleReportComment = "Her skal Registerfører kunne skrive inn kommentar.",
                ObstacleReportDate = DateTime.UtcNow,
                ObstacleReportStatus = ObstacleReportData.EnumTypes.New,
                MapDataID = mapData.MapDataID,
                ObstacleImageURL = "" // kan være tom
            };
            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

            // 5️⃣ Ferdig! Send bruker til oversikt
            return RedirectToAction(nameof(ReportListOverview));
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
