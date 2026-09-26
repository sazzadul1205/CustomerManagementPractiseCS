using Azure.Identity;
using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using Microsoft.AspNetCore.Mvc;

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

        public IActionResult Index()
        {
            var Customer = _context.Customers.ToList();
            return View(Customer);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Customer customer)
        {
            // Check oif the Model state is Matching the Validation
            if (ModelState.IsValid)
            {
                // Add the cutomer datas to the DB not pushed just staged
                _context.Customers.Add(customer);

                // Now pushed in async formayt there is one without async
                _context.SaveChanges();
                //_context.SaveChangesAsync();

                // This Redairects to Index page
                return RedirectToAction(nameof(Index));
            }

            // If Fail return to create

            return View();
        }

        public IActionResult Details(int id)
        {
            var customer = _context.Customers.FirstOrDefault(x => x.Id == id);

            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        public IActionResult Edit(int id)
        {
            var customer = _context.Customers.FirstOrDefault(x => x.Id == id);

            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        public IActionResult Edit(Customer customer, int id) {
            if (id != customer.Id) {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update the cutomer datas to the DB not pushed just staged
                _context.Customers.Update(customer);

                _context.SaveChanges();

                // This Redairects to Index page
                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }


        public IActionResult Delete(int id)
        {
            var customer = _context.Customers.FirstOrDefault(x => x.Id == id);

            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        public IActionResult DeleteConfirm(int id)
        {
            var customer = _context.Customers.FirstOrDefault(x => x.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}
