using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;

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
                Longitude1 = 0,
                Latitude1 = 0,
                Longitude2 = 0,
                Latitude2 = 0,
                MapData = new MapData()
            };

            return View(model);
        }

        // POST: /Map/SubmitObstacleWithLocation
        [HttpPost]
        public async Task<IActionResult> SubmitObstacleWithLocation(ObstacleData model)
        {
            if (model.MapData == null)
                model.MapData = new MapData();

            // ✅ Minst ett punkt må være angitt
            if (model.Longitude1 == 0 || model.Latitude1 == 0)
            {
                ModelState.AddModelError("", "Du må minst angi ett punkt på kartet.");
                return View("ObstacleAndMapForm", model);
            }

            // ✅ Bygg GeoJSON dynamisk
            string geoJson;
            if (model.Latitude2 != 0 && model.Longitude2 != 0)
            {
                geoJson = $@"{{ ""type"": ""LineString"", ""coordinates"": [
                    [{model.Longitude1}, {model.Latitude1}],
                    [{model.Longitude2}, {model.Latitude2}]
                ]}}";
            }
            else
            {
                geoJson = $@"{{ ""type"": ""Point"", ""coordinates"": [{model.Longitude1}, {model.Latitude1}] }}";
            }

            // ✅ Lagre MapData
            var mapData = new MapData
            {
                Latitude1 = model.Latitude1,
                Longitude1 = model.Longitude1,
                Latitude2 = model.Latitude2,
                Longitude2 = model.Longitude2,
                MapZoomLevel = model.MapData.MapZoomLevel != 0 ? model.MapData.MapZoomLevel : 13,
                GeoJsonCoordinates = geoJson
            };
            _context.MapDatas.Add(mapData);
            await _context.SaveChangesAsync();

            // ✅ Lagre Obstacle
            var obstacle = new ObstacleData
            {
                ObstacleType = model.ObstacleType,
                ObstacleHeight = model.ObstacleHeight,
                ObstacleWidth = model.ObstacleWidth,
                Latitude1 = model.Latitude1,
                Longitude1 = model.Longitude1,
                Latitude2 = model.Latitude2,
                Longitude2 = model.Longitude2,
                ObstacleComment = model.ObstacleComment,
                MapData = mapData
            };
            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            // ✅ Lag en første rapport
            var report = new ObstacleReportData
            {
                ObstacleID = obstacle.ObstacleId,
                UserID = null,
                ReviewedByUserID = null,
                ObstacleReportComment = "Her skal Registerfører kunne skrive inn kommentar.",
                ObstacleReportDate = DateTime.UtcNow,
                ObstacleReportStatus = ObstacleReportData.EnumTypes.New,
                MapDataID = mapData.MapDataID,
                ObstacleImageURL = ""
            };
            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

            return View("MapConfirmation", mapData);
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
                Latitude1 = obstacle.Latitude1,
                Longitude1 = obstacle.Longitude1,
                Latitude2 = obstacle.Latitude2,
                Longitude2 = obstacle.Longitude2,
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

            var features = obstacles.Select(o =>
            {
                object geometry;
                if (o.Latitude2 != 0 && o.Longitude2 != 0)
                {
                    geometry = new
                    {
                        type = "LineString",
                        coordinates = new[]
                        {
                            new[] { o.Longitude1, o.Latitude1 },
                            new[] { o.Longitude2.Value, o.Latitude2.Value }
                        }
                    };
                }
                else
                {
                    geometry = new
                    {
                        type = "Point",
                        coordinates = new[] { o.Longitude1, o.Latitude1 }
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
                Latitude1 = 60.3913,
                Longitude1 = 5.3221,
                Latitude2 = 61.3913,
                Longitude2 = 4.3221,
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
