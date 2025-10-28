using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRL_PROJECT.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required, StringLength(100)]
        public string FirstName { get; set; }

        [Required, StringLength(100)]
        public string LastName { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }

        [Required, StringLength(255)]
        public string PasswordHash { get; set; }

        [ForeignKey("Organisation")]
        public int OrgID { get; set; }

        [ForeignKey("UserRole")]
        public int RoleID { get; set; }
              

        public UserRole Role { get; set; }
        public Organisation Organisation { get; set; }

        public ICollection<ObstacleReportData> ObstacleReportsSubmitted { get; set; }
        public ICollection<ObstacleReportData> ObstacleReportsReviewed { get; set; }
    }
}
