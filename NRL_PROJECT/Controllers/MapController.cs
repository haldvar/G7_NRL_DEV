using Microsoft.AspNetCore.Mvc;
using NRL_PROJECT.Models;

namespace NRL_PROJECT.Controllers
{
    public class MapController : Controller
    {

        // GET: /Map/MapView
        // Viser kartet med sentrerte koordinater og zoomnivå


        [HttpGet]
        public IActionResult MapView()
        {
            var defaultMapData = new MapData
            {
                CenterLatitude = 60.3913,   // Standard sentrering (Bergen)
                CenterLongitude = 5.3221,
                MapZoomLevel = 12               // Standard zoomnivå
            };

            return View(defaultMapData);
            
        }

        // POST: /Map/Submit
        // Mottar kartdata (senter og zoom) og viser bekreftelse

        [HttpPost]
        public IActionResult Submit(MapData mapdata)
        {
            if (!ModelState.IsValid)
            {
                return View("MapView", mapdata);
            }

            // Lagre mapdata til database i mapconfirmation view
            
           return View("MapConfirmation", mapdata);
        }

        [HttpGet]
        public IActionResult MapConfirmation()
        {
            return View();
        }

    }
}
