using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.ViewModels.AdminViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        // The table is not rendered here. The page asks /Admin/List for it with ajax,
        // so this action only draws the filter form and an empty container.
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Admin/List
        // Returns just the table as html, which the page drops into its container
        public async Task<IActionResult> List(string search, string gender, string bloodGroup, int page = 1)
        {
            // How many profiles fit on one page
            const int pageSize = 10;

            // AsQueryable() is used when a Developer Wants to Dynamically query a Entity it is used for sorting, filtering and more 
            // AsNoTracking() because this page only displays the list, we never change these rows
            var query = _context.Persons.AsNoTracking().AsQueryable();

            // If the search (name) filter is not null it will search via the given "search" Content 
            if (!string.IsNullOrEmpty(search))
            {
                // Contains() checks whether a string contains a particular piece of text.
                query = query.Where(x => x.FullName.Contains(search));
            }

            // If the Gender Is Provided then
            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(x => x.Gender == gender);
            }

            // Same for the Blood Group, the empty option means "show every blood group"
            if (!string.IsNullOrEmpty(bloodGroup))
            {
                query = query.Where(x => x.BloodGroup == bloodGroup);
            }

            // Count first, so the view knows how many page links it has to draw
            int totalCount = await query.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));

            // The page number arrives from the URL, so never trust it.
            // Somebody can type /Admin?page=999 and Skip would walk off the end of the list
            if (page < 1)
            {
                page = 1;
            }

            if (page > totalPages)
            {
                page = totalPages;
            }

            // Skip and Take become OFFSET and FETCH in SQL, that is how SQL Server pages
            var profiles = await query.OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                City = x.Addresses.Where(a => a.IsPrimary).Select(a => a.City).FirstOrDefault(),
                CreatedAt = x.CreatedAt
            })
                .ToListAsync();

            ViewBag.Total = totalCount;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return PartialView("_ProfileList", profiles);
        }

        // GET: /Admin/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var person = await _context.Persons
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

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

                Addresses = await _context.Addresses.AsNoTracking().Where(x => x.PersonId == person.Id).ToListAsync(),
                Contacts = await _context.Contacts.AsNoTracking().Where(x => x.PersonId == person.Id).ToListAsync(),
                Educations = await _context.Educations
                    .AsNoTracking()
                    .Where(x => x.PersonId == person.Id)
                    .OrderByDescending(x => x.StartYear)
                    .ToListAsync(),

                Experiences = await _context.Experiences
                    .AsNoTracking()
                    .Where(x => x.PersonId == person.Id)
                    .OrderByDescending(x => x.StartDate)
                    .ToListAsync(),
                SocialLinks = await _context.SocialLinks.AsNoTracking().Where(x => x.PersonId == person.Id).ToListAsync()
            };

            return View(viewModel);
        }
    }
}