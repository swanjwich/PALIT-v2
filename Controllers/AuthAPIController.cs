using cce106_palit.Data;
using cce106_palit.Entity;
using cce106_palit.Models.Auth;
using cce106_palit.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace cce106_palit.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthApiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();
        private readonly EmailService _emailService;

        public AuthApiController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(m => m.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(new { message = "Validation failed", errors });
            }

            bool emailExists = _context.Customers.Any(u => u.Email == model.Email);
            if (emailExists)
            {
                return BadRequest(new { message = "Email already registered.", errors = new { Email = new[] { "Email already exists." } } });
            }
            else
            {

                // Generate Username
                string sanitizedUsername = model.Name.ToLower().Replace(" ", "");
                string generatedUsername = $"{sanitizedUsername}123";
                string hashedPassword = _passwordHasher.HashPassword(null, model.Password);

                var customer = new Customer
                {
                    Username = generatedUsername,
                    Password = hashedPassword,
                    Name = model.Name,
                    Email = model.Email,
                    IsVerified = false,
                    VerificationToken = Guid.NewGuid().ToString(),
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Generate verification link
                var verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify?token={customer.VerificationToken}";

                // Send email  
                await _emailService.SendEmailAsync(customer.Email, "Verify Your Email - PALIT",
                    $@"<p>Dear {customer.Name},</p>  
                    <p>Thank you for signing up at <strong>PALIT</strong>! Before you can access your account, we need to verify your email address.</p>  

                    <p>Please click the link below to complete your email verification:</p>  

                    <p><a href='{verificationLink}' style='color: blue; font-weight: bold;'>Verify My Email</a></p>  

                    <p>If you did not sign up for a PALIT account, please ignore this email.</p>  

                    <p>Happy shopping!</p>  

                    <p>Best regards,</p>  
                    <p><strong>The PALIT Team</strong></p>");


                return Ok(new { message = "User registered successfully! Please verify your email." });
            }

        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var user = await _context.Customers.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if (user == null) return NotFound(new { message = "Invalid or expired token." });

            user.IsVerified = true;
            user.VerificationToken = null;
            _context.Customers.Update(user);
            await _context.SaveChangesAsync();

            var displayPicture = user.Display_Picture ?? "images/profile-user.png";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("DisplayPicture", displayPicture)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return RedirectToAction("ProfileSetup", "Auth");
        }

    }
}
