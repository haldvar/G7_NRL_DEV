using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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
        public string ObstacleType { get; set; } = "Ukjent";

        public double ObstacleHeight { get; set; }
        public double ObstacleWidth { get; set; }
        public string ObstacleComment { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        /*

         // Ny "computed property" for GeoJSON
         [NotMapped]
         public string GeoJsonLine =>
             Latitude2.HasValue && Longitude2.HasValue
                 ? $"{{ \"type\": \"LineString\", \"coordinates\": [[{Longitude1}, {Latitude1}], [{Longitude2}, {Latitude2}]] }}"
                 : $"{{ \"type\": \"Point\", \"coordinates\": [{Longitude1}, {Latitude1}] }}";

         */

        public MapData MapData { get; set; } = new MapData
    {
        GeoJsonCoordinates = string.Empty
    };
       

        public ICollection<ObstacleReportData> ObstacleReports { get; set; } = new List<ObstacleReportData>();
    }
}
