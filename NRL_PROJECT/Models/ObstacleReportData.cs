using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRL_PROJECT.Models
{
    public enum ReportStatus { Ny = 0, UnderBehandling = 1, Godkjent = 2, Avvist = 3 }
    public class ObstacleReportData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Report_Id { get; set; }  // Autogenerert primærnøkkel for rapporter
        public string Reported_Item { get; set; }   // Fetch ObstacleType from ObstacleOverview.cshtml
        public string Reported_Location { get; set; } // Fetch "lat,lng" from MapConfirmation.cshtml
        public DateTime Time_of_Submitted_Report { get; set; }
        public int ObstacleId { get; set; }
        [ForeignKey("ObstacleId")]
        public ObstacleData Obstacle { get; set; }

        public ReportStatus ReportStatus { get; set; } = ReportStatus.Ny;

        public string? StatusComment { get; set; }
        public DateTime? StatusChangedAt { get; set; }
        public string? HandledBy { get; set; }
        public int ObstacleReportID { get; set; }

        // 🔹 Hører til et hinder
        [ForeignKey(nameof(Obstacle))]
        public int? ObstacleID { get; set; }
        public ObstacleData? Obstacle { get; set; }

        // 🔹 Brukeren som opprettet rapporten
        [ForeignKey(nameof(User))]
        public int? UserID { get; set; }
        public User? User { get; set; }

        [Required]
        public string ObstacleReportComment { get; set; } = string.Empty;

        public DateTime ObstacleReportDate { get; set; }

        public EnumTypes ObstacleReportStatus { get; set; }

        // 🔹 Brukeren som har vurdert rapporten (kan være null)
        [ForeignKey(nameof(Reviewer))]
        public int? ReviewedByUserID { get; set; }
        public User? Reviewer { get; set; }

        [StringLength(255)]
        public string? ObstacleImageURL { get; set; }

        // 🔹 Kobling til MapData (kan være null)
        [ForeignKey(nameof(MapData))]
        public int? MapDataID { get; set; }
        public MapData? MapData { get; set; }

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
