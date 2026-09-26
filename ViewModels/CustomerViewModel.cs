using CustomerManagementPractiseCS.Models;

namespace CustomerManagementPractiseCS.ViewModels
{
    public class CustomerViewModel
    {
        public Customer Customer { get; set; } = null!;

        public CustomerDetail? ActiveDetail { get; set; }
    }
}