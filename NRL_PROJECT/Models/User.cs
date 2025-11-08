using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace NRL_PROJECT.Models
{
    public class User : IdentityUser
    {
         // ID arves fra IdentityUser (som string)
         // Email arves fra IdentityUser
         // Passord arves fra IdentityUser

        [Required, StringLength(100)]
        public string FirstName { get; set; }

        [Required, StringLength(100)]
        public string LastName { get; set; }
        
        public int? OrgID { get; set; } // nullable
        public int? RoleID { get; set; } // nullable
              
        [ForeignKey("OrgID")]
        public Organisation? Organisation { get; set; }
        
        [ForeignKey("RoleID")]
        public UserRole? Role { get; set; }

        public ICollection<ObstacleReportData> ObstacleReportsSubmitted { get; set; }
        public ICollection<ObstacleReportData> ObstacleReportsReviewed { get; set; }
    }
}
