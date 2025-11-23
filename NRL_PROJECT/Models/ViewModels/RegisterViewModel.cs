using System.ComponentModel.DataAnnotations;

namespace NRL_PROJECT.Models.ViewModels
{
    /// <summary>
    /// View model used for user registration.
    /// - Keeps data annotations for validation.
    /// - No behavioral changes are made.
    /// </summary>
    public class RegisterViewModel
    {
        // -----------------------------
        // Credentials
        // -----------------------------

        [Required]
        [Display(Name = "Brukernavn")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Passord")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Gjenta passord")]
        [Compare("Password", ErrorMessage = "Passordene matcher ikke.")]
        public string ConfirmPassword { get; set; }

        // -----------------------------
        // Profile
        // -----------------------------

        [Required]
        [Display(Name = "Fornavn")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Etternavn")]
        public string LastName { get; set; }

        // -----------------------------
        // Organisation / role selection (optional)
        // -----------------------------

        [Display(Name = "Organisasjons ID")]
        public int? OrgID { get; set; }

        [Display(Name = "Rolle ID")]
        public int? RoleID { get; set; }

        [Display(Name = "Rolle")]
        public string? RoleName { get; set; }
    }
}
