using System.ComponentModel.DataAnnotations;

namespace NRL_PROJECT.Models
{
    public class MapCoordinate
    {
        [Key]
        public int CoordinateId { get; set; }

        public int MapDataID { get; set; }
        public MapData? MapData { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int OrderIndex { get; set; }
    }
}
