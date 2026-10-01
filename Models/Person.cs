using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.Models
{
    public class Person
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string? UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;


        [Required]
        [StringLength(20)]
        public string Gender { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateOnly DateOfBirth { get; set; }

        [StringLength(50)]
        public string? Religion { get; set; }

        [StringLength(20)]
        public string? BloodGroup { get; set; }

        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        [StringLength(2000)]
        public string? Summary { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [StringLength(450)]
        public string? UpdatedBy { get; set; }

        public bool Deleted { get; set; } = false;

      
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public ICollection<Education> Educations { get; set; } = new List<Education>();
        public ICollection<Experience> Experiences { get; set; } = new List<Experience>();
        public ICollection<SocialLink> SocialLinks { get; set; } = new List<SocialLink>();
    }
}