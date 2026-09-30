using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
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

        public ProfileController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Profile
        public IActionResult Index()
        {
            // Get the Current User Id
            var userId = _userManager.GetUserId(User);

            // Get the First Matching Profile Data
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
                Summary = profileData.Summary
            };

            return View(viewModel);
        }

        // GET: /Profile/Create
        public IActionResult Create()
        {
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

            var profileData = new Person
            {
                UserId = userId,
                FullName = ViewModel.FullName,
                Gender = ViewModel.Gender,
                DateOfBirth = ViewModel.DateOfBirth,
                Religion = ViewModel.Religion,
                BloodGroup = ViewModel.BloodGroup,
                PhotoUrl = ViewModel.PhotoUrl,
                Summary = ViewModel.Summary,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                Deleted = false
            };

            _context.Persons.Add(profileData);
            _context.SaveChanges();

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
                PhotoUrl = profileData.PhotoUrl,
                Summary = profileData.Summary
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

            // Get the Current User Id
            var userId = _userManager.GetUserId(User);

            // Get the Existing Profile Data
            var profileData = _context.Persons.FirstOrDefault(x => x.Id == ViewModel.Id);

            if (profileData == null)
            {
                return NotFound();
            }

            profileData.FullName = ViewModel.FullName;
            profileData.Gender = ViewModel.Gender;
            profileData.DateOfBirth = ViewModel.DateOfBirth;
            profileData.Religion = ViewModel.Religion;
            profileData.BloodGroup = ViewModel.BloodGroup;
            profileData.PhotoUrl = ViewModel.PhotoUrl;
            profileData.Summary = ViewModel.Summary;
            profileData.UpdatedAt = DateTime.UtcNow;
            profileData.UpdatedBy = userId;

            _context.SaveChanges();

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

            // Get the Existing Profile Data
            var profileData = _context.Persons.FirstOrDefault(x => x.Id == id);

            if (profileData == null)
            {
                return NotFound();
            }


            // Soft Delete
            //profileData.Deleted = true;
            //profileData.UpdatedAt = DateTime.UtcNow;
            //profileData.UpdatedBy = userId;
            _context.Persons.Remove(profileData);

            _context.SaveChanges();

            return RedirectToAction("Index", "Profile");
        }
    }
}