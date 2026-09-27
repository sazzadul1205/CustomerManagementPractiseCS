using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using CustomerManagementPractiseCS.ViewModels;

namespace CustomerManagementPractiseCS.Controllers
{
    public class CustomersController : Controller
    {
        private readonly AppDbContext _context;

        // DI Injection
        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        // Customer
        public IActionResult Index()
        {
            // Get the Customer with the Active Details Attached 
            var customers = _context.Customers.Include(x => x.Details).Select(x => new CustomerActiveDetailsViewModel
            {
                Customer = x,
                ActiveDetail = x.Details.FirstOrDefault(x => x.IsActive)
            }).ToList();

            // Build view models with the active detail picked out
            //var Data = customers.Select(x => new CustomerActiveDetailsViewModel
            //{
            //    Customer = x,
            //    ActiveDetail = x.Details.FirstOrDefault(x => x.IsActive), 
            //}).ToList();


            return View(customers);
        }

        // Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        public IActionResult Create(CustomerViewModel customerVM)
        {
            if (customerVM == null)
            {
                return View();
            }

            // Check oif the Model state is Matching the Validation
            if (ModelState.IsValid)
            {

                // Map ViewModel → Entity
                var customer = new Customer
                {
                    Name = customerVM.Name,
                    Gender = customerVM.Gender,
                    BioData = customerVM.BioData
                };

                // Add the cutomer datas to the DB not pushed just staged
                _context.Customers.Add(customer);

                // Now pushed in async formayt there is one without async
                _context.SaveChanges();
                //_context.SaveChangesAsync();

                // This Redairects to Index page
                return RedirectToAction(nameof(Index));
            }

            // If Fail return to create

            return View(customerVM);
        }

        // Customer/{id}
        public IActionResult Details(int id)
        {
            //var customer = _context.Customers.FirstOrDefault(x => x.Id == id);
            var customer = _context.Customers.Include(cd => cd.Details).FirstOrDefault(x => x.Id == id);

            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // Customer/Edit/{id}
        public IActionResult Edit(int id)
        {
            var customer = _context.Customers.FirstOrDefault(x => x.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            // Map Entity → ViewModel
            var customerVM = new CustomerViewModel
            {
                Id = customer.Id,
                Name = customer.Name,
                Gender = customer.Gender,
                BioData = customer.BioData
            };

            return View(customerVM);
        }

        // POST: Customer/Edit/{id}
        [HttpPost]
        public IActionResult Edit(CustomerViewModel customerVM, int id)
        {
            if (id != customerVM.Id)
            {
                return NotFound();
            }

            // Get the Previous Customer Data
            var previousCustomer = _context.Customers.FirstOrDefault(x => x.Id == id);


            if (previousCustomer == null)
            {
                return NotFound();
            }

            // If Data is Valid
            if (ModelState.IsValid)
            {

                // Register Changes
                previousCustomer.Name = customerVM.Name;
                previousCustomer.Gender = customerVM.Gender;
                previousCustomer.BioData = customerVM.BioData;

                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(customerVM);
        }

        // Customer/Delete/{id}
        public IActionResult Delete(int id)
        {
            var customer = _context.Customers.FirstOrDefault(x => x.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            // Map Entity → ViewModel
            var customerVM = new CustomerViewModel
            {
                Id = customer.Id,
                Name = customer.Name,
                Gender = customer.Gender,
                BioData = customer.BioData
            };


            return View(customerVM);
        }

        // POST: Customer/Delete/{id}
        [HttpPost, ActionName("Delete")]    
        public IActionResult DeleteConfirm(int id)
        {
            // Find the Customer with the Details Attached 
            var customer = _context.Customers.Include(c => c.Details).FirstOrDefault(x => x.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            // Check if there is any Details
            if (customer.Details != null && customer.Details.Any())
            {
                // One by One Go through all the Details and Delete 
                foreach (var detail in customer.Details)
                {
                    _context.CustomersDetail.Remove(detail);
                }
            }

            _context.Customers.Remove(customer);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
