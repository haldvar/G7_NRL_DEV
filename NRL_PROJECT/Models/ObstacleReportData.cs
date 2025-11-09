using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace NRL_PROJECT.Models
{
    public class ObstacleReportData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ObstacleReportID { get; set; }

        //  Hører til et hinder
        [ForeignKey(nameof(Obstacle))]
        public int? ObstacleID { get; set; }
        public ObstacleData? Obstacle { get; set; }

        //  Brukeren som opprettet rapporten
        [ForeignKey(nameof(User))]
        public string? UserID { get; set; }
        public User? User { get; set; }

        [Required]
        public string ObstacleReportComment { get; set; } = string.Empty;

        public DateTime ObstacleReportDate { get; set; }

        public EnumTypes ObstacleReportStatus { get; set; }

        //  Brukeren som har vurdert rapporten (kan være null)
        [ForeignKey(nameof(Reviewer))]
        public string? ReviewedByUserID { get; set; }
        public User? Reviewer { get; set; }

        //  Kobling til MapData (kan være null)
        [ForeignKey(nameof(MapData))]
        public int? MapDataID { get; set; }
        public MapData? MapData { get; set; }

        //  Enum for status på rapporten
        public enum EnumTypes
        {
            New = 0,
            Open = 1,
            InProgress = 2,
            Resolved = 3,
            Closed = 4,
            Deleted = 5
        }


    }
}
