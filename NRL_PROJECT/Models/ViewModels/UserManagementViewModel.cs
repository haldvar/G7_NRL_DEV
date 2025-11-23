using Microsoft.AspNetCore.Mvc.Rendering;

namespace NRL_PROJECT.Models.ViewModels
{
    /// <summary>
    /// View model used by the Admin UI to represent a user and available actions.
    /// Contains basic identity information, current role and available organisations/roles.
    /// </summary>
    public class UserManagementViewModel
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? OrgName { get; set; }
        public string CurrentRole { get; set; }
        public List<string> AvailableRoles { get; set; }
        public int? OrgID { get; set; }
        public List<SelectListItem>? AvailableOrganizations { get; set; }
    }
}
