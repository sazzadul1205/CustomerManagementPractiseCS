using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public required string Gender { get; set; }

        [StringLength(1000)]
        public string? BioData { get; set; }

        public string? UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public bool Deleted { get; set; } = false;

        public ICollection<CustomerDetail> Details { get; set; } = new List<CustomerDetail>();
    }
}