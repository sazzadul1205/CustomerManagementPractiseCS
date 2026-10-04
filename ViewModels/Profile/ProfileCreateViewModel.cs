using System.ComponentModel.DataAnnotations;
using CustomerManagementPractiseCS.Data.Validations;

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
        [NotFutureDate(ErrorMessage = "Date of birth cannot be a future date.")]
        public DateOnly DateOfBirth { get; set; }

        [StringLength(50)]
        public string? Religion { get; set; }

        [StringLength(20)]
        public string? BloodGroup { get; set; }

        [StringLength(2000)]
        public string? Summary { get; set; }
        public IFormFile? PhotoFile { get; set; }
    }
}