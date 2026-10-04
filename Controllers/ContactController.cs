using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using CustomerManagementPractiseCS.ViewModels.ContactViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementPractiseCS.Controllers
{
    [Authorize]
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ContactController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Contact/Create
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

        // GET: /Contact/List
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

            // Get the Contacts that Belong to the Current Profile
            var contact = await _context.Contacts.Where(x => x.PersonId == profileData.Id).ToListAsync();

            // Return the partial
            return PartialView("_ContactList", contact);
        }

        // POST: /Contact/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ContactCreateViewModel ViewModel)
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

            // If this is Marked Primary, Demote the Others of the Same Type
            if (ViewModel.IsPrimary)
            {
                var others = await _context.Contacts.Where(x => x.PersonId == profileData.Id).ToListAsync();

                foreach (var other in others)
                {
                    other.IsPrimary = false;
                }
            }

            var contactData = new Contact
            {
                PersonId = profileData.Id,
                ContactType = ViewModel.ContactType,
                Label = ViewModel.Label,
                Value = ViewModel.Value,
                IsPrimary = ViewModel.IsPrimary,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Contacts.Add(contactData);
            await _context.SaveChangesAsync();

            // return RedirectToAction("Index", "Profile");
            return Ok(new { message = "Contact added successfully." });
        }

        // GET: /Contact/Edit/5
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

            // Get the Contact that Belongs to the Current Profile
            var contactData = await _context.Contacts.FirstOrDefaultAsync(x => x.Id == id && x.PersonId == profileData.Id);

            if (contactData == null)
            {
                // return NotFound();
                return BadRequest("Contact not found.");
            }

            var viewModel = new ContactEditViewModel
            {
                Id = contactData.Id,
                ContactType = contactData.ContactType,
                Label = contactData.Label,
                Value = contactData.Value,
                IsPrimary = contactData.IsPrimary
            };

            // return View(viewModel);
            return PartialView("_ContactEditForm", viewModel);
        }

        // POST: /Contact/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ContactEditViewModel ViewModel)
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

            // Get the Contact that Belongs to the Current Profile
            var contactData = await _context.Contacts
                .FirstOrDefaultAsync(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (contactData == null)
            {
                // return NotFound();
                return BadRequest("Contact not found.");

            }

            // If this is Marked Primary, Demote the Others of the Same Type
            if (ViewModel.IsPrimary)
            {
                var others = await _context.Contacts.Where(x => x.PersonId == profileData.Id && x.Id != ViewModel.Id).ToListAsync();

                foreach (var other in others)
                {
                    other.IsPrimary = false;
                }
            }

            contactData.ContactType = ViewModel.ContactType;
            contactData.Label = ViewModel.Label;
            contactData.Value = ViewModel.Value;
            contactData.IsPrimary = ViewModel.IsPrimary;
            contactData.UpdatedAt = DateTime.UtcNow;
            contactData.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            // return RedirectToAction("Index", "Profile");
            return Ok(new { message = "Contact updated successfully." });
        }

        // GET: /Contact/Delete/5
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

            // Get the Contact that Belongs to the Current Profile
            var contactData = await _context.Contacts
                .FirstOrDefaultAsync(x => x.Id == id && x.PersonId == profileData.Id);

            if (contactData == null)
            {
                // return NotFound();
                return BadRequest("Contact not found.");
            }

            var viewModel = new ContactDeleteViewModel
            {
                Id = contactData.Id,
                ContactType = contactData.ContactType,
                Label = contactData.Label,
                Value = contactData.Value,
                IsPrimary = contactData.IsPrimary
            };

            // return View(viewModel);
            return PartialView("_ContactDeleteForm", viewModel);
        }

        // POST: /Contact/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed([FromBody] ContactDeleteViewModel ViewModel)
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

            // Get the Contact that Belongs to the Current Profile
            var contactData = await _context.Contacts.FirstOrDefaultAsync(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (contactData == null)
            {
                // return NotFound();
                return BadRequest("Contact not found.");
            }

            _context.Contacts.Remove(contactData);
            await _context.SaveChangesAsync();

            // return RedirectToAction("Index", "Profile");
            return Ok(new { message = "Contact deleted successfully." });
        }
    }
}
