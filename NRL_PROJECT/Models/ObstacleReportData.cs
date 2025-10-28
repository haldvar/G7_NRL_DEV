using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace NRL_PROJECT.Models
{
    public class ObstacleReportData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ObstacleReportID { get; set; }  // Autogenerert primærnøkkel for rapporter
        
        [ForeignKey("User")]
        public int UserID { get; set; }
                
        public int ReviewedByUserID { get; set; }
        

        [ForeignKey("ObstacleID")]
        public int ObstacleID { get; set; }

        public string ObstacleReportComment { get; set; }

        public DateTime ObstacleReportDate { get; set; }

        public EnumTypes ObstacleReportStatus { get; set; } 

        [StringLength(255)]
        public string ObstacleImageURL { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }

        [ForeignKey("ReviewedByUserID")]
        public User Reviewer { get; set; }

        [ForeignKey("ObstacleID")]
        public ObstacleData Obstacle { get; set; }
                
        [ForeignKey("MapDataID")]
        public MapData MapData { get; set; }
        public int MapDataID { get; set; }


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
