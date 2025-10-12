using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace NRL_PROJECT.Models
{
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
    }
}
