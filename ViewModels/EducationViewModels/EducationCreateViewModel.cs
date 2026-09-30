using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.ViewModels.EducationViewModels
{
    public class EducationCreateViewModel
    {
        [Required]
        [StringLength(30)]
        public string DegreeLevel { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string DegreeName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string InstitutionName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? BoardOrUniversity { get; set; }

        [Required]
        public int StartYear { get; set; }

        public int? EndYear { get; set; }

        public bool IsOngoing { get; set; } = false;

        [StringLength(20)]
        public string? Result { get; set; }
    }
}
