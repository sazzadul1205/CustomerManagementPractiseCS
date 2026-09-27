using CustomerManagementPractiseCS.Models;

namespace CustomerManagementPractiseCS.ViewModels
{
    public class CustomerActiveDetailsViewModel
    {
        public Customer Customer { get; set; } = null!;

        public CustomerDetail? ActiveDetail { get; set; }
    }
}