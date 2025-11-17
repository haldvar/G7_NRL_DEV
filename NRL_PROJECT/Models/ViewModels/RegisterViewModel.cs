using System.ComponentModel.DataAnnotations;

namespace NRL_PROJECT.Models.ViewModels
{
    public class RegisterViewModel
    {
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        // 🚀 Organisasjon velges via dropdown
        [Required]
        public int OrgID { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        public string RoleName { get; set; }
    }
}
