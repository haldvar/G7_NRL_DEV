using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace NRL_PROJECT.Models
{
    public class User : IdentityUser
    {
        // UserID arves fra IdentityUser (som string)
        // Email arves fra IdentityUser
        // Passord arves fra IdentityUser

        [Required, StringLength(100)]
        public string FirstName { get; set; }

        [Required, StringLength(100)]
        public string LastName { get; set; }
        
        public int? OrgID { get; set; }
        public string? OrgName { get; set; }

        [ForeignKey(nameof(OrgID))]
        public Organisation Organisation { get; set; }

        
        [ForeignKey("UserRole")]
        public int? RoleID { get; set; } // nullable
        
        public ICollection<ObstacleReportData> ObstacleReportsSubmitted { get; set; }
        public ICollection<ObstacleReportData> ObstacleReportsReviewed { get; set; }
    }
}
