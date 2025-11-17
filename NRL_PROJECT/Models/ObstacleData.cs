using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRL_PROJECT.Models
{
    public class ObstacleData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ObstacleID { get; set; }
        // public string? ObstacleDescription { get; set; }

        [Required]
        public string ObstacleType { get; set; } = "Ukjent";

        public double ObstacleHeight { get; set; }
        public double ObstacleWidth { get; set; }
        public string? ObstacleComment { get; set; }
        
        //  URL / filsti til bilde lagret i wwwroot/uploads
        [StringLength(255)]
        public string? ObstacleImageURL { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

       
        public MapData MapData { get; set; } = new MapData
        {
            GeoJsonCoordinates = string.Empty
        };
       
        
        // Opplasting: brukes kun under innsending (ikke lagres i databasen)
        [NotMapped]
        public IFormFile? ImageFile { get; set; }  // mottar fil fra skjema

        // Lokal referanse etter lagring
        [NotMapped]
        public string? ImagePath { get; set; }     // intern bruk før lagring av URL
       

        public ICollection<ObstacleReportData> ObstacleReports { get; set; } = new List<ObstacleReportData>();
    }
}
