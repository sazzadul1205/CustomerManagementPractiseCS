using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagementPractiseCS.Controllers
{
    public class CustomerDetailController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; // This Alllowes me to Work with Files and paths 

        // DI Injection
        public CustomerDetailController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: CustomerDetail/Create?customerId={id}
        public IActionResult Create(int customerId)
        {
            // Get all the Customer Details With the CustomerId
            var customer = _context.Customers.FirstOrDefault(x => x.Id == customerId);

            if (customer == null)
            {
                return NotFound();
            }

            // Pre-fill the CustomerId so the form knows which customer this belongs to
            var detail = new CustomerDetail
            {
                CustomerId = customerId
            };

            return View(detail);
        }

        // POST: CustomerDetail/Create
        [HttpPost]
        public IActionResult Create(CustomerDetail customerDetail, IFormFile? imageFile)
        {
            // chek if teh Customer is Valid
            if (!_context.Customers.Any(c => c.Id == customerDetail.CustomerId))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
            return View(customerDetail);
        }


        // GET: CustomerDetail/Edit/5
        public IActionResult Edit(int id)
        {
            var customerDetail = _context.CustomersDetail.FirstOrDefault(x => x.Id == id);

            if (customerDetail == null)
            {
                return NotFound();
            }

            return View(customerDetail);
        }

        // POST: CustomerDetail/Edit/5
        [HttpPost]
        public IActionResult Edit(int id, CustomerDetail customerDetail, IFormFile? imageFile)
        {
            // Route id must match the id on the submitted model
            if (id != customerDetail.Id) 
            { 
                return NotFound();
            }

            // Get the Exsisting Details
            var existing = _context.CustomersDetail.FirstOrDefault(x => x.Id == id);

            if (existing == null) 
            { 
                return NotFound(); 
            }

            if (ModelState.IsValid)
            {

                // If the new data is Checked as active 
                if (customerDetail.IsActive)
                {
                    // Get all the Customer Details But exclude the Current one 
                    var others = _context.CustomersDetail.Where(x => x.CustomerId == customerDetail.CustomerId && x.Id != id).ToList();

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
                        string oldFile = Path.Combine(_env.WebRootPath,existing.ProfileImage.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                        // Delete Location
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }
                    }

                    existing.ProfileImage = newPath;
                }



                existing.Phone = customerDetail.Phone;
                existing.Email = customerDetail.Email;
                existing.DateOfBirth = customerDetail.DateOfBirth;
                existing.City = customerDetail.City;
                existing.Country = customerDetail.Country;
                existing.Address = customerDetail.Address;
                existing.IsActive = customerDetail.IsActive;

                _context.SaveChanges();

                return RedirectToAction("Details", "Customers", new { id = existing.CustomerId });
            }

            // Validation failed 
            return View(customerDetail);
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

            return View(customerDetail);
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

            // Get the Customer Id
            int customerId = customerDetail.CustomerId;

            // DElete Imaeg 
            if (!string.IsNullOrEmpty(customerDetail.ProfileImage))
            {
                string filePath = Path.Combine(_env.WebRootPath,customerDetail.ProfileImage.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

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