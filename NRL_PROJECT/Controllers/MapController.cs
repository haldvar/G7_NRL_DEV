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
        /// <summary>
        /// Processes GeoJSON coordinates submitted from the map and displays the appropriate view.
        /// </summary>
        /// <remarks>If the <see cref="MapData.GeoJsonCoordinates"/> property is null, empty, or consists
        /// only of whitespace, an error is added to the model state, and the "MapView" view is returned with the
        /// submitted data. Otherwise, the coordinates are passed to the "MapConfirmation" view via the <see
        /// cref="ViewBag"/>.</remarks>
        /// <param name="mapdata">An object containing the submitted GeoJSON coordinates.</param>
        /// <returns>A view displaying a confirmation page if the coordinates are valid; otherwise, a view displaying an error
        /// message.</returns>
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
    
}
}
