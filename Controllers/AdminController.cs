using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.ViewModels.AdminViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CustomerManagementPractiseCS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Admin
        public IActionResult Index(string search, string gender)
        {
            // AsQueryable() is used when a Developer Wants to Daynamically Querry a Entity it is used for sorting, filtering and more 
            var querry = _context.Persons.AsQueryable();

            // If the search (name) filetr is not null it will search via the given "search" Content 
            if (!string.IsNullOrEmpty(search))
            {
                // Contains() checks whether a string contains a particular piece of text.
                querry = querry.Where(x => x.FullName.Contains(search));
            }

            // If the Gender Is Provaided then
            if (!string.IsNullOrEmpty(gender))
            {
                querry = querry.Where(x => x.Gender == gender);
            }


            var profiles = querry.OrderByDescending(x => x.CreatedAt).Select(x => new AdminProfileListViewModel
            {
                Id = x.Id,
                UserId = x.UserId,
                FullName = x.FullName,
                Gender = x.Gender,
                DateOfBirth = x.DateOfBirth,
                BloodGroup = x.BloodGroup,
                Religion = x.Religion,
                PhotoUrl = x.PhotoUrl,
                City = x.Addresses.Where(a => a.IsPrimary).Select(a => a.City).FirstOrDefault(),
                CreatedAt = x.CreatedAt
            })
                .ToList();

            return View(profiles);
        }

        // GET: /Admin/Details/5
        public IActionResult Details(int id)
        {
            var person = _context.Persons.FirstOrDefault(x => x.Id == id);

            if (person == null)
            {
                return NotFound();
            }

            var viewModel = new AdminProfileDetailsViewModel
            {
                Id = person.Id,
                UserId = person.UserId,
                FullName = person.FullName,
                Gender = person.Gender,
                DateOfBirth = person.DateOfBirth,
                Religion = person.Religion,
                BloodGroup = person.BloodGroup,
                PhotoUrl = person.PhotoUrl,
                Summary = person.Summary,
                CreatedAt = person.CreatedAt,
                UpdatedAt = person.UpdatedAt,

                Addresses = _context.Addresses.Where(x => x.PersonId == person.Id).ToList(),
                Contacts = _context.Contacts.Where(x => x.PersonId == person.Id).ToList(),
                Educations = _context.Educations
                    .Where(x => x.PersonId == person.Id)
                    .OrderByDescending(x => x.StartYear)
                    .ToList(),

                Experiences = _context.Experiences
                    .Where(x => x.PersonId == person.Id)
                    .OrderByDescending(x => x.StartDate)
                    .ToList(),
                SocialLinks = _context.SocialLinks.Where(x => x.PersonId == person.Id).ToList()
            };

            return View(viewModel);
        }
    }
}