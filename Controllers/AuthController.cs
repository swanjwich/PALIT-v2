using Microsoft.AspNetCore.Mvc;
using it15_palit.Models.Auth;
using it15_palit.Entity;
using it15_palit.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using System.Data.SqlTypes;

namespace it15_palit.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AuthController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrationAsync(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var passwordHasher = new PasswordHasher<Customer>();

                Customer account = new Customer
                {
                    Username = model.Username,
                    Password = model.Password,
                    Name = model.Name,
                    Address = model.Address,
                    Email = model.Email,
                    Contact_Number = model.Contact_Number,
                    Display_Picture = "images/profile-user.png",
                    Created_at = DateTime.UtcNow,
                    Updated_at = DateTime.UtcNow
                };

                account.Password = passwordHasher.HashPassword(account, model.Password);

                try
                {
                    _context.Customers.Add(account);
                    await _context.SaveChangesAsync();

                    var verificationLink = Url.Action("VerifyEmail", "Auth",
                        new { userId = account.Id, token = GenerateEmailVerificationToken(account) }, Request.Scheme);
                    if (string.IsNullOrEmpty(verificationLink))
                    {
                        TempData["Message"] = "Failed to generate email verification link. Please try again.";
                        return View(model); 
                    }
                    await _emailService.SendVerificationEmail(account.Email, verificationLink);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, account.Name),
                        new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                        new Claim("DisplayPicture", account.Display_Picture),
                        new Claim(ClaimTypes.Role, "User"),
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    TempData["Message"] = "Registration successful. Please verify your email.";
                    return RedirectToAction("VerifyNotice");
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("Email", "Please enter a valid email address."); 
                    return View(model);
                }

            }
            return View(model);
        }

       

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = _context.Customers.FirstOrDefault(x => x.Username == model.Username);

                    if (user != null)
                    {
                        var passwordHasher = new PasswordHasher<Customer>();
                        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

                        if (passwordVerificationResult == PasswordVerificationResult.Success)
                        {
                            var displayPicture = user.Display_Picture ?? "images/profile-user.png";

                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, user.Name),
                                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                                new Claim("DisplayPicture", displayPicture),
                                new Claim(ClaimTypes.Role, "User"),
                            };

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                            TempData["Message"] = "You have successfully logged in!";

                            if (!user.IsVerified)
                            {
                                var verificationLink = Url.Action("VerifyEmail", "Auth",
                                    new { userId = user.Id, token = GenerateEmailVerificationToken(user) }, Request.Scheme);

                                if (string.IsNullOrEmpty(verificationLink))
                                {
                                    TempData["Message"] = "Failed to generate email verification link. Please try again.";
                                    return View(model);
                                }

                                await _emailService.SendVerificationEmail(user.Email, verificationLink);
                                TempData["Message"] = "Please verify your email. A new verification email has been sent.";
                                return RedirectToAction("VerifyNotice");
                            }

                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError("Password", "Your password is incorrect. Please try again.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Username", "Username is invalid. Please try again.");
                    }
                }
            }
            catch (SqlNullValueException ex)
            {
                ModelState.AddModelError("", $"A required field is missing. Please contact support. Error: {ex.Message}");
            }

            return View(model);
        }

        private string GenerateEmailVerificationToken(Customer account)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(account.Email));
        }

        [HttpGet]
        public IActionResult VerifyNotice()
        {
            return View();
        }
         
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string userId, string token)
        {
            if (!int.TryParse(userId, out int userIdInt))
            {
                ModelState.AddModelError("", "Invalid user ID.");
                return View();
            }

            var user = await _context.Customers.FindAsync(userIdInt);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View();
            }

            if (!VerifyEmailVerificationToken(token, user.Email))
            {
                ModelState.AddModelError("", "Invalid or expired verification link.");
                return View();
            }

            user.IsVerified = true;
            user.Email_verified_at = DateTime.UtcNow;

            try
            {
                _context.Customers.Update(user);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Your email has been successfully verified!";
                return RedirectToAction("Index", "Home");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Error updating user verification status. Please try again later.");
                return View();
            }
        }
        private bool VerifyEmailVerificationToken(string token, string userEmail)
        {
            string decodedEmail = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
            return decodedEmail == userEmail;
        }

        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult Index()
        {
            ViewBag.Name = HttpContext.User.Identity?.Name;
            return View();
        }
    }
}
