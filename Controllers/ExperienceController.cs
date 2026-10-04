using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using CustomerManagementPractiseCS.ViewModels.ExperienceViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = await _context.Persons.FirstOrDefaultAsync(x => x.UserId == userId);

            if (profileData == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            return View();
        }

        // GET: /Experience/List
        public async Task<IActionResult> List()
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = await _context.Persons.FirstOrDefaultAsync(x => x.UserId == userId);

            // Here we return not found because this is for partial view
            if (profileData == null)
            {
                return NotFound();
            }

            // Get the Experiences that Belong to the Current Profile
            var experiences = await _context.Experiences
                .Where(x => x.PersonId == profileData.Id)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync();

            // Return the partial
            return PartialView("_ExperienceList", experiences);
        }

        // POST: /Experience/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ExperienceCreateViewModel ViewModel)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = await _context.Persons.FirstOrDefaultAsync(x => x.UserId == userId);

            if (profileData == null)
            {
                // return RedirectToAction("Index", "Profile");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // return View(ViewModel);
                return BadRequest(ModelState);
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
            await _context.SaveChangesAsync();

            // return RedirectToAction("Index", "Profile");
            return Ok(new { message = "Experience added successfully." });
        }

        // GET: /Experience/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = await _context.Persons.FirstOrDefaultAsync(x => x.UserId == userId);

            if (profileData == null)
            {
                // return RedirectToAction("Index", "Profile");
                return NotFound();
            }

            // Get the Experience that Belongs to the Current Profile
            var experienceData = await _context.Experiences.FirstOrDefaultAsync(x => x.Id == id && x.PersonId == profileData.Id);

            if (experienceData == null)
            {
                // return NotFound();
                return BadRequest("Experience not found.");
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

            // return View(viewModel);
            return PartialView("_ExperienceEditForm", viewModel);
        }

        // POST: /Experience/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ExperienceEditViewModel ViewModel)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = await _context.Persons.FirstOrDefaultAsync(x => x.UserId == userId);

            if (profileData == null)
            {
                // return RedirectToAction("Index", "Profile");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // return View(ViewModel);
                return BadRequest(ModelState);
            }

            // Get the Experience that Belongs to the Current Profile
            var experienceData = await _context.Experiences.FirstOrDefaultAsync(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (experienceData == null)
            {
                // return NotFound();
                return BadRequest("Experience not found.");
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

            await _context.SaveChangesAsync();

            // return RedirectToAction("Index", "Profile");
            return Ok(new { message = "Experience updated successfully." });
        }

        // GET: /Experience/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = await _context.Persons.FirstOrDefaultAsync(x => x.UserId == userId);

            if (profileData == null)
            {
                // return RedirectToAction("Index", "Profile");
                return NotFound();
            }

            // Get the Experience that Belongs to the Current Profile
            var experienceData = await _context.Experiences.FirstOrDefaultAsync(x => x.Id == id && x.PersonId == profileData.Id);

            if (experienceData == null)
            {
                // return NotFound();
                return BadRequest("Experience not found.");
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

            // return View(viewModel);
            return PartialView("_ExperienceDeleteForm", viewModel);
        }

        // POST: /Experience/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed([FromBody] ExperienceDeleteViewModel ViewModel)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = await _context.Persons.FirstOrDefaultAsync(x => x.UserId == userId);

            if (profileData == null)
            {
                // return RedirectToAction("Index", "Profile");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the Experience that Belongs to the Current Profile
            var experienceData = await _context.Experiences.FirstOrDefaultAsync(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (experienceData == null)
            {
                // return NotFound();
                return BadRequest("Experience not found.");
            }

            _context.Experiences.Remove(experienceData);
            await _context.SaveChangesAsync();

            // return RedirectToAction("Index", "Profile");
            return Ok(new { message = "Experience deleted successfully." });
        }
    }
}