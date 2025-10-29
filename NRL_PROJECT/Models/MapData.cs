using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRL_PROJECT.Models
{
    public class MapData
    {
        [Key]
        public int MapDataID { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int MapZoomLevel { get; set; }

        public string GeoJsonCoordinates { get; set; } = string.Empty;

        public ICollection<ObstacleReportData> ObstacleReports { get; set; } = new List<ObstacleReportData>();
    }


}
