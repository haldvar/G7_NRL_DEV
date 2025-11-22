using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace NRL_PROJECT.Models
{
    /// <summary>
    /// Simple model used to represent a single file upload in forms.
    /// Validation attributes produce user-friendly error messages in the UI.
    /// </summary>
    public class FileUploadModel
    {
        [Required(ErrorMessage = "Du må velge en fil før du laster opp.")]
        [Display(Name = "Velg fil")]
        public IFormFile File { get; set; } = default!;
    }
}
