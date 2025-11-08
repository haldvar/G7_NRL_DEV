using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NRL_Prosjekt.Models
{
    public class RegistrarOverviewViewModel
    {
        // Valgt status i filteret (querystring/verdi fra dropdown)
        public string SelectedStatus { get; set; } = "Alle";

        // Alternativene i dropdown
        public IEnumerable<SelectListItem> StatusOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem("Alle", "Alle"),
            new SelectListItem("Ny", "Ny"),
            new SelectListItem("Under behandling", "Under behandling"),
            new SelectListItem("Godkjent", "Godkjent"),
            new SelectListItem("Avvist", "Avvist"),
        };

        // Resultatliste som vises
        public List<RegistrarListItemViewModel> Reports { get; set; } = new();

        // (valgfritt) tellekanter du kan vise i UI
        public int CountNy { get; set; }
        public int CountUnderBehandling { get; set; }
        public int CountGodkjent { get; set; }
        public int CountAvvist { get; set; }
        public int CountAlle { get; set; }
    }

    public class RegistrarListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public string? LatestComment { get; set; }
        public int CommentCount { get; set; }
    }
}
