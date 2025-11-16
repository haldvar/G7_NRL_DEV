using System.Collections.Generic;

namespace NRL_PROJECT.Models.ViewModels
{
    public class UserManagementViewModel
    {
        public string UserID { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        // 🚀 Organisasjon
        public int? OrgID { get; set; }
        public string? OrgName { get; set; }

        // 🚀 Roller
        public string CurrentRole { get; set; }
        public List<string> AvailableRoles { get; set; }
    }
}
