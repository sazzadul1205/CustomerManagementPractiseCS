using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Services.Interfaces;
using CustomerManagementPractiseCS.ViewModels;

namespace CustomerManagementPractiseCS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IImageService _imageService;

        public UserManagementController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            IImageService imageService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _imageService = imageService;
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
                ProfileCount = _context.Persons.Count(p => p.UserId == user.Id)
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

            // Get the profile
            var profiles = _context.Persons.Where(p => p.UserId == user.Id).ToList();

            if (profiles.Count > 0)
            {
                foreach (var profile in profiles)
                {
                    // Delete the Connected Errors First 
                    var addresses = _context.Addresses.Where(x => x.PersonId == profile.Id).ToList();
                    var contacts = _context.Contacts.Where(x => x.PersonId == profile.Id).ToList();
                    var educations = _context.Educations.Where(x => x.PersonId == profile.Id).ToList();
                    var experiences = _context.Experiences.Where(x => x.PersonId == profile.Id).ToList();
                    var socialLinks = _context.SocialLinks.Where(x => x.PersonId == profile.Id).ToList();

                    _context.Addresses.RemoveRange(addresses);
                    _context.Contacts.RemoveRange(contacts);
                    _context.Educations.RemoveRange(educations);
                    _context.Experiences.RemoveRange(experiences);
                    _context.SocialLinks.RemoveRange(socialLinks);

                    // Delete the profile image
                    _imageService.DeleteImage(profile.PhotoUrl);

                    // Now remove the profile itself
                    _context.Persons.Remove(profile);
                }
                _context.SaveChanges();
            }

            await _userManager.DeleteAsync(user);

            TempData["Success"] = "User deleted successfully.";

            return RedirectToAction("Index");
        }
    }
}