using Microsoft.AspNetCore.Mvc;
using NRL_PROJECT.Models;

namespace NRL_PROJECT.Controllers
{
    public class MapController : Controller
    {

        // GET: /Map/MapView
        // Displays the Leaflet map with drawing tools
        [HttpGet]
        public IActionResult MapView()
        {
            return View();
        }

        // POST: /Map/Submit
        // Receives GeoJSON coordinates from the map and displays them
        [HttpPost]
        public IActionResult Submit(MapData mapdata)
        {
            if (string.IsNullOrWhiteSpace(mapdata.GeoJsonCoordinates))
            {
                ModelState.AddModelError("GeoJsonCoordinates", "No coordinates submitted.");
                return View("MapView", mapdata);
            }

            // Pass coordinates to mapconfirmation view
            ViewBag.Coordinates = mapdata.GeoJsonCoordinates;
            return View("MapConfirmation", mapdata);
        }

        [HttpGet]
        public IActionResult MapConfirmation()
        {
            return View();
        }

    }
}
