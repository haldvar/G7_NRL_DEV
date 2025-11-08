using System.ComponentModel.DataAnnotations;

namespace NRL_PROJECT.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passordene matcher ikke.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Fornavn")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Etternavn")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Organisasjons ID")]
        public int OrgID { get; set; }

        [Required]
        [Display(Name = "Rolle ID")]
        public int RoleID { get; set; }
    }

};
