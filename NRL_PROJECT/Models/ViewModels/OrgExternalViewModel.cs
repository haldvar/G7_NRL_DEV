using static NRL_PROJECT.Models.ObstacleReportData;
using NRL_PROJECT.Models;

namespace NRL_PROJECT.Models.ViewModels
{
    /// <summary>
    /// Lightweight view model presented to external organisations.
    /// Contains only the fields needed by organisation-facing listing views.
    /// </summary>
    public class OrgExternalViewModel
    {
        // -----------------------------
        // Reporter / organisation
        // -----------------------------
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? OrgName { get; set; }

        // -----------------------------
        // Report summary
        // -----------------------------
        public int ObstacleReportID { get; set; }
        public DateTime ObstacleReportDate { get; set; }
        public EnumTypes ObstacleReportStatus { get; set; }

        // -----------------------------
        // Obstacle summary
        // -----------------------------
        public int? ObstacleID { get; set; }
        public string? ObstacleType { get; set; }

        // ⭐ Image support (optional)
        public string? ObstacleImageURL { get; set; }

        // -----------------------------
        // Map presentation
        // -----------------------------
        /// MapData can be included when needed; otherwise the summary is sufficient.
        public MapData? MapData { get; set; }

        /// Human-friendly coordinate summary (Point or Line summary).
        public string? CoordinateSummary { get; set; }
    }
}
