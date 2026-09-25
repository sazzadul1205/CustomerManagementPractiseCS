using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public required string Gender { get; set; }

        [StringLength(1000)]
        public string? BioData { get; set; }

        // This Indecates that the Customer Table has a One-To-Many Relaionship with CustomerDetail Table
        public ICollection<CustomerDetail> Details { get; set; } = new List<CustomerDetail>();
    }
}
