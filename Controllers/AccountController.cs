using CustomerManagementPractiseCS.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagementPractiseCS.Controllers
{

    // The UserManager, SignInManager, RoleManager Provaide the Async Version of there
    // Methods Because its built with the mind thata there will be many users 
    public class AccountController : Controller
    {
        // IdentityUser is the built-in user class provided by Identity 
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public IActionResult Register()
        {
            return View();
        }

        // The Task Represents a Async Funtion that can return a value 
        [HttpPost]
        public async Task<IActionResult> Register(string email, string password)
        {
            // Check if there is any Users in DB Already 
            bool usersAlreadyExist = _userManager.Users.Any();

            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
            };

            // This auto Hashes the Password in PBKDF2
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Check If there Is an Admin Role
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Check if there is a User Role
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }

                // IF there is already users inside User
                if (usersAlreadyExist)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }

                return RedirectToAction("Login");
            }

            // Show the Identity Errors (Duplicate Email, Weak Password and More)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: false);
            // isPersistent: is to Controle how Log the Auth Cookie Stayes False: Current Browser Session.
            // lockoutOnFailure: is the built in multi login attempt login Locker 

            if (result.Succeeded)
            {
                // The SignInManager needs the User object to check roles
                var user = await _userManager.FindByEmailAsync(email);

                // If the User is an Admin go to Index
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Customers");
                }

                // Otherwise (regular User) go to MyData
                return RedirectToAction("MyData", "Customers");
            }

            // Wrong Email or Password
            ModelState.AddModelError(string.Empty, "Invalid email or password.");

            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: Account/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var vm = new ProfileViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Roles = await _userManager.GetRolesAsync(user)
            };

            return View(vm);
        }

        // GET: Account/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
        
            var vm = new EditProfileViewModel
            {
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                PhoneNumber = user.PhoneNumber
            };
        
            return View(vm);
        }
        
        // POST: Account/EditProfile
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
        
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
        

            vm.Email = user.Email ?? string.Empty;
        
            // UserName
            if (user.UserName != vm.UserName)
            {
                var result = await _userManager.SetUserNameAsync(user, vm.UserName);
                if (!result.Succeeded)
                {
                    foreach (var e in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, e.Description);
                    }
        
                    return View(vm);
                }
            }
        
            // Phone
            if (user.PhoneNumber != vm.PhoneNumber)
            {
                var result = await _userManager.SetPhoneNumberAsync(user, vm.PhoneNumber);
                if (!result.Succeeded)
                {
                    foreach (var e in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, e.Description);
                    }
        
                    return View(vm);
                }
            }
        
            // Refresh the login so everything is up to date
            await _signInManager.RefreshSignInAsync(user);
        
            return RedirectToAction("Profile");
        }

        // GET: Account/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, e.Description);
                }

                return View(vm);
            }

            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Profile");
        }

    }
}
