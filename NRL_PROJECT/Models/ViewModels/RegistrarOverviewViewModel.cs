using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NRL_PROJECT.Models.ViewModels
{
    public class RegistrarOverviewViewModel
    {
        // Allerede: valgt status
        public string SelectedStatus { get; set; } = "Alle";

        // NYTT: valgt hindertype
        public string? SelectedObstacleType { get; set; } = "";

        // Allerede: status-alternativer
        public IEnumerable<SelectListItem> StatusOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem("Alle", "Alle"),
            new SelectListItem("Ny", "Ny"),
            new SelectListItem("Under behandling", "Under behandling"),
            new SelectListItem("Godkjent", "Godkjent"),
            new SelectListItem("Avvist", "Avvist"),
        };

        // hindertype-alternativer
        public IEnumerable<SelectListItem> ObstacleTypeOptions { get; set; } = new List<SelectListItem>();

        // Resultatliste
        public List<RegistrarListItemViewModel> Reports { get; set; } = new();

        // Tellerne dine uendret ...
        public int CountNy { get; set; }
        public int CountUnderBehandling { get; set; }
        public int CountGodkjent { get; set; }
        public int CountAvvist { get; set; }
        public int CountAlle { get; set; }
    }

    public class RegistrarListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public List<LatLng> Reported_Location { get; set; } = new();
        public class LatLng
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        // for å vise i tabellen og kunne filtrere
        public string ObstacleType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        // Valgfritt men ofte nyttig
        public string? Organisation { get; set; }

        public string? LatestComment { get; set; }
        public int CommentCount { get; set; }
    }
}
