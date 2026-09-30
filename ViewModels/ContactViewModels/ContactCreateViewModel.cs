using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.ViewModels.ContactViewModels
{
    public class ContactCreateViewModel
    {
        [Required]
        [StringLength(20)]
        public string ContactType { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Label { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Value { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;
    }
}