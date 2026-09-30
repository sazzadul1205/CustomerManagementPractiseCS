using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.Models
{
    public class Address
    {
        public int Id { get; set; }

        public int PersonId { get; set; }
        public Person? Person { get; set; }

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

        // audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(450)]
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [StringLength(450)]
        public string? UpdatedBy { get; set; }
        public bool Deleted { get; set; } = false;
    }
}