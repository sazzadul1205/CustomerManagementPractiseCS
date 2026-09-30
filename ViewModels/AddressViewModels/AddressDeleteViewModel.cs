namespace CustomerManagementPractiseCS.ViewModels.AddressViewModels
{
    public class AddressDeleteViewModel
    {
        public int Id { get; set; }
        public string AddressType { get; set; } = string.Empty;
        public string Line1 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string Country { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}