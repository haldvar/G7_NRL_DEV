using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NRL_PROJECT.Models.ViewModels
{
    /// <summary>
    /// View model for the Registrar overview page.
    /// - Holds selected filters, available options and the result list.
    /// - Preserves existing defaults and semantics.
    /// </summary>
    public class RegistrarOverviewViewModel
    {
        // -----------------------------
        // Selected filter values
        // -----------------------------

        public string SelectedStatus { get; set; } = "Alle";
        
        public string? SelectedObstacleType { get; set; } = "";

        // -----------------------------
        // Dropdown options / filters
        // -----------------------------        
        public IEnumerable<SelectListItem> StatusOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem("Alle", "Alle"),
            new SelectListItem("Ny", "Ny"),
            new SelectListItem("Under behandling", "Under behandling"),
            new SelectListItem("Godkjent", "Godkjent"),
            new SelectListItem("Avvist", "Avvist"),
        };

        /// <summary>Obstacle-type options (populated by controller).</summary>
        public IEnumerable<SelectListItem> ObstacleTypeOptions { get; set; } = new List<SelectListItem>();

        // -----------------------------
        // Results
        // -----------------------------

        /// <summary>List of reports to render in the overview table.</summary>
        public List<RegistrarListItemViewModel> Reports { get; set; } = new();

        // -----------------------------
        // Counters (status summaries)
        // -----------------------------

        public int CountNy { get; set; }
        public int CountUnderBehandling { get; set; }
        public int CountGodkjent { get; set; }
        public int CountAvvist { get; set; }
        public int CountAlle { get; set; }
    }

    

    /// <summary>
    /// Lightweight model representing a single row in the registrar overview list.
    /// </summary>
    public class RegistrarListItemViewModel
    {
        /// <summary>Report identifier.</summary>
        public int Id { get; set; }

        /// <summary>Title or short description for display in the table.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>List of latitude/longitude pairs representing the reported location(s).</summary>
        public List<LatLng> Reported_Location { get; set; } = new();

        /// <summary>Obstacle type (used for filtering and display).</summary>
        public string ObstacleType { get; set; } = string.Empty;

        /// <summary>Human-readable status string for the row.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Organisation name of the submitter (optional).</summary>
        public string? Organisation { get; set; }

        /// <summary>Latest comment text (optional).</summary>
        public string? LatestComment { get; set; }

        /// <summary>Count of comments for the report.</summary>
        public int CommentCount { get; set; }

        /// <summary>Simple container for a single lat/lng pair.</summary>
        public class LatLng
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
    }
    
}
