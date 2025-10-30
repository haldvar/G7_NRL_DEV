namespace NRL_PROJECT.Models
{
    public class ObstacleReportViewModel
    {
        public int ObstacleId { get; set; } // kobling til hindring

        public string ObstacleType { get; set; }
        public double ObstacleHeight { get; set; }
        public double ObstacleWidth { get; set; }
        public string ObstacleComment { get; set; }

        public string GeoJsonCoordinates { get; set; } // brukes til karttegning
        public double Longitude { get; set; }           // brukes til database
        public double Latitude { get; set; }            // brukes til database

        public int ObstacleDataID { get; set; }
    }
}


