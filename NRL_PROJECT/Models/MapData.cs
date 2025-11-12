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


        // For tabellvisning
        [NotMapped]
        public string CoordinateSummary
        {
            get
            {
                if (Coordinates == null || !Coordinates.Any())
                    return "No coordinates";

                if (GeometryType.Equals("Point", StringComparison.OrdinalIgnoreCase))
                {
                    var c = Coordinates.First();
                    return $"Punkt ({c.Latitude:F5}, {c.Longitude:F5})";
                }

                if (GeometryType.Equals("LineString", StringComparison.OrdinalIgnoreCase))
                {
                    var start = Coordinates.First();
                    var end = Coordinates.Last();
                    return $"Linje med {Coordinates.Count} punkter fra " +
                           $"({start.Latitude:F5}, {start.Longitude:F5}) til " +
                           $"({end.Latitude:F5}, {end.Longitude:F5})";
                }

                return string.Join(", ", Coordinates.Select(c => $"({c.Latitude:F5}, {c.Longitude:F5})"));
            }
        }
    }
}
