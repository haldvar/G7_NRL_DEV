using System.ComponentModel.DataAnnotations;

namespace NRL_PROJECT.Models.ViewModels;

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "{0} måre være minimun {2} karakterer lang.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Bekreft passord")]
    [Compare("Passord", ErrorMessage = "Passordene matcher ikke.")]
    public string ConfirmPassword { get; set; }

    public string Code { get; set; }
}
