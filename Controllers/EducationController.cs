using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using CustomerManagementPractiseCS.ViewModels.EducationViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // GET: /Education/List
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

            // Get the Educations that Belong to the Current Profile
            var educations = await _context.Educations
                .Where(x => x.PersonId == profileData.Id)
                .OrderByDescending(x => x.StartYear)
                .ToListAsync();

            // Return the partial
            return PartialView("_EducationList", educations);
        }

        // POST: /Education/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EducationCreateViewModel ViewModel)
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
            await _context.SaveChangesAsync();

            // return RedirectToAction("Index", "Profile");
            return Ok(new { message = "Education added successfully." });
        }

        // GET: /Education/Edit/5
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

            // Get the Education that Belongs to the Current Profile
            var educationData = await _context.Educations.FirstOrDefaultAsync(x => x.Id == id && x.PersonId == profileData.Id);

            if (educationData == null)
            {
                // return NotFound();
                return BadRequest("Education not found.");
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

            // return View(viewModel);
            return PartialView("_EducationEditForm", viewModel);
        }

        // POST: /Education/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] EducationEditViewModel ViewModel)
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

            // Get the Education that Belongs to the Current Profile
            var educationData = await _context.Educations.FirstOrDefaultAsync(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (educationData == null)
            {
                // return NotFound();
                return BadRequest("Education not found.");
            }

            educationData.DegreeLevel = ViewModel.DegreeLevel;
            educationData.DegreeName = ViewModel.DegreeName;
            educationData.InstitutionName = ViewModel.InstitutionName;
            educationData.BoardOrUniversity = ViewModel.BoardOrUniversity;
            educationData.StartYear = ViewModel.StartYear;
            educationData.EndYear = ViewModel.IsOngoing ? null : ViewModel.EndYear;
            educationData.IsOngoing = ViewModel.IsOngoing;
            educationData.Result = ViewModel.Result;
            educationData.UpdatedAt = DateTime.UtcNow;
            educationData.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            // return RedirectToAction("Index", "Profile");
            return Ok(new { message = "Education updated successfully." });
        }

        // GET: /Education/Delete/5
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

            // Get the Education that Belongs to the Current Profile
            var educationData = await _context.Educations.FirstOrDefaultAsync(x => x.Id == id && x.PersonId == profileData.Id);

            if (educationData == null)
            {
                // return NotFound();
                return BadRequest("Education not found.");
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

            // return View(viewModel);
            return PartialView("_EducationDeleteForm", viewModel);
        }

        // POST: /Education/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed([FromBody] EducationDeleteViewModel ViewModel)
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

            // Get the Education that Belongs to the Current Profile
            var educationData = await _context.Educations.FirstOrDefaultAsync(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (educationData == null)
            {
                // return NotFound();
                return BadRequest("Education not found.");
            }

            _context.Educations.Remove(educationData);
            await _context.SaveChangesAsync();

            // return RedirectToAction("Index", "Profile");
            return Ok(new { message = "Education deleted successfully." });
        }
    }
}
