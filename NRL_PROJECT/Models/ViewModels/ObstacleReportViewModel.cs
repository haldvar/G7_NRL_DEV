using static NRL_PROJECT.Models.ObstacleReportData;
using EnumStatus = NRL_PROJECT.Models.ObstacleReportData.EnumTypes;
using NRL_PROJECT.Models;

namespace NRL_PROJECT.Models.ViewModels
{
    /// <summary>
    /// View model used to display and edit obstacle reports in the UI.
    /// Contains reporter info, obstacle details, map data and registrar-related fields.
    /// This class is a DTO only and does not affect persistence directly.
    /// </summary>
    public class ObstacleReportViewModel
    {
        // -----------------------------
        // Identity / metadata
        // -----------------------------
       
        public int ObstacleReportID { get; set; }

        public DateTime ObstacleReportDate { get; set; }


        // -----------------------------
        // Reporter / organisation
        // -----------------------------
        
        public string? UserName { get; set; }
                
        public string? OrgName { get; set; } = string.Empty;
              
        public User SubmittedByUser { get; set; }

        // -----------------------------
        // Obstacle (target) details
        // -----------------------------
      
        public int ObstacleID { get; set; }        
             
        public string? ObstacleType { get; set; }
        
        public double? ObstacleHeight { get; set; }
          
        public string ObstacleComment { get; set; } = "";
        
        public string? ObstacleImageURL { get; set; }

        // -----------------------------
        // Report-specific fields
        // -----------------------------       
        public string ObstacleReportComment { get; set; } = string.Empty;   
               
        public EnumTypes ObstacleReportStatus { get; set; }
       
        public EnumStatus? ReportStatus { get; set; }

        // -----------------------------
        // Map / coordinate information
        // -----------------------------

        /// MapData object attached to this report (may be null).
        public MapData? MapData { get; set; }

        /// Human friendly summary of coordinates (computed or stored).
        public string CoordinateSummary { get; set; }

        /// GeoJSON string with coordinates (if available).
        public string? GeoJsonCoordinates { get; set; }

        /// Latitude for quick checks and simple views (first coord).
        public double Latitude { get; set; }

        /// Longitude for quick checks and simple views (first coord).
        public double Longitude { get; set; }

        /// User-facing short representation of reported location (lat,lng).
        public string? Reported_Location { get; set; }

        /// True when a non-zero lat/lng exists (used by views to show map controls).
        public bool HasCoordinates => Latitude != 0 && Longitude != 0;

        // -----------------------------
        // Registrar / workflow fields
        // -----------------------------
        public string? AssignedRegistrarUserID { get; set; }
        
        public string? TransferToUserID { get; set; }
       
        public string? ReviewerName { get; set; }
    }
}

