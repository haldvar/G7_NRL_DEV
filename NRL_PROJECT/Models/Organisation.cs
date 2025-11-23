using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRL_PROJECT.Models
{
    /// <summary>
    /// Organisation entity used to group users and reports by organisation.
    /// </summary>
    public class Organisation
    {
        [Key]
        public int OrgID { get; set; }

        [StringLength(100)]
        public string OrgName { get; set; }

        [StringLength(100)]
        public string OrgContactEmail { get; set; }

        /// <summary>
        /// Collection of users belonging to this organisation.
        /// Initialized to avoid null checks in views/controllers.
        /// </summary>
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
