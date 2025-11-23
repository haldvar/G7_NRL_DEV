using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace NRL_PROJECT.Models
{
    /// <summary>
    /// Application user extending IdentityUser.
    /// Adds profile fields and organisation linkage used across the app.
    /// </summary>
    public class User : IdentityUser
    {
        // -----------------------------
        // Profile fields
        // -----------------------------

        [Required, StringLength(100)]
        public string FirstName { get; set; }

        [Required, StringLength(100)]
        public string LastName { get; set; }

        // -----------------------------
        // Organisation linkage
        // -----------------------------

        /// <summary>
        /// Nullable FK to Organisation (may be null when user not assigned).
        /// </summary>
        public int? OrgID { get; set; }

        /// <summary>
        /// Cached organisation name (keeps a denormalized copy on the user).
        /// </summary>
        public string? OrgName { get; set; }

        /// <summary>
        /// Navigation to the Organisation entity.
        /// May be null if user has no organisation assigned.
        /// </summary>
        [ForeignKey(nameof(OrgID))]
        public Organisation Organisation { get; set; }

        // -----------------------------
        // Navigation collections
        // -----------------------------

        /// <summary>
        /// Reports submitted by this user.
        /// Initialized to avoid null-reference when iterating.
        /// </summary>
        public ICollection<ObstacleReportData> ObstacleReportsSubmitted { get; set; } = new List<ObstacleReportData>();

        /// <summary>
        /// Reports reviewed/handled by this user (as registrar/reviewer).
        /// Initialized to avoid null-reference when iterating.
        /// </summary>
        public ICollection<ObstacleReportData> ObstacleReportsReviewed { get; set; } = new List<ObstacleReportData>();
    }
}
