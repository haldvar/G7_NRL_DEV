using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace NRL_PROJECT.Models
{
    /// <summary>
    /// Represents a report submitted for an obstacle.
    /// Contains references to the obstacle, submitter, reviewer and optional MapData.
    /// </summary>
    public class ObstacleReportData
    {
        // Primary key
        [Key]
        public int ObstacleReportID { get; set; }

        // Foreign key -> Obstacle (nullable)
        public int? ObstacleID { get; set; }
        public ObstacleData? Obstacle { get; set; }

        // Submitter (nullable)
        public string? SubmittedByUserId { get; set; }
        public User? SubmittedByUser { get; set; }

        // Reviewer (nullable)
        public string? ReviewedByUserID { get; set; }
        public User? Reviewer { get; set; }

        // Main textual comment attached to the report (never null in DB logic)
        public string? ObstacleReportComment { get; set; } = string.Empty;

        // When the report was created
        public DateTime ObstacleReportDate { get; set; }

        // Backend status enum
        public EnumTypes ObstacleReportStatus { get; set; }

        // Link to MapData (nullable)
        public int? MapDataID { get; set; }
        public MapData? MapData { get; set; }

        // Human-readable summary of coordinates (persisted)
        public string CoordinateSummary { get; set; } = string.Empty;

        // Enum describing report status (kept nested for compatibility)
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

