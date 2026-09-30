using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.Models
{
    public class FamilyMember
    {
        public int Id { get; set; }

        public int PersonId { get; set; }
        public Person? Person { get; set; }

        [Required]
        [StringLength(30)]
        public string Relation { get; set; } = string.Empty;

        [StringLength(50)]
        public string? RelationOther { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Occupation { get; set; }

        [StringLength(30)]
        public string? ContactNumber { get; set; }

        public DateOnly? DateOfBirth { get; set; }
        public bool IsDeceased { get; set; } = false;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(450)]
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [StringLength(450)]
        public string? UpdatedBy { get; set; }
        public bool Deleted { get; set; } = false;
    }
}