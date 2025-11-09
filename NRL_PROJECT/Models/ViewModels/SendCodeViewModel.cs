using Microsoft.AspNetCore.Mvc.Rendering;

namespace NRL_PROJECT.Models.ViewModels;

public class SendCodeViewModel
{
    public string SelectedProvider { get; set; }

    public ICollection<SelectListItem> Providers { get; set; }

    public string ReturnUrl { get; set; }

    public bool RememberMe { get; set; }
}
