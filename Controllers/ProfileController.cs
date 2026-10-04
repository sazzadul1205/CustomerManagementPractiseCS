using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using CustomerManagementPractiseCS.Services.Interfaces;
using CustomerManagementPractiseCS.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementPractiseCS.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IProfileCompletenessService _completenessService;

        public ProfileController(
            AppDbContext context,
            UserManager<IdentityUser> userManager,
            IImageService imageService,
            IProfileCompletenessService completenessService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _completenessService = completenessService;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // Send admins to the admin panel
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            var profileData = await _context.Persons
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (profileData == null)
            {
                return View();
            }

            var viewModel = new ProfileIndexViewModel
            {
                Id = profileData.Id,
                FullName = profileData.FullName,
                Gender = profileData.Gender,
                DateOfBirth = profileData.DateOfBirth,
                Religion = profileData.Religion,
                BloodGroup = profileData.BloodGroup,
                PhotoUrl = profileData.PhotoUrl,
                Summary = profileData.Summary,

                Addresses = await _context.Addresses.AsNoTracking().Where(x => x.PersonId == profileData.Id).ToListAsync(),

                Contacts = await _context.Contacts.AsNoTracking().Where(x => x.PersonId == profileData.Id).ToListAsync(),

                Educations = await _context.Educations
                    .AsNoTracking()
                    .Where(x => x.PersonId == profileData.Id)
                    .OrderByDescending(x => x.StartYear)
                    .ToListAsync(),

                Experiences = await _context.Experiences
                    .AsNoTracking()
                    .Where(x => x.PersonId == profileData.Id)
                    .OrderByDescending(x => x.StartDate)
                    .ToListAsync(),

                SocialLinks = await _context.SocialLinks.AsNoTracking().Where(x => x.PersonId == profileData.Id).ToListAsync()
            };

            _completenessService.Fill(viewModel);

            return View(viewModel);
        }

        // GET: /Profile/Create
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);

            // Check if i already have a profile,
            bool hasProfile = await _context.Persons.AnyAsync(x => x.UserId == userId);
            if (hasProfile)
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        // POST: /Profile/Create
        [HttpPost]
        public async Task<IActionResult> Create(ProfileCreateViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(ViewModel);
            }

            var userId = _userManager.GetUserId(User);

            bool hasProfile = await _context.Persons.AnyAsync(x => x.UserId == userId);
            if (hasProfile)
            {
                return RedirectToAction("Index");
            }

            var profileData = new Person
            {
                UserId = userId,
                FullName = ViewModel.FullName,
                Gender = ViewModel.Gender,
                DateOfBirth = ViewModel.DateOfBirth,
                Religion = ViewModel.Religion,
                BloodGroup = ViewModel.BloodGroup,
                Summary = ViewModel.Summary,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                Deleted = false
            };

            // Save the photo, if any, and store the path on the entity
            string? photoPath = _imageService.SaveImage(ViewModel.PhotoFile, "profiles");
            if (photoPath != null)
            {
                profileData.PhotoUrl = photoPath;
            }

            _context.Persons.Add(profileData);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile created successfully.";

            return RedirectToAction("Index", "Profile");
        }

        // GET: /Profile/Edit
        public async Task<IActionResult> Edit()
        {
            // Get the Current User Id
            var userId = _userManager.GetUserId(User);

            // Get the Existing Profile Data
            var profileData = await _context.Persons.FirstOrDefaultAsync(x => x.UserId == userId);

            if (profileData == null)
            {
                return NotFound();
            }

            var viewModel = new ProfileEditViewModel
            {
                Id = profileData.Id,
                FullName = profileData.FullName,
                Gender = profileData.Gender,
                DateOfBirth = profileData.DateOfBirth,
                Religion = profileData.Religion,
                BloodGroup = profileData.BloodGroup,
                Summary = profileData.Summary,

                CurrentPhotoUrl = profileData.PhotoUrl
            };

            return View(viewModel);
        }

        // POST: /Profile/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(ProfileEditViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(ViewModel);
            }

            var userId = _userManager.GetUserId(User);

            // The ID must match AND it must belong to me
            // Find the First Profile where the Id matches the view model id and also match it with userid from the logged in user to see if all is right 
            var profileData = await _context.Persons.FirstOrDefaultAsync(x => x.Id == ViewModel.Id && x.UserId == userId);
            if (profileData == null)
            {
                return NotFound();
            }

            profileData.FullName = ViewModel.FullName;
            profileData.Gender = ViewModel.Gender;
            profileData.DateOfBirth = ViewModel.DateOfBirth;
            profileData.Religion = ViewModel.Religion;
            profileData.BloodGroup = ViewModel.BloodGroup;
            profileData.Summary = ViewModel.Summary;
            profileData.UpdatedAt = DateTime.UtcNow;
            profileData.UpdatedBy = userId;

            // If a new file was uploaded, replace the old one
            string? newPhotoPath = _imageService.SaveImage(ViewModel.PhotoFile, "profiles");
            if (newPhotoPath != null)
            {
                _imageService.DeleteImage(profileData.PhotoUrl);
                profileData.PhotoUrl = newPhotoPath;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully.";

            return RedirectToAction("Index", "Profile");
        }

        // GET: /Profile/Delete
        public async Task<IActionResult> Delete()
        {
            // Get the Current User Id
            var userId = _userManager.GetUserId(User);

            // Get the Existing Profile Data
            var profileData = await _context.Persons.FirstOrDefaultAsync(x => x.UserId == userId);

            if (profileData == null)
            {
                return NotFound();
            }

            var viewModel = new ProfileDeleteViewModel
            {
                Id = profileData.Id,
                FullName = profileData.FullName,
                Gender = profileData.Gender,
                DateOfBirth = profileData.DateOfBirth,
                Religion = profileData.Religion,
                BloodGroup = profileData.BloodGroup,
                PhotoUrl = profileData.PhotoUrl,
                Summary = profileData.Summary
            };

            return View(viewModel);
        }

        // POST: /Profile/Delete
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Get the Current User Id
            var userId = _userManager.GetUserId(User);

            // Get the Existing Profile Data Also Check the Ownership 
            var profileData = await _context.Persons.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (profileData == null)
            {
                return NotFound();
            }


            // Soft Delete
            //profileData.Deleted = true;
            //profileData.UpdatedAt = DateTime.UtcNow;
            //profileData.UpdatedBy = userId;

            // Connected 
            var addresses = await _context.Addresses.Where(x => x.PersonId == profileData.Id).ToListAsync();
            var contacts = await _context.Contacts.Where(x => x.PersonId == profileData.Id).ToListAsync();
            var educations = await _context.Educations.Where(x => x.PersonId == profileData.Id).ToListAsync();
            var experiences = await _context.Experiences.Where(x => x.PersonId == profileData.Id).ToListAsync();
            var socialLinks = await _context.SocialLinks.Where(x => x.PersonId == profileData.Id).ToListAsync();

            // RemoveRange is used primarily for a list of objects so we do not have to parse one by one and delete    
            _context.Contacts.RemoveRange(contacts);
            _context.Experiences.RemoveRange(experiences);
            _context.Educations.RemoveRange(educations);
            _context.Addresses.RemoveRange(addresses);
            _context.SocialLinks.RemoveRange(socialLinks);

            _imageService.DeleteImage(profileData.PhotoUrl);
            _context.Persons.Remove(profileData);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile deleted successfully.";

            return RedirectToAction("Index", "Profile");
        }

        // GET: /Profile/Cv/5
        [AllowAnonymous]
        public async Task<IActionResult> Cv(int id)
        {
            var profileData = await _context.Persons
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (profileData == null)
            {
                return NotFound();
            }

            var viewModel = new ProfileCvViewModel
            {
                Id = profileData.Id,
                FullName = profileData.FullName,
                Gender = profileData.Gender,
                DateOfBirth = profileData.DateOfBirth,
                Religion = profileData.Religion,
                BloodGroup = profileData.BloodGroup,
                PhotoUrl = profileData.PhotoUrl,
                Summary = profileData.Summary,

                Addresses = await _context.Addresses.AsNoTracking().Where(x => x.PersonId == profileData.Id).ToListAsync(),
                Contacts = await _context.Contacts.AsNoTracking().Where(x => x.PersonId == profileData.Id).ToListAsync(),
                Educations = await _context.Educations.AsNoTracking().Where(x => x.PersonId == profileData.Id).OrderByDescending(x => x.StartYear).ToListAsync(),
                Experiences = await _context.Experiences.AsNoTracking().Where(x => x.PersonId == profileData.Id).OrderByDescending(x => x.StartDate).ToListAsync(),
                SocialLinks = await _context.SocialLinks.AsNoTracking().Where(x => x.PersonId == profileData.Id).ToListAsync()
            };

            return View(viewModel);
        }
    }

}
