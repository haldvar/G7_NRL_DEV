namespace NRL_PROJECT.Models
{
    public class ObstacleReportViewModel
    {
        public int ObstacleId { get; set; } // kobling til hindring

        public string ObstacleName { get; set; }
        public string ObstacleType { get; set; }
        public int ObstacleHeight { get; set; }
        public int ObstacleWidth { get; set; }
        public string ObstacleDescription { get; set; }

        public string GeoJsonCoordinates { get; set; } // brukes til karttegning
        public double Longitude { get; set; }           // brukes til database
        public double Latitude { get; set; }            // brukes til database
    }

}
