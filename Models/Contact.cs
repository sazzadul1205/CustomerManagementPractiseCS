using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.Models
{
    public class Contact
    {
        public int Id { get; set; }

        public int PersonId { get; set; }
        public Person? Person { get; set; }

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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(450)]
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [StringLength(450)]
        public string? UpdatedBy { get; set; }
        public bool Deleted { get; set; } = false;
    }
}