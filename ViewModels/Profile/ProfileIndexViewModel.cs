using CustomerManagementPractiseCS.Models;

namespace CustomerManagementPractiseCS.ViewModels.Profile
{
    public class ProfileIndexViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string? Religion { get; set; }
        public string? BloodGroup { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Summary { get; set; }

        public List<Address> Addresses { get; set; } = new();
        public List<Contact> Contacts { get; set; } = new();
        public List<Education> Educations { get; set; } = new();
        public List<Experience> Experiences { get; set; } = new();
        public List<SocialLink> SocialLinks { get; set; } = new();
    }
}