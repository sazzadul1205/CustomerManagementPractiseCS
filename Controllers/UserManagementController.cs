using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Services.Interfaces;
using CustomerManagementPractiseCS.ViewModels;
using CustomerManagementPractiseCS.ViewModels.UserManagementViewModels;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Index()
        {
            var users = await BuildUserListAsync();
            return View(users);
        }

        // GET: UserManagement/List
        // The page reloads the table from here after a delete, so it returns just the table
        public async Task<IActionResult> List()
        {
            var users = await BuildUserListAsync();
            return PartialView("_UserList", users);
        }

        // Builds the rows for the users table: email, roles and how many profiles each user has
        private async Task<List<UserListItemViewModel>> BuildUserListAsync()
        {
            var currentUserId = _userManager.GetUserId(User);

            // This list is only displayed, so nothing is changed and nothing needs tracking
            var users = await _userManager.Users
                .AsNoTracking()
                .OrderBy(x => x.Email)
                .ToListAsync();

            var list = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                list.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Roles = await _userManager.GetRolesAsync(user),
                    ProfileCount = await _context.Persons.CountAsync(p => p.UserId == user.Id),
                    IsCurrentUser = user.Id == currentUserId
                });
            }

            return list;
        }

        // GET: UserManagement/Delete/{id}
        // Returns the body of the confirmation modal, nothing is deleted yet
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

            // Build a ViewModel for the confirmation window
            var ViewModel = new UserDeleteViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Roles = await _userManager.GetRolesAsync(user),
                ProfileCount = await _context.Persons.CountAsync(p => p.UserId == user.Id)
            };

            return PartialView("_UserDeleteForm", ViewModel);
        }

        // POST: UserManagement/Delete/{id}
        // This only runs after the confirmation modal was accepted
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
            var profiles = await _context.Persons.Where(p => p.UserId == user.Id).ToListAsync();

            if (profiles.Count > 0)
            {
                foreach (var profile in profiles)
                {
                    // Delete the Connected Errors First 
                    var addresses = await _context.Addresses.Where(x => x.PersonId == profile.Id).ToListAsync();
                    var contacts = await _context.Contacts.Where(x => x.PersonId == profile.Id).ToListAsync();
                    var educations = await _context.Educations.Where(x => x.PersonId == profile.Id).ToListAsync();
                    var experiences = await _context.Experiences.Where(x => x.PersonId == profile.Id).ToListAsync();
                    var socialLinks = await _context.SocialLinks.Where(x => x.PersonId == profile.Id).ToListAsync();

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
                await _context.SaveChangesAsync();
            }

            await _userManager.DeleteAsync(user);

            return Ok(new { message = "User deleted successfully." });
        }
    }
}
