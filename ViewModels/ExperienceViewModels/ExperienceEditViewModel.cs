using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.ViewModels.ExperienceViewModels
{
    public class ExperienceEditViewModel
    {
        public int Id { get; set; }

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

        [Required]
        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? EndDate { get; set; }

        public bool IsCurrent { get; set; } = false;

        [StringLength(4000)]
        public string? Responsibilities { get; set; }
    }
}