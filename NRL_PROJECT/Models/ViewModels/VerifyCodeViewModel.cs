using System.ComponentModel.DataAnnotations;

namespace NRL_PROJECT.Models.ViewModels;

public class VerifyCodeViewModel
{
    [Required]
    public string Provider { get; set; }

    [Required]
    public string Code { get; set; }

    public string ReturnUrl { get; set; }

    [Display(Name = "Husk denne nettleseren?")]
    public bool RememberBrowser { get; set; }

    [Display(Name = "Husk meg?")]
    public bool RememberMe { get; set; }
}
