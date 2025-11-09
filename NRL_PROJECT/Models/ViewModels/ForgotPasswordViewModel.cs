using System.ComponentModel.DataAnnotations;

namespace NRL_PROJECT.Models.ViewModels;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
