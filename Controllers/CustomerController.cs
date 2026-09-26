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

        public IActionResult Index()
        {
            var customers = _context.Customers.Include(x => x.Details).ToList();

            // Build view models with the active detail picked out
            var Data = customers.Select(x => new CustomerViewModel
            {
                Customer = x,
                ActiveDetail = x.Details.FirstOrDefault(x => x.IsActive), 
            }).ToList();


            return View(Data);
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
            //var customer = _context.Customers.FirstOrDefault(x => x.Id == id);
            var customer = _context.Customers.Include(cd => cd.Details).FirstOrDefault(x => x.Id == id);

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
        public IActionResult Edit(Customer customer, int id)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            var previousCustomer = _context.Customers.Find(id);


            if (previousCustomer == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                previousCustomer.Name = customer.Name;
                previousCustomer.Gender = customer.Gender;
                previousCustomer.BioData = customer.BioData;

                _context.SaveChanges();
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

        [HttpPost, ActionName("Delete")]    
        public IActionResult DeleteConfirm(int id)
        {
            var customer = _context.Customers.Include(c => c.Details).FirstOrDefault(x => x.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            // Remove all child CustomerDetail rows first
            if (customer.Details != null && customer.Details.Any())
            {
                foreach (var detail in customer.Details)
                {
                    _context.CustomersDetail.Remove(detail);
                }
            }

            _context.Customers.Remove(customer);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}
