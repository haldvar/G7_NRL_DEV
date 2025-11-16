using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRL_PROJECT.Models
{
    public class Organisation
    {
        [Key]
        public int OrgID { get; set; }

        [StringLength(100)]
        public string? OrgName { get; set; }

        [StringLength(100)]
        public string OrgContactEmail { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
