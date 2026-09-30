namespace CustomerManagementPractiseCS.ViewModels.EducationViewModels
{
    public class EducationDeleteViewModel
    {
        public int Id { get; set; }
        public string DegreeLevel { get; set; } = string.Empty;
        public string DegreeName { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = string.Empty;
        public string? BoardOrUniversity { get; set; }
        public int StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool IsOngoing { get; set; }
        public string? Result { get; set; }
    }
}
