using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.ViewModels
{
    public class CustomerViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public required string Gender { get; set; }

        [StringLength(1000)]
        public string? BioData { get; set; }
    }
}
