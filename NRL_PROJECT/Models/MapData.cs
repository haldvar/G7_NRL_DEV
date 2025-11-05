using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRL_PROJECT.Models
{
    public class MapData
    {
        [Key]
        public int MapDataID { get; set; }

        // Første punkt (alltid påkrevd)
        public double Latitude1 { get; set; }
        public double Longitude1 { get; set; }

        // Andre punkt (valgfritt – brukes for linje)
        public double? Latitude2 { get; set; }
        public double? Longitude2 { get; set; }

        // Zoomnivå
        public int MapZoomLevel { get; set; }

        // Lagret GeoJSON (kan være Point eller LineString)
        public string GeoJsonCoordinates { get; set; } = string.Empty;

        // Dynamisk GeoJSON (ikke lagres i databasen)
        [NotMapped]
        public string GeoJsonGeometry
        {
            get
            {
                if (Latitude2.HasValue && Longitude2.HasValue)
                {
                    return $@"{{ ""type"": ""LineString"", ""coordinates"": [
                        [{Longitude1}, {Latitude1}],
                        [{Longitude2}, {Latitude2}]
                    ]}}";
                }
                else
                {
                    return $@"{{ ""type"": ""Point"", ""coordinates"": [{Longitude1}, {Latitude1}] }}";
                }
            }
        }

        // Relasjon til rapporter
        public ICollection<ObstacleReportData> ObstacleReports { get; set; } = new List<ObstacleReportData>();
    }
}
