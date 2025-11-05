using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRL_PROJECT.Models
{
    public class MapData
    {
        [Key]
        public int MapDataID { get; set; }

        public string GeometryType { get; set; } = "Point"; // "Point" eller "LineString"
        public int MapZoomLevel { get; set; }

        public string GeoJsonCoordinates { get; set; } = string.Empty;

        public ICollection<MapCoordinate> Coordinates { get; set; } = new List<MapCoordinate>();


        // Relasjon til rapporter
        public ICollection<ObstacleReportData> ObstacleReports { get; set; } = new List<ObstacleReportData>();
    }
}
