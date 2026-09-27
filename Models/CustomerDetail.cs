using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.Models
{
    public class CustomerDetail
    {
        public int Id {  get; set; }

        // this is the foregn key that is connected to the Customer table
        public int CustomerId {  get; set; }

        // Thisd is a Logical Connectjion to Connect to the Customer this dosent create atable Field 
        public Customer? Customer { get; set; }  

        [Required]
        [Phone]
        public string? Phone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? ProfileImage { get; set; } // "?" allowes the content to be null

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)]
        public string? City { get; set; }

        [StringLength(20)]
        public string? Country { get; set; }

        [Required]
        [StringLength(100)]
        public string Address { get; set; } = string.Empty; // "string.Empty" this allowes the Content to be empty 

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool Deleted { get; set; } = false;

    }
}
