using CustomerManagementPractiseCS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementPractiseCS.Data
{
    // Instead of DbContext we use the IdentityDbContext this adds the Identitiy Assets
    public class AppDbContext: IdentityDbContext  
    {

        // Setup Db Context
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Table Connections
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerDetail> CustomersDetail { get; set; }
    }
}
