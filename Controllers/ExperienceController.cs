using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using CustomerManagementPractiseCS.ViewModels.ExperienceViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagementPractiseCS.Controllers
{
    [Authorize]
    public class ExperienceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ExperienceController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Experience/Create
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

            if (profileData == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            return View();
        }

        // POST: /Experience/Create
        [HttpPost]
        public IActionResult Create(ExperienceCreateViewModel ViewModel)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

            if (profileData == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            if (!ModelState.IsValid)
            {
                return View(ViewModel);
            }

            var experienceData = new Experience
            {
                PersonId = profileData.Id,
                CompanyName = ViewModel.CompanyName,
                Designation = ViewModel.Designation,
                Department = ViewModel.Department,
                EmploymentType = ViewModel.EmploymentType,
                Location = ViewModel.Location,
                StartDate = ViewModel.StartDate,
                EndDate = ViewModel.IsCurrent ? null : ViewModel.EndDate,
                IsCurrent = ViewModel.IsCurrent,
                Responsibilities = ViewModel.Responsibilities,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Experiences.Add(experienceData);
            _context.SaveChanges();

            return RedirectToAction("Index", "Profile");
        }

        // GET: /Experience/Edit/5
        public IActionResult Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

            if (profileData == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            // Get the Experience that Belongs to the Current Profile
            var experienceData = _context.Experiences.FirstOrDefault(x => x.Id == id && x.PersonId == profileData.Id);

            if (experienceData == null)
            {
                return NotFound();
            }

            var viewModel = new ExperienceEditViewModel
            {
                Id = experienceData.Id,
                CompanyName = experienceData.CompanyName,
                Designation = experienceData.Designation,
                Department = experienceData.Department,
                EmploymentType = experienceData.EmploymentType,
                Location = experienceData.Location,
                StartDate = experienceData.StartDate,
                EndDate = experienceData.EndDate,
                IsCurrent = experienceData.IsCurrent,
                Responsibilities = experienceData.Responsibilities
            };

            return View(viewModel);
        }

        // POST: /Experience/Edit/5
        [HttpPost]
        public IActionResult Edit(ExperienceEditViewModel ViewModel)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

            if (profileData == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            if (!ModelState.IsValid)
            {
                return View(ViewModel);
            }

            // Get the Experience that Belongs to the Current Profile
            var experienceData = _context.Experiences.FirstOrDefault(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (experienceData == null)
            {
                return NotFound();
            }

            experienceData.CompanyName = ViewModel.CompanyName;
            experienceData.Designation = ViewModel.Designation;
            experienceData.Department = ViewModel.Department;
            experienceData.EmploymentType = ViewModel.EmploymentType;
            experienceData.Location = ViewModel.Location;
            experienceData.StartDate = ViewModel.StartDate;
            experienceData.EndDate = ViewModel.IsCurrent ? null : ViewModel.EndDate;
            experienceData.IsCurrent = ViewModel.IsCurrent;
            experienceData.Responsibilities = ViewModel.Responsibilities;
            experienceData.UpdatedAt = DateTime.UtcNow;
            experienceData.UpdatedBy = userId;

            _context.SaveChanges();

            return RedirectToAction("Index", "Profile");
        }

        // GET: /Experience/Delete/5
        public IActionResult Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

            if (profileData == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            // Get the Experience that Belongs to the Current Profile
            var experienceData = _context.Experiences.FirstOrDefault(x => x.Id == id && x.PersonId == profileData.Id);

            if (experienceData == null)
            {
                return NotFound();
            }

            var viewModel = new ExperienceDeleteViewModel
            {
                Id = experienceData.Id,
                CompanyName = experienceData.CompanyName,
                Designation = experienceData.Designation,
                Department = experienceData.Department,
                EmploymentType = experienceData.EmploymentType,
                Location = experienceData.Location,
                StartDate = experienceData.StartDate,
                EndDate = experienceData.EndDate,
                IsCurrent = experienceData.IsCurrent,
                Responsibilities = experienceData.Responsibilities
            };

            return View(viewModel);
        }

        // POST: /Experience/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

            if (profileData == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            // Get the Experience that Belongs to the Current Profile
            var experienceData = _context.Experiences.FirstOrDefault(x => x.Id == id && x.PersonId == profileData.Id);

            if (experienceData == null)
            {
                return NotFound();
            }

            _context.Experiences.Remove(experienceData);
            _context.SaveChanges();

            return RedirectToAction("Index", "Profile");
        }
    }
}