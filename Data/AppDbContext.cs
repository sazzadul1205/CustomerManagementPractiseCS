using CustomerManagementPractiseCS.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementPractiseCS.Data
{
    public class AppDbContext: DbContext
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
