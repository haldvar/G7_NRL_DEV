using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRL_PROJECT.Models
{
    /// <summary>
    /// Domain model for an obstacle (reported hazard).
    /// Stores type, dimensions, optional image and related MapData.
    /// </summary>
    public class ObstacleData
    {
        // Primary key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ObstacleID { get; set; }

        // Type is required, defaulting to "Ukjent"
        [Required]
        public string ObstacleType { get; set; } = "Ukjent";

        // Physical dimensions
        public double ObstacleHeight { get; set; }
        public double ObstacleWidth { get; set; }

        // Free-form comment about the obstacle
        public string? ObstacleComment { get; set; }

        // URL / path to image stored under wwwroot/uploads
        [StringLength(255)]
        public string? ObstacleImageURL { get; set; }

        // Cached primary location (may be redundant with MapData.Coordinates)
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Navigation to MapData (contains the actual GeoJSON + coordinates)
        public MapData MapData { get; set; } = new MapData
        {
            GeoJsonCoordinates = string.Empty
        };

        // File upload helpers — not mapped to DB
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [NotMapped]
        public string? ImagePath { get; set; }

        // Reports associated with this obstacle
        public ICollection<ObstacleReportData> ObstacleReports { get; set; } = new List<ObstacleReportData>();
    }
}
