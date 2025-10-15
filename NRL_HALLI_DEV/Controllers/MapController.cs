using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace NRL_HALLI_DEV.Controllers
{
    public class MapController : Controller
    {
        [HttpGet]
        public IActionResult Map() => View();

        // POST from the form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterObstacles(string markersJson)
        {
            // Basic guard
            if (string.IsNullOrWhiteSpace(markersJson))
            {
                TempData["MarkersJson"] = "[]";
            }
            else
            {
                TempData["MarkersJson"] = markersJson;
            }

            // Redirect to a neat display page
            return RedirectToAction(nameof(Review));
        }

        [HttpGet]
        public IActionResult Review()
        {
            var json = TempData["MarkersJson"] as string ?? "[]";
            var markers = JsonSerializer.Deserialize<List<MarkerDto>>(json) ?? new List<MarkerDto>();
            return View(markers);
        }
    }

    public class MarkerDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime? CreatedUtc { get; set; }
    }
}