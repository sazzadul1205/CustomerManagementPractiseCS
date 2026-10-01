using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using CustomerManagementPractiseCS.ViewModels.SocialLinkViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        // POST: /SocialLink/Create
        [HttpPost]
        public IActionResult Create(SocialLinkCreateViewModel ViewModel)
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

            var socialLinkData = new SocialLink
            {
                PersonId = profileData.Id,
                Platform = ViewModel.Platform,
                Url = ViewModel.Url,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.SocialLinks.Add(socialLinkData);
            _context.SaveChanges();

            return RedirectToAction("Index", "Profile");
        }

        // GET: /SocialLink/Edit/5
        public IActionResult Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

            if (profileData == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            // Get the SocialLink that Belongs to the Current Profile
            var socialLinkData = _context.SocialLinks.FirstOrDefault(x => x.Id == id && x.PersonId == profileData.Id);

            if (socialLinkData == null)
            {
                return NotFound();
            }

            var viewModel = new SocialLinkEditViewModel
            {
                Id = socialLinkData.Id,
                Platform = socialLinkData.Platform,
                Url = socialLinkData.Url
            };

            return View(viewModel);
        }

        // POST: /SocialLink/Edit/5
        [HttpPost]
        public IActionResult Edit(SocialLinkEditViewModel ViewModel)
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

            // Get the SocialLink that Belongs to the Current Profile
            var socialLinkData = _context.SocialLinks.FirstOrDefault(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (socialLinkData == null)
            {
                return NotFound();
            }

            socialLinkData.Platform = ViewModel.Platform;
            socialLinkData.Url = ViewModel.Url;
            socialLinkData.UpdatedAt = DateTime.UtcNow;
            socialLinkData.UpdatedBy = userId;

            _context.SaveChanges();

            return RedirectToAction("Index", "Profile");
        }

        // GET: /SocialLink/Delete/5
        public IActionResult Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

            if (profileData == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            // Get the SocialLink that Belongs to the Current Profile
            var socialLinkData = _context.SocialLinks.FirstOrDefault(x => x.Id == id && x.PersonId == profileData.Id);

            if (socialLinkData == null)
            {
                return NotFound();
            }

            var viewModel = new SocialLinkDeleteViewModel
            {
                Id = socialLinkData.Id,
                Platform = socialLinkData.Platform,
                Url = socialLinkData.Url
            };

            return View(viewModel);
        }

        // POST: /SocialLink/Delete/5
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

            // Get the SocialLink that Belongs to the Current Profile
            var socialLinkData = _context.SocialLinks.FirstOrDefault(x => x.Id == id && x.PersonId == profileData.Id);

            if (socialLinkData == null)
            {
                return NotFound();
            }

            _context.SocialLinks.Remove(socialLinkData);
            _context.SaveChanges();

            return RedirectToAction("Index", "Profile");
        }
    }
}