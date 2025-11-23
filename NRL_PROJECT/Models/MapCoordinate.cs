using System.ComponentModel.DataAnnotations;

namespace NRL_PROJECT.Models
{
    /// <summary>
    /// Represents a single coordinate (latitude/longitude) belonging to MapData.
    /// OrderIndex is used to reconstruct LineStrings.
    /// </summary>
    public class MapCoordinate
    {
        [Key]
        public int CoordinateId { get; set; }

        // Foreign key to MapData
        public int MapDataID { get; set; }
        public MapData? MapData { get; set; }

        // Coordinate values
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Order index used to reconstruct LineStrings
        public int OrderIndex { get; set; }
    }
}
