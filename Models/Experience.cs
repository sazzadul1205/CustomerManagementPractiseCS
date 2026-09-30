using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.Models
{
    public class Experience
    {
        public int Id { get; set; }

        public int PersonId { get; set; }
        public Person? Person { get; set; }

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Designation { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Department { get; set; }

        [Required]
        [StringLength(20)]
        public string EmploymentType { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Location { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsCurrent { get; set; } = false;

        [StringLength(4000)]
        public string? Responsibilities { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(450)]
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [StringLength(450)]
        public string? UpdatedBy { get; set; }
        public bool Deleted { get; set; } = false;
    }
}