using static NRL_PROJECT.Models.ObstacleReportData;

namespace NRL_PROJECT.Models.ViewModels
{
    public class OrgExternalViewModel
    {
        // Innsender
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? OrgName { get; set; }

        // Rapport
        public int ObstacleReportID { get; set; }
        public DateTime ObstacleReportDate { get; set; }
        public EnumTypes ObstacleReportStatus { get; set; }

        // Hindring
        public int? ObstacleID { get; set; }
        public string? ObstacleType { get; set; }

        // Kart
        public MapData? MapData { get; set; }
        public string? CoordinateSummary { get; set; }
    }
}
