namespace CustomerManagementPractiseCS.ViewModels.ExperienceViewModels
{
    public class ExperienceDeleteViewModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string EmploymentType { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? Responsibilities { get; set; }
    }
}