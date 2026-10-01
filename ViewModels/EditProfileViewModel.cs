using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.ViewModels
{
    public class EditProfileViewModel
    {

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        //[Required]
        //[Display(Name = "Username")]
        //[StringLength(50, MinimumLength = 3)]
        //public string UserName { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }
}