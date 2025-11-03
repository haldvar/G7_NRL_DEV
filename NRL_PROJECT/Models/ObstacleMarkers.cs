using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRL_PROJECT.Models
{
    public class ObstacleMarkerData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ObstacleMarkerID { get; set; }

        [ForeignKey(nameof(Obstacle))]
        public int ObstacleID { get; set; }
        public ObstacleData? Obstacle { get; set; }

        [Range(1, 2)]
        public byte MarkerNo { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        [Range(-180, 180)]
        public decimal Longitude { get; set; }
    }
}
