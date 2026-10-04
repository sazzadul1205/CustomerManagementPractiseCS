using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Models;
using CustomerManagementPractiseCS.ViewModels.AddressViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementPractiseCS.Controllers
{
    [Authorize]
    public class AddressController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AddressController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);

            // Check If there ios any Profile 
            var profileData = await _context.Persons.AnyAsync(x => x.UserId == userId);

            if (!profileData)
            {
                return RedirectToAction("Index", "Profile");
            }

            return View();
        }

        // GET: /Address/List
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

            // Get the Addresses that Belong to the Current Profiles
            var addresses = await _context.Addresses.Where(x => x.PersonId == profileData.Id).ToListAsync();

            // Return the partial
            return PartialView("_AddressList", addresses);
        }

        // POST: /Address/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddressCreateViewModel ViewModel)
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

            // If this is Marked Primary, then uncheck all the others 
            if (ViewModel.IsPrimary)
            {
                // Get all the Addresses related to this Profile 
                var others = await _context.Addresses.Where(x => x.PersonId == profileData.Id).ToListAsync();

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
            await _context.SaveChangesAsync();

            // return RedirectToAction("Index", "Profile");
            return Ok(new { message = "Address added successfully." });
        }

        // GET: /Address/Edit/5
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

            // Get the Address that Belongs to the Current Profile and validate the Provided id 
            var addressData = await _context.Addresses.FirstOrDefaultAsync(x => x.Id == id && x.PersonId == profileData.Id);

            if (addressData == null)
            {
                // return NotFound();
                return BadRequest("Address not found.");
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

            // return View(viewModel);
            // return Ok(viewModel);
            return PartialView("_AddressEditForm", viewModel);
        }

        // POST: /Address/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] AddressEditViewModel ViewModel)
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

            // Get  the address whose ID matches the requested address ID AND whose PersonId matches the current users profile ID.
            var addressData = await _context.Addresses.FirstOrDefaultAsync(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (addressData == null)
            {
                // return NotFound();
                return BadRequest("Address not found.");
            }

            // If this is Marked Primary, then uncheck all the others 
            if (ViewModel.IsPrimary)
            {
                // Get all addresses belonging to this person except the address currently being edited.
                var others = await _context.Addresses.Where(x => x.PersonId == profileData.Id && x.Id != ViewModel.Id).ToListAsync();

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

            await _context.SaveChangesAsync();

            // return RedirectToAction("Index", "Profile");
            return Ok(new { message = "Address updated successfully." });
        }

        // GET: /Address/Delete/5
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

            // Get the Address that Belongs to the Current Profile
            var addressData = await _context.Addresses.FirstOrDefaultAsync(x => x.Id == id && x.PersonId == profileData.Id);

            if (addressData == null)
            {
                // return NotFound();
                return BadRequest("Address not found.");
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

            // return View(viewModel);
            return PartialView("_AddressDeleteForm", viewModel);
        }

        // POST: /Address/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed([FromBody] AddressDeleteViewModel ViewModel)
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

            // Get the Address that Belongs to the Current Profile
            var addressData = await _context.Addresses.FirstOrDefaultAsync(x => x.Id == ViewModel.Id && x.PersonId == profileData.Id);

            if (addressData == null)
            {
                // return NotFound();
                return BadRequest("Address not found.");
            }

            _context.Addresses.Remove(addressData);
            await _context.SaveChangesAsync();

            // return RedirectToAction("Index", "Profile");
            return Ok(new { message = "Address deleted successfully." });
        }
    }
}
