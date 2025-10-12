using System.ComponentModel.DataAnnotations;


namespace NRL_PROJECT.Models
{
    public class ObstacleReportData
    {
        public int Report_Id { get; set; }
        public string Reported_Item { get; set; }
        public string Reported_Location { get; set; }
        public DateTime Time_of_Submitted_Report { get; set; }
    }
}
