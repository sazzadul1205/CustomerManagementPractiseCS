using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using CustomerManagementPractiseCS.ViewModels.SocialLinkViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementPractiseCS.Controllers
{
    [Authorize]
    public class SocialLinkController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SocialLinkController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /SocialLink/Create
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

        // GET: /SocialLink/List
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

            // Get the SocialLinks that Belong to the Current Profile
            var socialLinks = await _context.SocialLinks
                .Where(x => x.PersonId == profileData.Id)
                .OrderBy(x => x.Platform)
                .ToListAsync();

            // Return the partial
            return PartialView("_SocialLinkList", socialLinks);
        }

        // POST: /SocialLink/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SocialLinkCreateViewModel ViewModel)
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

            var socialLinkData = new SocialLink
            {
                PersonId = profileData.Id,
                Platform = ViewModel.Platform,
                Url = ViewModel.Url,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.SocialLinks.Add(socialLinkData);
            await _context.SaveChangesAsync();

            // return RedirectToAction("Index", "Profile");
            return Ok(new { message = "Social link added successfully." });
        }

        // GET: /SocialLink/Edit/5
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

            // Get the SocialLink that Belongs to the Current Profile
            var socialLinkData = await _context.SocialLinks.FirstOrDefaultAsync(x => x.Id == id && x.PersonId == profileData.Id);

            if (socialLinkData == null)
            {
                // return NotFound();
                return BadRequest("Social link not found.");
            }

            var viewModel = new SocialLinkEditViewModel
            {
                Id = socialLinkData.Id,
                Platform = socialLinkData.Platform,
                Url = socialLinkData.Url
            };

            // return View(viewModel);
            return PartialView("_SocialLinkEditForm", viewModel);
        }

        // POST: /SocialLink/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] SocialLinkEditViewModel ViewModel)
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

            // Get the SocialLink that Belongs to the Current Profile
            var socialLinkData = await _context.SocialLinks.FirstOrDefaultAsync(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (socialLinkData == null)
            {
                // return NotFound();
                return BadRequest("Social link not found.");
            }

            socialLinkData.Platform = ViewModel.Platform;
            socialLinkData.Url = ViewModel.Url;
            socialLinkData.UpdatedAt = DateTime.UtcNow;
            socialLinkData.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            // return RedirectToAction("Index", "Profile");
            return Ok(new { message = "Social link updated successfully." });
        }

        // GET: /SocialLink/Delete/5
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

            // Get the SocialLink that Belongs to the Current Profile
            var socialLinkData = await _context.SocialLinks.FirstOrDefaultAsync(x => x.Id == id && x.PersonId == profileData.Id);

            if (socialLinkData == null)
            {
                // return NotFound();
                return BadRequest("Social link not found.");
            }

            var viewModel = new SocialLinkDeleteViewModel
            {
                Id = socialLinkData.Id,
                Platform = socialLinkData.Platform,
                Url = socialLinkData.Url
            };

            // return View(viewModel);
            return PartialView("_SocialLinkDeleteForm", viewModel);
        }

        // POST: /SocialLink/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed([FromBody] SocialLinkDeleteViewModel ViewModel)
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

            // Get the SocialLink that Belongs to the Current Profile
            var socialLinkData = await _context.SocialLinks.FirstOrDefaultAsync(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (socialLinkData == null)
            {
                // return NotFound();
                return BadRequest("Social link not found.");
            }

            _context.SocialLinks.Remove(socialLinkData);
            await _context.SaveChangesAsync();

            // return RedirectToAction("Index", "Profile");
            return Ok(new { message = "Social link deleted successfully." });
        }
    }
}