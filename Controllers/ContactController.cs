using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using CustomerManagementPractiseCS.ViewModels.ContactViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        public IActionResult List(int personId)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

            // Here we return not found because this is for partial view
            if (profileData == null)
            {
                return NotFound();
            }

            var contact = _context.Contacts.Where(x => x.PersonId == personId).ToList();

            // Return the partial
            return PartialView("_ContactList", contact);
        }

        // POST: /Contact/Create
        [HttpPost]
        public IActionResult Create([FromBody] ContactCreateViewModel ViewModel)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

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
                var others = _context.Contacts.Where(x => x.PersonId == profileData.Id).ToList();

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
            _context.SaveChanges();

            TempData["Success"] = "Contact added successfully.";

            // return RedirectToAction("Index", "Profile");
            return Ok();
        }

        // GET: /Contact/Edit/5
        public IActionResult Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

            if (profileData == null)
            {
                // return RedirectToAction("Index", "Profile");
                return NotFound();
            }

            // Get the Contact that Belongs to the Current Profile
            var contactData = _context.Contacts.FirstOrDefault(x => x.Id == id && x.PersonId == profileData.Id);

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
        public IActionResult Edit([FromBody] ContactEditViewModel ViewModel)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

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
            var contactData = _context.Contacts
                .FirstOrDefault(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (contactData == null)
            {
                // return NotFound();
                return BadRequest("Contact not found.");

            }

            // If this is Marked Primary, Demote the Others of the Same Type
            if (ViewModel.IsPrimary)
            {
                var others = _context.Contacts.Where(x => x.PersonId == profileData.Id && x.Id != ViewModel.Id).ToList();

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

            _context.SaveChanges();

            TempData["Success"] = "Contact updated successfully.";

            // return RedirectToAction("Index", "Profile");
            return Ok();
        }

        // GET: /Contact/Delete/5
        public IActionResult Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

            if (profileData == null)
            {
                // return RedirectToAction("Index", "Profile");
                return NotFound();
            }

            // Get the Contact that Belongs to the Current Profile
            var contactData = _context.Contacts
                .FirstOrDefault(x => x.Id == id && x.PersonId == profileData.Id);

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
        public IActionResult DeleteConfirmed([FromBody] ContactDeleteViewModel ViewModel)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

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
            var contactData = _context.Contacts.FirstOrDefault(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (contactData == null)
            {
                // return NotFound();
                return BadRequest("Contact not found.");
            }

            _context.Contacts.Remove(contactData);
            _context.SaveChanges();

            TempData["Success"] = "Contact deleted successfully.";

            // return RedirectToAction("Index", "Profile");
            return Ok();
        }
    }
}