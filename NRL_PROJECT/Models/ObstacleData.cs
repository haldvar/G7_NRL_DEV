using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRL_PROJECT.Models
{
    public class ObstacleData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ObstacleId { get; set; }

        [Required]
        public string ObstacleType { get; set; }
        public float ObstacleHeight { get; set; }
        public float ObstacleWidth { get; set; }
        public decimal Coordinates1 { get; set; }
        public decimal Coordinates2 { get; set; }

        public string ObstacleComment { get; set; }

        // Relasjon til rapporter
        public ICollection<ObstacleReportData> ObstacleReports { get; set; }




        public int ObstacleDataID { get; set; }

    }
}
