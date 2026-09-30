using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using CustomerManagementPractiseCS.ViewModels.EducationViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagementPractiseCS.Controllers
{
    [Authorize]
    public class EducationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EducationController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Education/Create
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

        // POST: /Education/Create
        [HttpPost]
        public IActionResult Create(EducationCreateViewModel ViewModel)
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

            var educationData = new Education
            {
                PersonId = profileData.Id,
                DegreeLevel = ViewModel.DegreeLevel,
                DegreeName = ViewModel.DegreeName,
                InstitutionName = ViewModel.InstitutionName,
                BoardOrUniversity = ViewModel.BoardOrUniversity,
                StartYear = ViewModel.StartYear,
                EndYear = ViewModel.IsOngoing ? null : ViewModel.EndYear,
                IsOngoing = ViewModel.IsOngoing,
                Result = ViewModel.Result,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Educations.Add(educationData);
            _context.SaveChanges();

            return RedirectToAction("Index", "Profile");
        }

        // GET: /Education/Edit/5
        public IActionResult Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

            if (profileData == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            // Get the Education that Belongs to the Current Profile
            var educationData = _context.Educations
                .FirstOrDefault(x => x.Id == id && x.PersonId == profileData.Id);

            if (educationData == null)
            {
                return NotFound();
            }

            var viewModel = new EducationEditViewModel
            {
                Id = educationData.Id,
                DegreeLevel = educationData.DegreeLevel,
                DegreeName = educationData.DegreeName,
                InstitutionName = educationData.InstitutionName,
                BoardOrUniversity = educationData.BoardOrUniversity,
                StartYear = educationData.StartYear,
                EndYear = educationData.EndYear,
                IsOngoing = educationData.IsOngoing,
                Result = educationData.Result
            };

            return View(viewModel);
        }

        // POST: /Education/Edit/5
        [HttpPost]
        public IActionResult Edit(EducationEditViewModel ViewModel)
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

            // Get the Education that Belongs to the Current Profile
            var educationData = _context.Educations
                .FirstOrDefault(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (educationData == null)
            {
                return NotFound();
            }

            educationData.DegreeLevel = ViewModel.DegreeLevel;
            educationData.DegreeName = ViewModel.DegreeName;
            educationData.InstitutionName = ViewModel.InstitutionName;
            educationData.BoardOrUniversity = ViewModel.BoardOrUniversity;
            educationData.StartYear = ViewModel.StartYear;
            educationData.EndYear = ViewModel.IsOngoing ? null : ViewModel.EndYear;
            educationData.IsOngoing = ViewModel.IsOngoing;
            educationData.UpdatedAt = DateTime.UtcNow;
            educationData.UpdatedBy = userId;

            _context.SaveChanges();

            return RedirectToAction("Index", "Profile");
        }

        // GET: /Education/Delete/5
        public IActionResult Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

            if (profileData == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            // Get the Education that Belongs to the Current Profile
            var educationData = _context.Educations
                .FirstOrDefault(x => x.Id == id && x.PersonId == profileData.Id);

            if (educationData == null)
            {
                return NotFound();
            }

            var viewModel = new EducationDeleteViewModel
            {
                Id = educationData.Id,
                DegreeLevel = educationData.DegreeLevel,
                DegreeName = educationData.DegreeName,
                InstitutionName = educationData.InstitutionName,
                BoardOrUniversity = educationData.BoardOrUniversity,
                StartYear = educationData.StartYear,
                EndYear = educationData.EndYear,
                IsOngoing = educationData.IsOngoing,
                Result = educationData.Result
            };

            return View(viewModel);
        }

        // POST: /Education/Delete/5
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

            // Get the Education that Belongs to the Current Profile
            var educationData = _context.Educations
                .FirstOrDefault(x => x.Id == id && x.PersonId == profileData.Id);

            if (educationData == null)
            {
                return NotFound();
            }

            _context.Educations.Remove(educationData);
            _context.SaveChanges();

            return RedirectToAction("Index", "Profile");
        }
    }
}