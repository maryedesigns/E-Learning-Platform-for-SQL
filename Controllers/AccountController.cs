using E_LearningProject.Entities;
using E_LearningProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace E_LearningProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
       // private readonly IEmailSender _emailSender;
        public AccountController(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment webHostEnvironment)
        { 
            _context = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;

        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User); // Get the logged-in user
            var profilePicture = user.ProfilePicturePath ?? "~/images/default-profile.png"; // Default picture if not set
            var model = new EditProfileViewModel
            {
                FullName = user.FullName,
                ProfilePicturePath = profilePicture
            };

            return View(model);
        }


        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email)
                    };

                    await _userManager.AddClaimsAsync(user, claims);
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Set TempData to show the success message in the view
                    TempData["SignupSuccess"] = "Account Created Successfully!";

                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }


        public IActionResult Login()
        {
            return View();  
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);

                    if (result.Succeeded)
                    {
                        // Adding Full Name to Claims
                        var claims = new List<Claim>
                        {
                            new Claim("FullName", user.FullName)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await _signInManager.SignInAsync(user, isPersistent: false);

                        return RedirectToAction("MyLearning");
                    }
                }

                ModelState.AddModelError("", "Invalid email or password.");
            }

            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> MyLearning()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Name = user.FullName; 
            var myCourses = await _context.MyLearnings
                .Include(m => m.Course)
                .Where(m => m.UserId == user.Id)
                .ToListAsync();

            return View(myCourses);
        }



        [Authorize]
        public async Task<IActionResult> StartLearning()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Name = user.FullName; 
            var courses = await _context.Courses.ToListAsync(); 
            var coursesViewModel = courses.Select(c => new Courses
            {
                Id = c.Id,
                CourseName = c.CourseName,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                YouTubeUrl = c.YouTubeUrl
            }).ToList();

            return View(coursesViewModel);
        }

        public async Task<IActionResult> ViewCourses()
        {
            var courses = await _context.Courses.ToListAsync();
            var coursesViewModel = courses.Select(c => new Courses
            {
                Id = c.Id,
                CourseName = c.CourseName,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                YouTubeUrl = c.YouTubeUrl
            }).ToList();

            return View(coursesViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => View();

        [HttpGet]
        public IActionResult UploadProfilePicture()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new EditProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                ProfilePicturePath = user.ProfilePicturePath ?? "~/images/default-pic.png"
            };

            return View(model); 
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Handle profile picture upload
            if (model.ProfilePic != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/profiles");
                string fileName = $"{Guid.NewGuid()}_{model.ProfilePic.FileName}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                // Ensure directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePic.CopyToAsync(fileStream);
                }

                // Update user profile picture path
                user.ProfilePicturePath = $"/images/profiles/{fileName}";
            }

            // Update other user details
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to update profile.");
                return View(model);
            }

            return RedirectToAction("StartLearning", "Account");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // To prevent email enumeration, return success even if the user is not found
                return View("ForgotPasswordConfirmation");
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Create reset link
            var resetLink = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);

            // Send email
            var subject = "Password Reset Request";
            var message = $"Click the link below to reset your password:\n{resetLink}";

            //await _emailSender.SendEmailAsync(model.Email, subject, message);

            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

    }
}
