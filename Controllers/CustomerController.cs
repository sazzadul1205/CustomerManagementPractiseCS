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
                .Where(c => !c.Deleted)
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
                .Where(x => x.UserId == userId && !x.Deleted)
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
            if (!User.IsInRole("Admin") && _context.Customers.Any(x => x.UserId == _userManager.GetUserId(User) && !x.Deleted))
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
            if (!User.IsInRole("Admin") &&
                _context.Customers.Any(x => x.UserId == userId && !x.Deleted))
            {
                return RedirectToAction("MyData");
            }

            // Check oif the Model state is Matching the Validation
            if (ModelState.IsValid)
            {
                var existing = _context.Customers.FirstOrDefault(x => x.UserId == userId);

                if (existing != null)
                {
                    // Reuse the row: restore + update
                    existing.Name = customerVM.Name;
                    existing.Gender = customerVM.Gender;
                    existing.BioData = customerVM.BioData;
                    existing.Deleted = false;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = userId;
                }
                else
                {
                    // Map ViewModel → Entity
                    var customer = new Customer
                    {
                        Name = customerVM.Name,
                        Gender = customerVM.Gender,
                        BioData = customerVM.BioData,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    };

                    // Add the cutomer datas to the DB not pushed just staged
                    _context.Customers.Add(customer);
                }

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
            var customer = _context.Customers
                .Where(c => c.Id == id && !c.Deleted)
                .Include(cd => cd.Details.Where(d => !d.Deleted))
                .FirstOrDefault(x => x.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            // Check if the Customer Belongs to the Current User
            if (customer.UserId != userId && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(customer);
        }

        // Customer/Edit/{id}
        public IActionResult Edit(int id)
        {
            // Get the Current User Id
            string? userId = _userManager.GetUserId(User);

            var customer = _context.Customers.FirstOrDefault(x => x.Id == id && !x.Deleted);

            if (customer == null)
            {
                return NotFound();
            }

            // Check if the Customer Belongs to the Current User
            if (customer.UserId != userId && !User.IsInRole("Admin"))

            {
                return RedirectToAction("AccessDenied", "Account");
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

            // Check if the Customer Belongs to the Current User or the User Is Admin
            if (previousCustomer.UserId != userId && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // If Data is Valid
            if (ModelState.IsValid)
            {

                // Register Changes
                previousCustomer.Name = customerVM.Name;
                previousCustomer.Gender = customerVM.Gender;
                previousCustomer.BioData = customerVM.BioData;
                previousCustomer.UpdatedAt = DateTime.UtcNow;
                previousCustomer.UpdatedBy = userId;

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

            var customer = _context.Customers.FirstOrDefault(x => x.Id == id && !x.Deleted);

            if (customer == null)
            {
                return NotFound();
            }

            // Check if the Customer Belongs to the Current User or the User Is Admin 
            if (customer.UserId != userId && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
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

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int id)
        {
            string? userId = _userManager.GetUserId(User);

            var customer = _context.Customers
                .Include(c => c.Details)
                .FirstOrDefault(x => x.Id == id && !x.Deleted);

            if (customer == null)
            {
                return NotFound();
            }

            if (customer.UserId != userId && !User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Soft delete customer + its (non-deleted) details
            customer.Deleted = true;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedBy = userId;

            if (customer.Details != null && customer.Details.Any())
            {
                foreach (var detail in customer.Details.Where(d => !d.Deleted))
                {
                    detail.Deleted = true;
                    detail.UpdatedAt = DateTime.UtcNow;
                    detail.UpdatedBy = userId;
                }
            }

            _context.SaveChanges();

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("MyData");
        }

    }
}