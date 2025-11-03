using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace NRL_PROJECT.Models
{
    // Modell for å representere opplasting av én fil (brukes i form)
    public class FileUploadModel
    {
        [Required(ErrorMessage = "Du må velge et bilde før du laster opp.")]
        [Display(Name = "Velg fil")]
        public IFormFile File { get; set; } = default!;
    }
}
