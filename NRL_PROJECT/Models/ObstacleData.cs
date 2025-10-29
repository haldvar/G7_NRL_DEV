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
        public string ObstacleName { get; set; }

        public string ObstacleType { get; set; }
        public int ObstacleHeight { get; set; }
        public int ObstacleWidth { get; set; }
        public string ObstacleDescription { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        // Relasjon til rapporter
        public ICollection<ObstacleReportData> Reports { get; set; }
    }
}