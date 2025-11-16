using static NRL_PROJECT.Models.ObstacleReportData;
using EnumStatus = NRL_PROJECT.Models.ObstacleReportData.EnumTypes;

namespace NRL_PROJECT.Models

{
    public class ObstacleReportViewModel
    {
        //innsender
        // public string? UserID { get; set; }
        public string? UserName { get; set; }
        public string? OrgName { get; set; } = string.Empty;
        public int ObstacleReportID { get; set; }                 
        public DateTime TimeOfSubmittedReport { get; set; } 

        // Kobling til hindring
        public string? ObstacleName { get; set; }
        public string? ObstacleType { get; set; }
        public double? ObstacleHeight { get; set; }
        public double? ObstacleWidth { get; set; }
        public string ObstacleComment { get; set; } = "";
        public int ObstacleID { get; set; }

        // Kobling til rapporten
        public string ReportedByUserName { get; set; } = string.Empty;
        public DateTime ObstacleReportDate { get; set; }
        public EnumTypes ObstacleReportStatus { get; set; }
        public User SubmittedByUser { get; set; }

        public string? StatusComment { get; set; }      // kommentaren du skriver i ReportDetails


        // Kart / koordinater
        public MapData? MapData { get; set; }
        public string? Reported_Location { get; set; }
        public string? GeoJsonCoordinates { get; set; }   
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string CoordinateSummary { get; set; }

        // Statusfelt (valgfritt, for registerførervisning)
        public EnumStatus? ReportStatus { get; set; }

        // For enkel sjekk i view
        public bool HasCoordinates => Latitude != 0 && Longitude != 0;

        // Registrar 
        public string? AssignedRegistrarUserID { get; set; }
        public string? TransferToUserID { get; set; }
    }
}

