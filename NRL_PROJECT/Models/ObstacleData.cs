using System.ComponentModel.DataAnnotations;

namespace NRL_PROJECT.Models
{
    public class ObstacleData
    {
        [Required(ErrorMessage = "Obstacle name is required")]
        public string ObstacleName { get; set; }

        [Required(ErrorMessage = "Obstacle type is required")]
        public string ObstacleType { get; set; }

        [Range(1, 1000, ErrorMessage = "Height must be between 1 and 1000")]
        public int ObstacleHeight { get; set; }

        [Range(1, 1000, ErrorMessage = "Width must be between 1 and 1000")]
        public int ObstacleWidth { get; set; }

        public string ObstacleDescription { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

    }
}