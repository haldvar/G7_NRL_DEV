using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRL_PROJECT.Models
{
    /// <summary>
    /// Holds geometry and display information for map features.
    /// Stores GeoJSON text and related MapCoordinate list.
    /// </summary>
    public class MapData
    {
        // Primary key
        [Key]
        public int MapDataID { get; set; }

        // Geometry type ("Point" or "LineString")
        public string GeometryType { get; set; } = "Point";

        // Zoom level used when rendering the map
        public int MapZoomLevel { get; set; }

        // GeoJSON geometry stored as text (server expects this format)
        public string GeoJsonCoordinates { get; set; } = string.Empty;

        // Coordinates related to this MapData (ordered by OrderIndex)
        public ICollection<MapCoordinate> Coordinates { get; set; } = new List<MapCoordinate>();

        // Back-reference: reports attached to this MapData
        public ICollection<ObstacleReportData> ObstacleReports { get; set; } = new List<ObstacleReportData>();

        // Computed helper for friendly display in UIs; not persisted
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
