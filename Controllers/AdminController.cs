using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.ViewModels.AdminViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Index(string? search, string? gender)
        {
            // Start with all profiles
            var query = _context.Persons.AsQueryable();

            // Apply search filter (name or city)
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.FullName.Contains(search) ||
                    x.Addresses.Any(a => a.City.Contains(search)));
            }

            // Apply gender filter
            if (!string.IsNullOrWhiteSpace(gender))
            {
                query = query.Where(x => x.Gender == gender);
            }

            var profiles = query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new AdminProfileListViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    FullName = x.FullName,
                    Gender = x.Gender,
                    DateOfBirth = x.DateOfBirth,
                    BloodGroup = x.BloodGroup,
                    Religion = x.Religion,
                    PhotoUrl = x.PhotoUrl,
                    City = x.Addresses
                        .Where(a => a.IsPrimary)
                        .Select(a => a.City)
                        .FirstOrDefault(),
                    CreatedAt = x.CreatedAt
                })
                .ToList();

            // Pass filter values back so the form stays populated
            ViewData["Search"] = search;
            ViewData["Gender"] = gender;

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

                Addresses = _context.Addresses
                    .Where(x => x.PersonId == person.Id).ToList(),
                Contacts = _context.Contacts
                    .Where(x => x.PersonId == person.Id).ToList(),
                Educations = _context.Educations
                    .Where(x => x.PersonId == person.Id).ToList(),
                Experiences = _context.Experiences
                    .Where(x => x.PersonId == person.Id).ToList(),
                SocialLinks = _context.SocialLinks
                    .Where(x => x.PersonId == person.Id).ToList()
            };

            return View(viewModel);
        }
    }
}