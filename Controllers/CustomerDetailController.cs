using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using CustomerManagementPractiseCS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagementPractiseCS.Controllers
{
    // Only Logged In Users May Touch Customer Details
    [Authorize]
    public class CustomerDetailController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; // This Alllowes me to Work with Files and paths 
        private readonly UserManager<IdentityUser> _userManager;

        // The Only Image Types we Allow to be Stored on the Server
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        // DI Injection
        public CustomerDetailController(AppDbContext context, IWebHostEnvironment env, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        // The Id of the Current User
        private string? CurrentUserId => _userManager.GetUserId(User);

        // Load a Customer by Id (null when it does Not Exist)
        private Customer? GetCustomer(int customerId)
        {
            return _context.Customers.FirstOrDefault(x => x.Id == customerId);
        }

        // A Customer can Only be Managed by the User that Owns it
        private bool CanManage(Customer? customer)
        {
            return customer != null && customer.UserId == CurrentUserId;
        }

        // The File Name Must Have an Image Extension (the "accept" attribute is Client Side Only)
        private static bool IsAllowedImage(string fileName)
        {
            return AllowedImageExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());
        }

        // GET: CustomerDetail/Create?customerId={id}
        public IActionResult Create(int customerId)
        {
            // Get the Customer With the CustomerId
            var customer = GetCustomer(customerId);

            if (customer == null)
            {
                return NotFound();
            }

            // A Customer Detail can Only be Added by the Owner of the Customer
            if (!CanManage(customer))
            {
                return Forbid();
            }

            // Pre-fill the CustomerId so the form knows which customer this belongs to
            var detail = new CustomerDetailsViewModel
            {
                CustomerId = customerId
            };

            return View(detail);
        }

        // POST: CustomerDetail/Create
        [HttpPost]
        public IActionResult Create(CustomerDetailsViewModel customerDetailVM, IFormFile? imageFile)
        {
            // chek if teh Customer is Valid
            var customer = GetCustomer(customerDetailVM.CustomerId);

            if (customer == null)
            {
                return NotFound();
            }

            // The Customer Detail can Only be Added by the Owner of the Customer
            if (!CanManage(customer))
            {
                return Forbid();
            }

            // Reject any Upload that is Not a Real Image File
            if (imageFile != null && imageFile.Length > 0 && !IsAllowedImage(imageFile.FileName))
            {
                ModelState.AddModelError("ProfileImage", "Only image files (.jpg, .jpeg, .png, .gif, .webp) are allowed.");
            }

            if (ModelState.IsValid)
            {
                // Map ViewModel → Entity
                var customerDetail = new CustomerDetail
                {
                    CustomerId = customerDetailVM.CustomerId,
                    Phone = customerDetailVM.Phone,
                    Email = customerDetailVM.Email,
                    DateOfBirth = customerDetailVM.DateOfBirth,
                    City = customerDetailVM.City,
                    Country = customerDetailVM.Country,
                    Address = customerDetailVM.Address,
                    IsActive = customerDetailVM.IsActive
                };

                // Save uploaded image, if any, and attach its path to the entity
                string? savedPath = SaveImage(imageFile);
                if (savedPath != null)
                {
                    customerDetail.ProfileImage = savedPath;
                }

                // If the new data is Checked as active 
                if (customerDetail.IsActive)
                {
                    // Get all the Customer Details
                    var others = _context.CustomersDetail.Where(x => x.CustomerId == customerDetail.CustomerId).ToList();

                    // Go Through each and Deactivate all
                    foreach (var other in others)
                    {
                        other.IsActive = false;
                    }
                }

                _context.CustomersDetail.Add(customerDetail);
                _context.SaveChanges();

                // Redirecting to the Customers controlle Details page, using the id of the customer this detail belongs to
                return RedirectToAction("Details", "Customers", new { id = customerDetail.CustomerId });
            }

            // Validation failed
            return View(customerDetailVM);
        }


        // GET: CustomerDetail/Edit/5
        public IActionResult Edit(int id)
        {
            var customerDetail = _context.CustomersDetail.FirstOrDefault(x => x.Id == id);

            if (customerDetail == null)
            {
                return NotFound();
            }

            // Only the Owner of the Customer can Edit its Details
            if (!CanManage(GetCustomer(customerDetail.CustomerId)))
            {
                return Forbid();
            }

            // Map Entity → ViewModel
            var vm = new CustomerDetailsViewModel
            {
                Id = customerDetail.Id,
                CustomerId = customerDetail.CustomerId,
                Phone = customerDetail.Phone,
                Email = customerDetail.Email,
                ProfileImage = customerDetail.ProfileImage,
                DateOfBirth = customerDetail.DateOfBirth,
                City = customerDetail.City,
                Country = customerDetail.Country,
                Address = customerDetail.Address,
                IsActive = customerDetail.IsActive
            };

            return View(vm);
        }

        // POST: CustomerDetail/Edit/5
        [HttpPost]
        public IActionResult Edit(int id, CustomerDetailsViewModel customerDetailVM, IFormFile? imageFile)
        {
            // Route id must match the id on the submitted model
            if (id != customerDetailVM.Id)
            {
                return NotFound();
            }

            // Get the Exsisting Details
            var existing = _context.CustomersDetail.FirstOrDefault(x => x.Id == id);

            if (existing == null)
            {
                return NotFound();
            }

            // Only the Owner of the Customer can Edit its Details
            if (!CanManage(GetCustomer(existing.CustomerId)))
            {
                return Forbid();
            }

            // Reject any Upload that is Not a Real Image File
            if (imageFile != null && imageFile.Length > 0 && !IsAllowedImage(imageFile.FileName))
            {
                ModelState.AddModelError("ProfileImage", "Only image files (.jpg, .jpeg, .png, .gif, .webp) are allowed.");
            }

            if (ModelState.IsValid)
            {

                // If the new data is Checked as active 
                if (customerDetailVM.IsActive)
                {
                    // Get all the Customer Details But exclude the Current one 
                    // Use the Stored CustomerId and Not the Posted one so it can not be Tampered With
                    var others = _context.CustomersDetail.Where(x => x.CustomerId == existing.CustomerId && x.Id != id).ToList();

                    // Go Through each and Deactivate all
                    foreach (var other in others)
                    {
                        other.IsActive = false;
                    }
                }

                // Handle new image upload
                string? newPath = SaveImage(imageFile);

                // 
                if (newPath != null)
                {
                    // Delete old Image Location
                    if (!string.IsNullOrEmpty(existing.ProfileImage))
                    {
                        // Get the Old image Location
                        string oldFile = Path.Combine(_env.WebRootPath, existing.ProfileImage.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                        // Delete Location
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }
                    }

                    existing.ProfileImage = newPath;
                }



                existing.Phone = customerDetailVM.Phone;
                existing.Email = customerDetailVM.Email;
                existing.DateOfBirth = customerDetailVM.DateOfBirth;
                existing.City = customerDetailVM.City;
                existing.Country = customerDetailVM.Country;
                existing.Address = customerDetailVM.Address;
                existing.IsActive = customerDetailVM.IsActive;

                _context.SaveChanges();

                return RedirectToAction("Details", "Customers", new { id = existing.CustomerId });
            }

            // Validation failed 
            return View(customerDetailVM);
        }


        // POST: CustomerDetail/SetActive/5
        [HttpPost]
        public IActionResult SetActive(int id)
        {
            var customerDetail = _context.CustomersDetail.FirstOrDefault(x => x.Id == id);

            if (customerDetail == null)
            {
                return NotFound();
            }

            // Only the Owner of the Customer can Change the Active Detail
            if (!CanManage(GetCustomer(customerDetail.CustomerId)))
            {
                return Forbid();
            }

            var otherDetails = _context.CustomersDetail.Where(x => x.CustomerId == customerDetail.CustomerId && x.Id != id).ToList();

            foreach (var other in otherDetails)
            {
                other.IsActive = false;
            }

            customerDetail.IsActive = true;

            _context.SaveChanges();

            return RedirectToAction("Details", "Customers", new { id = customerDetail.CustomerId });
        }

        // GET: CustomerDetail/Delete/5
        public IActionResult Delete(int id)
        {
            var customerDetail = _context.CustomersDetail.FirstOrDefault(x => x.Id == id);

            if (customerDetail == null)
            {
                return NotFound();
            }

            // Only the Owner of the Customer can Delete its Details
            if (!CanManage(GetCustomer(customerDetail.CustomerId)))
            {
                return Forbid();
            }

            // Map Entity → ViewModel
            var vm = new CustomerDetailsViewModel
            {
                Id = customerDetail.Id,
                CustomerId = customerDetail.CustomerId,
                Phone = customerDetail.Phone,
                Email = customerDetail.Email,
                ProfileImage = customerDetail.ProfileImage,
                DateOfBirth = customerDetail.DateOfBirth,
                City = customerDetail.City,
                Country = customerDetail.Country,
                Address = customerDetail.Address,
                IsActive = customerDetail.IsActive
            };

            return View(vm);
        }

        // POST: CustomerDetail/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var customerDetail = _context.CustomersDetail.FirstOrDefault(x => x.Id == id);
            if (customerDetail == null)
            {
                return NotFound();
            }

            // Only the Owner of the Customer can Delete its Details
            if (!CanManage(GetCustomer(customerDetail.CustomerId)))
            {
                return Forbid();
            }

            // Get the Customer Id
            int customerId = customerDetail.CustomerId;

            // DElete Imaeg 
            if (!string.IsNullOrEmpty(customerDetail.ProfileImage))
            {
                string filePath = Path.Combine(_env.WebRootPath, customerDetail.ProfileImage.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.CustomersDetail.Remove(customerDetail);

            _context.SaveChanges();

            return RedirectToAction("Details", "Customers", new { id = customerId });
        }

        // Image Upload Helper
        private string? SaveImage(IFormFile? imageFile)
        {
            // No file was uploaded — nothing to save
            if (imageFile == null || imageFile.Length == 0)
            {
                return null;
            }

            // Extra Safety : Only Real Image Files may reach the web root
            if (!IsAllowedImage(imageFile.FileName))
            {
                return null;
            }

            // Ensure the uploads folder exists under wwwroot
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");

            Directory.CreateDirectory(uploadsFolder);

            // Generate a unique filename to avoid collisions/overwrites
            string extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            string uniqueName = Guid.NewGuid().ToString() + extension;
            string filePath = Path.Combine(uploadsFolder, uniqueName);

            // Write the uploaded file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            // Return the relative URL used to display the image
            return "/uploads/" + uniqueName;
        }
    }
}