using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using CustomerManagementPractiseCS.ViewModels.AddressViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagementPractiseCS.Controllers
{
    [Authorize]
    public class AddressController: Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AddressController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);

            // Check If there ios any Profile 
            var profileData = _context.Persons.Any(x => x.UserId == userId);

            if (!profileData)
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

            var addresses = _context.Addresses.Where(x => x.PersonId == personId).ToList();

            // Return the partial
            return PartialView("_AddressList", addresses);
        }

        // POST: /Address/Create
        [HttpPost]
        public IActionResult Create(AddressCreateViewModel ViewModel)
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

            // If this is Marked Primary, then uncheck all the others 
            if (ViewModel.IsPrimary)
            {
                // Get all the Addresses related to this Profile 
                var others = _context.Addresses.Where(x => x.PersonId == profileData.Id).ToList();

                // One By one Uncheck IsPrimary 
                foreach (var other in others)
                {
                    other.IsPrimary = false;
                }
            }

            var addressData = new Address
            {
                PersonId = profileData.Id,
                AddressType = ViewModel.AddressType,
                Line1 = ViewModel.Line1,
                City = ViewModel.City,
                State = ViewModel.State,
                PostalCode = ViewModel.PostalCode,
                Country = ViewModel.Country,
                IsPrimary = ViewModel.IsPrimary,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Addresses.Add(addressData);
            _context.SaveChanges();

            TempData["Success"] = "Address added successfully.";

            return RedirectToAction("Index", "Profile");
        }
        // GET: /Address/Edit/5
        public IActionResult Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

            if (profileData == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            // Get the Address that Belongs to the Current Profile and validate the Provided id 
            var addressData = _context.Addresses.FirstOrDefault(x => x.Id == id && x.PersonId == profileData.Id);

            if (addressData == null)
            {
                return NotFound();
            }

            var viewModel = new AddressEditViewModel
            {
                Id = addressData.Id,
                AddressType = addressData.AddressType,
                Line1 = addressData.Line1,
                City = addressData.City,
                State = addressData.State,
                PostalCode = addressData.PostalCode,
                Country = addressData.Country,
                IsPrimary = addressData.IsPrimary
            };

            return View(viewModel);
        }

        // POST: /Address/Edit/5
        [HttpPost]
        public IActionResult Edit(AddressEditViewModel ViewModel)
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

            // Get  the address whose ID matches the requested address ID AND whose PersonId matches the current users profile ID.
            var addressData = _context.Addresses.FirstOrDefault(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (addressData == null)
            {
                return NotFound();
            }

            // If this is Marked Primary, then uncheck all the others 
            if (ViewModel.IsPrimary)
            {
                // Get all addresses belonging to this person except the address currently being edited.
                var others = _context.Addresses.Where(x => x.PersonId == profileData.Id && x.Id != ViewModel.Id).ToList();

                foreach (var other in others)
                {
                    other.IsPrimary = false;
                }
            }

            addressData.AddressType = ViewModel.AddressType;
            addressData.Line1 = ViewModel.Line1;
            addressData.City = ViewModel.City;
            addressData.State = ViewModel.State;
            addressData.PostalCode = ViewModel.PostalCode;
            addressData.Country = ViewModel.Country;
            addressData.IsPrimary = ViewModel.IsPrimary;
            addressData.UpdatedAt = DateTime.UtcNow;
            addressData.UpdatedBy = userId;

            _context.SaveChanges();

            TempData["Success"] = "Address updated successfully.";

            return RedirectToAction("Index", "Profile");
        }

        // GET: /Address/Delete/5
        public IActionResult Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Get the Profile that Belongs to the Current User
            var profileData = _context.Persons.FirstOrDefault(x => x.UserId == userId);

            if (profileData == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            // Get the Address that Belongs to the Current Profile
            var addressData = _context.Addresses.FirstOrDefault(x => x.Id == id && x.PersonId == profileData.Id);

            if (addressData == null)
            {
                return NotFound();
            }

            var viewModel = new AddressDeleteViewModel
            {
                Id = addressData.Id,
                AddressType = addressData.AddressType,
                Line1 = addressData.Line1,
                City = addressData.City,
                State = addressData.State,
                PostalCode = addressData.PostalCode,
                Country = addressData.Country,
                IsPrimary = addressData.IsPrimary
            };

            return View(viewModel);
        }

        // POST: /Address/Delete/5
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

            // Get the Address that Belongs to the Current Profile
            var addressData = _context.Addresses.FirstOrDefault(x => x.Id == id && x.PersonId == profileData.Id);

            if (addressData == null)
            {
                return NotFound();
            }

            _context.Addresses.Remove(addressData);
            _context.SaveChanges();

            TempData["Success"] = "Address deleted successfully.";

            return RedirectToAction("Index", "Profile");
        }
    }
}
