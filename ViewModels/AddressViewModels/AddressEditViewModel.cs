using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.ViewModels.AddressViewModels
{
    public class AddressEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string AddressType { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Line1 { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;
    }
}