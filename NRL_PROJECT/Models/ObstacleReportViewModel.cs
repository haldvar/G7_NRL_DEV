namespace NRL_PROJECT.Models
{
    public class ObstacleReportViewModel
    {
        public int ReportId { get; set; }                 // ID til rapporten
        public DateTime TimeOfSubmittedReport { get; set; } // tidspunkt

        // Kobling til hindring
        public string? ObstacleName { get; set; }
        public string? ObstacleType { get; set; }
        public int ObstacleHeight { get; set; }
        public int ObstacleWidth { get; set; }
        public string? ObstacleDescription { get; set; }
        public int ObstacleID { get; set; }

        // Kart / koordinater
        public string? GeoJsonCoordinates { get; set; }   
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Statusfelt (valgfritt, for registerførervisning)
        public ReportStatus ReportStatus { get; set; } = ReportStatus.Ny;

        // For enkel sjekk i view
        public bool HasCoordinates => Latitude != 0 && Longitude != 0;
    }
}

