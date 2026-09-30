using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.Models
{
    public class Education
    {
        public int Id { get; set; }

        public int PersonId { get; set; }
        public Person? Person { get; set; }

        [Required]
        [StringLength(30)]
        public string DegreeLevel { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string DegreeName { get; set; } = string.Empty;

        [StringLength(150)]
        public string? FieldOfStudy { get; set; }

        [Required]
        [StringLength(200)]
        public string InstitutionName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? BoardOrUniversity { get; set; }

        public int StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool IsOngoing { get; set; } = false;

        [Required]
        [StringLength(20)]
        public string ResultType { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Result { get; set; }

        public decimal? ResultScale { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(450)]
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [StringLength(450)]
        public string? UpdatedBy { get; set; }
        public bool Deleted { get; set; } = false;
    }
}