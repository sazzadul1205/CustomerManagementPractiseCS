using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.ViewModels.SocialLinkViewModels
{
    public class SocialLinkEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Platform { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Url]
        public string Url { get; set; } = string.Empty;
    }
}