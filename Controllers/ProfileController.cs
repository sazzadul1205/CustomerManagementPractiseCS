using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using CustomerManagementPractiseCS.Services.Interfaces;
using CustomerManagementPractiseCS.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            // Send admins to the admin panel
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

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

                Addresses = _context.Addresses.Where(x => x.PersonId == profileData.Id).ToList(),

                Contacts = _context.Contacts.Where(x => x.PersonId == profileData.Id).ToList(),

                Educations = _context.Educations.Where(x => x.PersonId == profileData.Id).ToList(),

                Experiences = _context.Experiences.Where(x => x.PersonId == profileData.Id).ToList(),

                SocialLinks = _context.SocialLinks.Where(x => x.PersonId == profileData.Id).ToList()
            };

            _completenessService.Fill(viewModel);

            return View(viewModel);
        }

        // GET: /Profile/Create
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);

            // Check if i already have a profile,
            bool hasProfile = _context.Persons.Any(x => x.UserId == userId);
            if (hasProfile)
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        // POST: /Profile/Create
        [HttpPost]
        public IActionResult Create(ProfileCreateViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(ViewModel);
            }

            var userId = _userManager.GetUserId(User);

            bool hasProfile = _context.Persons.Any(x => x.UserId == userId);
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
            _context.SaveChanges();

            TempData["Success"] = "Profile created successfully.";

            return RedirectToAction("Index", "Profile");
        }

        // GET: /Profile/Edit
        public IActionResult Edit()
        {
            // Get the Current User Id
            var userId = _userManager.GetUserId(User);

            // Get the Existing Profile Data
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

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
        public IActionResult Edit(ProfileEditViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(ViewModel);
            }

            var userId = _userManager.GetUserId(User);

            // The ID must match AND it must belong to me
            // Find the First Profile where the Id matches the view model id and also match it with userid from the logged in user to see if all is right 
            var profileData = _context.Persons.FirstOrDefault(x => x.Id == ViewModel.Id && x.UserId == userId);
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

            _context.SaveChanges();

            TempData["Success"] = "Profile updated successfully.";

            return RedirectToAction("Index", "Profile");
        }

        // GET: /Profile/Delete
        public IActionResult Delete()
        {
            // Get the Current User Id
            var userId = _userManager.GetUserId(User);

            // Get the Existing Profile Data
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

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
        public IActionResult DeleteConfirmed(int id)
        {
            // Get the Current User Id
            var userId = _userManager.GetUserId(User);

            // Get the Existing Profile Data Also Check the Ownership 
            var profileData = _context.Persons.FirstOrDefault(x => x.Id == id && x.UserId == userId);

            if (profileData == null)
            {
                return NotFound();
            }


            // Soft Delete
            //profileData.Deleted = true;
            //profileData.UpdatedAt = DateTime.UtcNow;
            //profileData.UpdatedBy = userId;

            // Connected 
            var addresses = _context.Addresses.Where(x => x.PersonId == profileData.Id).ToList();
            var contacts = _context.Contacts.Where(x => x.PersonId == profileData.Id).ToList();
            var educations = _context.Educations.Where(x => x.PersonId == profileData.Id).ToList();
            var experiences = _context.Experiences.Where(x => x.PersonId == profileData.Id).ToList();
            var socialLinks = _context.SocialLinks.Where(x => x.PersonId == profileData.Id).ToList();

            // RemoveRange is used primarily for a list of objects so we do not have to parse one by one and delete    
            _context.Contacts.RemoveRange(contacts);
            _context.Experiences.RemoveRange(experiences);
            _context.Educations.RemoveRange(educations);
            _context.Addresses.RemoveRange(addresses);
            _context.SocialLinks.RemoveRange(socialLinks);

            _imageService.DeleteImage(profileData.PhotoUrl);
            _context.Persons.Remove(profileData);

            _context.SaveChanges();

            TempData["Success"] = "Profile deleted successfully.";

            return RedirectToAction("Index", "Profile");
        }

        // GET: /Profile/Cv/5
        [AllowAnonymous]
        public IActionResult Cv(int id)
        {
            var profileData = _context.Persons.FirstOrDefault(x => x.Id == id);

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

                Addresses = _context.Addresses.Where(x => x.PersonId == profileData.Id).ToList(),
                Contacts = _context.Contacts.Where(x => x.PersonId == profileData.Id).ToList(),
                Educations = _context.Educations.Where(x => x.PersonId == profileData.Id).OrderByDescending(x => x.StartYear).ToList(),
                Experiences = _context.Experiences.Where(x => x.PersonId == profileData.Id).OrderByDescending(x => x.StartDate).ToList(),
                SocialLinks = _context.SocialLinks.Where(x => x.PersonId == profileData.Id).ToList()
            };

            return View(viewModel);
        }
    }

}