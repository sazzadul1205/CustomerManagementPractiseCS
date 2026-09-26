using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagementPractiseCS.Controllers
{
    public class CustomerDetailController : Controller
    {
        private readonly AppDbContext _context;

        // DI Injection
        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Create(int customersId) {
            var customer = _context.Customers.Find(customersId);

            if (customer == null)
            {
                return NotFound();
            }

            var detail = new CustomerDetail
            {
                CustomerId = customerId
            };

            return View(detail);
        }

        [HttpPost]
        public IActionResult Create(CustomerDetail customerDetail)
        {
            if (ModelState.IsValid)
            {

                // Check if the Detail is Active
                if (customerDetail.IsActive)
                {
                    // Get all existing addresses 
                    var existingDetails = _context.CustomerDetails
                        .Where(x => x.CustomerId == customerDetail.CustomerId)
                        .ToList();

                    // Loop through each one and Deactivate the rest 
                    foreach (var existingDetail in existingDetails)
                    {
                        existingDetail.IsActive = false;
                    }
                }

                _context.CustomerDetails.Add(customerDetail);
                _context.SaveChanges();


                return RedirectToAction(
                    "Details",
                    "Customers",
                    new { id = customerDetail.CustomerId }
                );
            }

            return View(customerDetail);
        }
    }
}
