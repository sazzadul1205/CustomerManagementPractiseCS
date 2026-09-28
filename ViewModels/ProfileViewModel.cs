using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.ViewModels
{
    public class ProfileViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Username")]
        public string? UserName { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();
    }
}