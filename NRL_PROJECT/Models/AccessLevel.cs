using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRL_PROJECT.Models
{
    public class AccessLevel
    {
        [Key]
        public int AccessLevelID { get; set; }

        [Required, StringLength(100)]
        public string AccessLevelName { get; set; }

        [StringLength(200)]
        public string AccessLevelDescription { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }
}
