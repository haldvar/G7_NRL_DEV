using EnumStatus = NRL_PROJECT.Models.ObstacleReportData.EnumTypes;

namespace NRL_PROJECT.Models

{
    public class ObstacleReportViewModel
    {
        //innsender
        public int UserId { get; set; }
        public string? UserName { get; set; }

        public int ReportId { get; set; }                 // ID til rapporten
        public DateTime TimeOfSubmittedReport { get; set; } // tidspunkt

        // Kobling til hindring
        public string? ObstacleName { get; set; }
        public string? ObstacleType { get; set; }
        public double? ObstacleHeight { get; set; }
        public double? ObstacleWidth { get; set; }
        public string? ObstacleComment { get; set; }
        public int ObstacleID { get; set; }


        public string? StatusComment { get; set; }      // kommentaren du skriver i ReportDetails


        // Kart / koordinater
        public string? Reported_Location { get; set; }
        public string? GeoJsonCoordinates { get; set; }   
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Statusfelt (valgfritt, for registerførervisning)
        public EnumStatus ReportStatus { get; set; }

        // For enkel sjekk i view
        public bool HasCoordinates => Latitude != 0 && Longitude != 0;
    }
}

