using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using CustomerManagementPractiseCS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CustomerManagementPractiseCS.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CustomersController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Customer
        // Admin Only : This Lists Every Customer in the System
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            // Get Every Customer with the Active Details Attached
            var customers = _context.Customers
                .Include(x => x.Details)
                .Select(x => new CustomerActiveDetailsViewModel
                {
                    Customer = x,
                    ActiveDetail = x.Details.FirstOrDefault(x => x.IsActive)
                }).ToList();

            return View(customers);
        }

        public IActionResult MyData()
        {
            //User is a property automatically available in every controller(inherited from ControllerBase)
            // Get the Current User Id Visa Manager 
            string? userId = _userManager.GetUserId(User);

            // Get the Customer With My User Id and then Including the Details and then Modfel Bainding 
            var customers = _context.Customers
                .Where(x => x.UserId == userId)
                .Include(x => x.Details)
                .Select(x => new CustomerActiveDetailsViewModel
                {
                    Customer = x,
                    ActiveDetail = x.Details.FirstOrDefault(x => x.IsActive)
                }).FirstOrDefault();

            return View(customers);
        }

        // Customer/Create
        public IActionResult Create()
        {
            // A Regular User only ever Sees One Customer Profile (MyData)
            // so Stop them from Creating a Second one that they can never see
            if (!User.IsInRole("Admin") && _context.Customers.Any(x => x.UserId == _userManager.GetUserId(User)))
            {
                return RedirectToAction("MyData");
            }

            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        public IActionResult Create(CustomerViewModel customerVM)
        {
            string? userId = _userManager.GetUserId(User);

            if (customerVM == null)
            {
                return View();
            }

            // Same Guard as the GET : a Regular User gets Exactly One Profile
            if (!User.IsInRole("Admin") && _context.Customers.Any(x => x.UserId == userId))
            {
                return RedirectToAction("MyData");
            }

            // Check oif the Model state is Matching the Validation
            if (ModelState.IsValid)
            {

                // Map ViewModel → Entity
                var customer = new Customer
                {
                    Name = customerVM.Name,
                    Gender = customerVM.Gender,
                    BioData = customerVM.BioData,
                    UserId = userId
                };

                // Add the cutomer datas to the DB not pushed just staged
                _context.Customers.Add(customer);

                // Now pushed in async formayt there is one without async
                _context.SaveChanges();

                // Role Based Redirect
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index");
                }

                return RedirectToAction("MyData");
            }

            // If Fail return to create

            return View(customerVM);
        }

        // Customer/{id}
        public IActionResult Details(int id)
        {
            // Get the Current User Id
            string? userId = _userManager.GetUserId(User);

            //var customer = _context.Customers.FirstOrDefault(x => x.Id == id);
            var customer = _context.Customers.Include(cd => cd.Details).FirstOrDefault(x => x.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            // Check if the Customer Belongs to the Current User
            if (customer.UserId != userId)
            {
                return Forbid();
            }

            return View(customer);
        }

        // Customer/Edit/{id}
        public IActionResult Edit(int id)
        {
            // Get the Current User Id
            string? userId = _userManager.GetUserId(User);

            var customer = _context.Customers.FirstOrDefault(x => x.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            // Check if the Customer Belongs to the Current User
            if (customer.UserId != userId)
            {
                return Forbid();
            }

            // Map Entity → ViewModel
            var customerVM = new CustomerViewModel
            {
                Id = customer.Id,
                Name = customer.Name,
                Gender = customer.Gender,
                BioData = customer.BioData,
                UserId = customer.UserId
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

            // Get the Current User Id
            string? userId = _userManager.GetUserId(User);

            // Get the Previous Customer Data
            var previousCustomer = _context.Customers.FirstOrDefault(x => x.Id == id);


            if (previousCustomer == null)
            {
                return NotFound();
            }

            // Check if the Customer Belongs to the Current User
            if (previousCustomer.UserId != userId)
            {
                return Forbid();
            }

            // If Data is Valid
            if (ModelState.IsValid)
            {

                // Register Changes
                previousCustomer.Name = customerVM.Name;
                previousCustomer.Gender = customerVM.Gender;
                previousCustomer.BioData = customerVM.BioData;
                // UserId is NOT touched, Owner Never Changes

                _context.SaveChanges();

                // Role Based Redirect
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index");
                }

                return RedirectToAction("MyData");
            }

            return View(customerVM);
        }

        // Customer/Delete/{id}
        public IActionResult Delete(int id)
        {
            // Get the Current User Id
            string? userId = _userManager.GetUserId(User);

            var customer = _context.Customers.FirstOrDefault(x => x.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            // Check if the Customer Belongs to the Current User
            if (customer.UserId != userId)
            {
                return Forbid();
            }

            // Map Entity → ViewModel
            var customerVM = new CustomerViewModel
            {
                Id = customer.Id,
                Name = customer.Name,
                Gender = customer.Gender,
                BioData = customer.BioData,
                UserId = customer.UserId
            };


            return View(customerVM);
        }

        // POST: Customer/Delete/{id}
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int id)
        {
            // Get the Current User Id
            string? userId = _userManager.GetUserId(User);

            // Find the Customer with the Details Attached 
            var customer = _context.Customers.Include(c => c.Details).FirstOrDefault(x => x.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            // Check if the Customer Belongs to the Current User
            if (customer.UserId != userId)
            {
                return Forbid();
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

            // Role Based Redirect
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("MyData");
        }

    }
}