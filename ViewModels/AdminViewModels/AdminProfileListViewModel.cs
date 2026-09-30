namespace CustomerManagementPractiseCS.ViewModels.AdminViewModels
{
    public class AdminProfileListViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string? BloodGroup { get; set; }
        public string? Religion { get; set; }
        public string? PhotoUrl { get; set; }
        public string? City { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}