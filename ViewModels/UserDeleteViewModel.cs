namespace CustomerManagementPractiseCS.ViewModels
{
    public class UserDeleteViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string? Email { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();

        public int CustomerCount { get; set; }
    }
}