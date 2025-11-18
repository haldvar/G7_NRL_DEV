using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace NRL_PROJECT.Models
{
    public class ObstacleReportData
    {
        [Key]
        public int ObstacleReportID { get; set; }

        // Hindring
        public int? ObstacleID { get; set; }
        public ObstacleData? Obstacle { get; set; }

        // Innsender (Pilot / ExternalOrg / hvem som helst)
        public string? SubmittedByUserId { get; set; }
        public User? SubmittedByUser { get; set; }

        // Registerfører (kan være null)
        public string? ReviewedByUserID { get; set; }
        public User? Reviewer { get; set; }

        // Map
        public int? MapDataID { get; set; }
        public MapData? MapData { get; set; }

        // Kommentarer
        public string ObstacleReportComment { get; set; } = string.Empty;

        // Metadata
        public DateTime ObstacleReportDate { get; set; }
        public EnumTypes ObstacleReportStatus { get; set; }

        public string CoordinateSummary { get; set; } = string.Empty;

        public enum EnumTypes
        {
            New,
            Open,
            InProgress,
            Resolved,
            Closed,
            Deleted
        }
    }
}

