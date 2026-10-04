namespace CustomerManagementPractiseCS.ViewModels.UserManagementViewModels
{
    public class UserListItemViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string? Email { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();

        public int ProfileCount { get; set; }

        // True for the account that is signed in, so the page can hide its Delete button
        public bool IsCurrentUser { get; set; }
    }
}
