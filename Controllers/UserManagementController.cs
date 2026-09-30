using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.ViewModels;

namespace CustomerManagementPractiseCS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public UserManagementController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: UserManagement
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // GET: UserManagement/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Self Account Delete Protection
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                return BadRequest("You can't delete your own account here.");
            }

            // Build a ViewModel for the confirmation page
            var ViewModel = new UserDeleteViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Roles = await _userManager.GetRolesAsync(user),
                CustomerCount = _context.Customers.Count(c => c.UserId == user.Id)
            };

            return View(ViewModel);
        }

        // POST: UserManagement/Delete/{id}
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Self Account Delete Protection
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                return BadRequest("You can't delete your own account here.");
            }

            // Get Every Customer of this User with the Details Attached
            var customers = _context.Customers.Include(c => c.Details).Where(c => c.UserId == user.Id).ToList();

            // Check if there is any Customer
            if (customers.Count > 0)
            {
                // One by One Go through every Customer and Delete it With its Details
                foreach (var customer in customers)
                {
                    // Check if there is any Details
                    if (customer.Details != null && customer.Details.Any())
                    {
                        // One by One Go through all the Details and Delete
                        foreach (var detail in customer.Details)
                        {
                            _context.CustomersDetail.Remove(detail);
                        }
                    }
                    // Remove the Customer itself
                    _context.Customers.Remove(customer);
                }
                _context.SaveChanges();
            }

            // Finally Delete the Identity User
            await _userManager.DeleteAsync(user);

            return RedirectToAction("Index");
        }
    }
}