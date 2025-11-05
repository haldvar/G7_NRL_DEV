using System.ComponentModel.DataAnnotations;

namespace NRL_PROJECT.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
        
        // Redirecting to page?
        public string? ReturnUrl { get; set; }
    }
}
