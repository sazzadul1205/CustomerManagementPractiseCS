using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.ViewModels.Profile
{
    public class ProfileEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Gender { get; set; } = string.Empty;

        [Required]
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
    }
}