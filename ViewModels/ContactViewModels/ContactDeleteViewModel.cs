namespace CustomerManagementPractiseCS.ViewModels.ContactViewModels
{
    public class ContactDeleteViewModel
    {
        public int Id { get; set; }
        public string ContactType { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}