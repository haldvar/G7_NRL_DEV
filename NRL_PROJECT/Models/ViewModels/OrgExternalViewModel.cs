using static NRL_PROJECT.Models.ObstacleReportData;

namespace NRL_PROJECT.Models.ViewModels
    {
        public class OrgExternalViewModel
        {
        // Kobling til innlogget bruker    
        public string? UserID { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? OrgName { get; set; }
        public string? CurrentRole { get; set; }
        public List<string> AvailableRoles { get; set; }

        // Kobling til hindring
        public string? ObstacleName { get; set; }
        public string? ObstacleType { get; set; }
        public double? ObstacleHeight { get; set; }
        public double? ObstacleWidth { get; set; }
        public string? ObstacleComment { get; set; } = "";
        public int? ObstacleID { get; set; }



        // Kobling til rapporten
        // public string ReportedByUserName { get; set; } = string.Empty; // - overflødig?
        public DateTime? ObstacleReportDate { get; set; }
        public EnumTypes? ObstacleReportStatus { get; set; }
        public User? SubmittedByUser { get; set; }
        public int? ObstacleReportID { get; set; }
        public string? ObstacleReportComment { get; set; } = string.Empty;
        public string? ReviewedByUserID { get; set; }


        // Kart / koordinater
        public MapData? MapData { get; set; }
        public string? CoordinateSummary { get; set; }
        public string? ObstacleImageURL { get; set; }

    }
}


