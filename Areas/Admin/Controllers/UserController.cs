using cce106_palit.Data;
using cce106_palit.Entity;
using cce106_palit.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using X.PagedList;

namespace cce106_palit.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Super Admin, Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();
        private readonly EmailService _emailService;

        public UserController(ApplicationDbContext context, EmailService email)
        {
            _context = context;
            _emailService = email;
        }

        [Route("admin/users")]
        [HttpGet]
        public IActionResult Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var users = _context.Users
                                   .OrderBy(p => p.Id)
                                   .ToPagedList(pageNumber, pageSize);
            ViewBag.Page = pageNumber;
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string Name, string Email, string Role)
        {
            bool userExists = await _context.Users
            .AnyAsync(u => u.Email == Email);

            if (userExists)
            {
                TempData["Message"] = "A user with this email already exists.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }


            // Generate Username
            string sanitizedName = Name.ToLower().Replace(" ", "");

            string generatedUsername = $"{sanitizedName}{new Random().Next(100, 999)}";

            string generatedPassword = sanitizedName + 123;
            string hashedPassword = _passwordHasher.HashPassword(null, generatedPassword);

            var user = new User
            {
                Username = generatedUsername,
                Password = hashedPassword,
                Name = Name,
                Email = Email,
                IsVerified = true,
                Role = Role,
                Created_At = DateTime.Now,
                Updated_At = DateTime.Now,
            };

            
            _context.Users.Add(user);
            _context.SaveChanges();

            await _emailService.SendEmailAsync(user.Email, "Your PALIT Account Credentials",
                 $@"<p>Dear {user.Name},</p>  
                   <p>Your account on <strong>PALIT</strong> has been created successfully.</p>  

                   <p><strong>Here are your login credentials:</strong></p>  
                   <ul>
                       <li><strong>Username:</strong> {user.Username}</li>
                       <li><strong>Password:</strong> {generatedPassword}</li>
                   </ul>

                   <p>For security, please change your password after logging in.</p>

                   <p>Best regards,<br><strong>The PALIT Team</strong></p>");

            var totalBefore = await _context.Users
               .Where(c => c.Id < user.Id)
               .CountAsync();

            int pageSize = 10;
            int page = (totalBefore / pageSize) + 1;

            TempData["Message"] = "User successfully added!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Index", new { page });
        }


        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }


        [Route("/User/UpdateProfile")]
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromForm] User updatedUser)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
            {
                return NotFound();
            }

            var existingUsername = await _context.Users.FirstOrDefaultAsync(u => u.Username == updatedUser.Username);
            if (existingUsername != null && existingUsername.Id != user.Id)
            {
                return Json(new { success = false, message = "This username is already in use by another account." });
            }

            user.Name = updatedUser.Name;
            user.Username = updatedUser.Username;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Profile updated successfully" });
        }

        [Route("/User/ChangeEmail")]
        [HttpPatch]
        public async Task<IActionResult> ChangeEmail([FromBody] User updatedUser)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
            {
                return NotFound();
            }

            var existingUserWithEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == updatedUser.Email);
            if (existingUserWithEmail != null && existingUserWithEmail.Id != user.Id)
            {
                return Json(new { success = false, field = "email", message = "This email is already in use by another account." });
            }

            if (user.Email == updatedUser.Email)
            {
                return Json(new { success = false, field = "email", message = "This is your current email. If you wish to change your email, please enter a valid one." });
            }

            var passwordIsValid = _passwordHasher.VerifyHashedPassword(null, user.Password, updatedUser.Password) == PasswordVerificationResult.Success;
            if (!passwordIsValid)
            {
                return Json(new { success = false, field = "password", message = "Password is incorrect." });
            }

            user.Email = updatedUser.Email;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Email updated successfully!" });
        }

        [Route("/User/ChangePassword")]
        [HttpPatch]
        public async Task<IActionResult> ChangePassword([FromBody] Dictionary<string, string> data)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                if (!data.TryGetValue("Password", out var password) || string.IsNullOrEmpty(password))
                {
                    return Json(new { success = false, field = "password", message = "Password is required." });
                }

                var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$");
                if (!passwordRegex.IsMatch(password))
                {
                    return Json(new
                    {
                        success = false,
                        field = "password",
                        message = "Password must be at least 8 characters and include uppercase, lowercase, a number, and a special character."
                    });
                }

                user.Password = _passwordHasher.HashPassword(null, password);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Password updated successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while updating password." });
            }
        }
    }
}
