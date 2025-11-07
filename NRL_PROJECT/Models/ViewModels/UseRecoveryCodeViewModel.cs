using System.ComponentModel.DataAnnotations;

namespace NRL_PROJECT.Models.ViewModels;


public class UseRecoveryCodeViewModel
{
    [Required]
    public string Code { get; set; }

    public string ReturnUrl { get; set; }
}
