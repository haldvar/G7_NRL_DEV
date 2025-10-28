using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRL_PROJECT.Models
{
    public class MapData
    {
        [Key]
        public int MapViewID { get; set; }

        public double CenterLatitude { get; set; }
        public double CenterLongitude { get; set; }
        public int MapZoomLevel { get; set; }

        public ICollection<ObstacleReportData> ObstacleReports { get; set; }


       
    }

}
