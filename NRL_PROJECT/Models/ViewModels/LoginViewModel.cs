using System.ComponentModel.DataAnnotations;

namespace NRL_PROJECT.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Epost")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Passord")]
        public string Password { get; set; }

        [Display(Name = "Husk meg")]
        public bool RememberMe { get; set; }
        
    }
}
