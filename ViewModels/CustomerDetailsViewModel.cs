using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.ViewModels
{
    public class CustomerDetailsViewModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }

        [Required]
        [Phone]
        public string? Phone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? ProfileImage { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)]
        public string? City { get; set; }

        [StringLength(20)]
        public string? Country { get; set; }

        [Required]
        [StringLength(100)]
        public string Address { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}