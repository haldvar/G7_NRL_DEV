using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRL_PROJECT.Models
{
    public class UserRole
    {
       
            [Key]
            public int RoleID { get; set; }

            [Required, StringLength(100)]
            public UserRoleType RoleName { get; set; }

            [ForeignKey("AccessLevel")]
            public int AccessLevelID { get; set; }

            public AccessLevel AccessLevel { get; set; }

            public ICollection<User> Users { get; set; }
       
    }
}
