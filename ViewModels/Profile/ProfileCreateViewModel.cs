using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CustomerManagementPractiseCS.ViewModels.Profile
{
    public class ProfileCreateViewModel
    {
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

        [StringLength(2000)]
        public string? Summary { get; set; }

        // Photo upload
        public IFormFile? PhotoFile { get; set; }
    }
}