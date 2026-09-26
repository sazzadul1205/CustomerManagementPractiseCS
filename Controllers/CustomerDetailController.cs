using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagementPractiseCS.Controllers
{
    public class CustomerDetailController : Controller
    {
        private readonly AppDbContext _context;

        // DI Injection
        public CustomerDetailController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CustomerDetail/Create?customerId=5
        public IActionResult Create(int customerId)
        {
            var customer = _context.Customers.Find(customerId);

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

        // POST: CustomerDetail/Create
        [HttpPost]

        public IActionResult Create(CustomerDetail customerDetail)
        {
            // Make sure the customer actually exists (prevents overposting CustomerId)
            if (!_context.Customers.Any(c => c.Id == customerDetail.CustomerId))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // If this record is marked active, deactivate all others for this customer
                if (customerDetail.IsActive)
                {
                    // Get all other CustomerDetail records for this customer
                    var others = _context.CustomersDetail
                        .Where(x => x.CustomerId == customerDetail.CustomerId)
                        .ToList();

                    foreach (var other in others)
                    {
                        other.IsActive = false;
                    }
                }

                _context.CustomersDetail.Add(customerDetail);
                _context.SaveChanges();

                return RedirectToAction(
                    "Details",
                    "Customers",
                    new { id = customerDetail.CustomerId }
                );
            }

            return View(customerDetail);
        }

        // GET: CustomerDetail/Edit/5
        public IActionResult Edit(int id)
        {
            var customerDetail = _context.CustomersDetail.Find(id);

            if (customerDetail == null)
            {
                return NotFound();
            }

            return View(customerDetail);
        }

        // POST: CustomerDetail/Edit/5
        [HttpPost]

        public IActionResult Edit(int id, CustomerDetail customerDetail)
        {
            if (id != customerDetail.Id)
            {
                return NotFound();
            }

            var existing = _context.CustomersDetail.Find(id);

            if (existing == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // If this record is marked active, deactivate all others for this customer
                if (customerDetail.IsActive)
                {
                    // Get all other CustomerDetail records for this customer
                    var others = _context.CustomersDetail
                        .Where(x => x.CustomerId == existing.CustomerId && x.Id != id)
                        .ToList();

                    foreach (var other in others)
                    {
                        other.IsActive = false;
                    }
                }

                // Fields 
                existing.Phone = customerDetail.Phone;
                existing.Email = customerDetail.Email;
                existing.DateOfBirth = customerDetail.DateOfBirth;
                existing.City = customerDetail.City;
                existing.Country = customerDetail.Country;
                existing.Address = customerDetail.Address;
                existing.ProfileImage = customerDetail.ProfileImage;
                existing.IsActive = customerDetail.IsActive;

                _context.SaveChanges();

                return RedirectToAction(
                    "Details",
                    "Customers",
                    new { id = existing.CustomerId }
                );
            }

            return View(customerDetail);
        }

        // POST: CustomerDetail/SetActive/5
        [HttpPost]

        public IActionResult SetActive(int id)
        {
            var customerDetail = _context.CustomersDetail.Find(id);

            if (customerDetail == null)
            {
                return NotFound();
            }

            if (!customerDetail.IsActive)
            {
                // Get all other CustomerDetail records for this customer
                var others = _context.CustomersDetail
                    .Where(x => x.CustomerId == customerDetail.CustomerId && x.Id != id)
                    .ToList();

                foreach (var other in others)
                {
                    other.IsActive = false;
                }

                customerDetail.IsActive = true;
                _context.SaveChanges();
            }

            return RedirectToAction(
                "Details",
                "Customers",
                new { id = customerDetail.CustomerId }
            );
        }

        // GET: CustomerDetail/Delete/5
        public IActionResult Delete(int id)
        {
            var customerDetail = _context.CustomersDetail.Find(id);

            if (customerDetail == null)
            {
                return NotFound();
            }

            return View(customerDetail);
        }

        // POST: CustomerDetail/Delete/5
        [HttpPost, ActionName("Delete")]

        public IActionResult DeleteConfirmed(int id)
        {
            var customerDetail = _context.CustomersDetail.Find(id);

            if (customerDetail == null)
            {
                return NotFound();
            }

            int customerId = customerDetail.CustomerId;

            _context.CustomersDetail.Remove(customerDetail);
            _context.SaveChanges();

            return RedirectToAction(
                "Details",
                "Customers",
                new { id = customerId }
            );
        }
    }
}