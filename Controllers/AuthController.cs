using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using System.Data.SqlTypes;
using Microsoft.AspNetCore.Authentication.Google;
using Newtonsoft.Json;
using cce106_palit.Data;
using cce106_palit.Services;
using cce106_palit.Models.Auth;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using cce106_palit.Entity;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace cce106_palit.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();

        public AuthController(ApplicationDbContext context, EmailService emailService, IMemoryCache cache)
        {
            _context = context;
            _emailService = emailService;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrationAsync(RegistrationViewModel model)
        {
          

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("Email", "Email and password are required.");
                return View();
            }

            try
            {
                var host = model.Email.Split('@')[1];
                var ip = Dns.GetHostEntry(host);
            }
            catch
            {
                ModelState.AddModelError("Email", "Invalid or unreachable email domain. Please enter a valid email address.");
                return View(model);
            }

            bool emailExists = _context.Customers.Any(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email is already registered. Please log in.");
                return View();
            }
            else 
            {
                // Generate Username
                string sanitizedUsername = model.Name.ToLower().Replace(" ", "");
                string generatedUsername = $"{sanitizedUsername}{new Random().Next(100, 999)}";
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
            }

            return RedirectToAction("VerifyNotice");
        }
       
        public async Task GoogleRegister()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
               new AuthenticationProperties
               {
                   RedirectUri = Url.Action("GoogleResponseRegister")
               });
        }

        public async Task<IActionResult> GoogleResponseRegister()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result?.Principal == null)
                return RedirectToAction("Index", "Home");

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                TempData["Message"] = "Google login failed. Please try again.";
                return RedirectToAction("Login", "Auth");
            }

            var user = _context.Customers.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                string sanitizedUsername = name.ToLower().Replace(" ", "");
                string generatedUsername = $"{sanitizedUsername}{new Random().Next(100, 999)}";

                string randomPassword = GenerateRandomPassword();
                string hashedPassword = _passwordHasher.HashPassword(null, randomPassword);
                user = new Customer
                {
                    Username = generatedUsername,
                    Name = name,
                    Email = email,
                    Password = hashedPassword, 
                    IsVerified = true,
                };

                _context.Customers.Add(user);
                await _context.SaveChangesAsync();
            }

            await _emailService.SendEmailAsync(user.Email, "Welcome to PALIT - Your Account is Ready!",
                 $@"<p>Dear {user.Name},</p>  

                <p>Welcome to <strong>PALIT</strong>! Your account has been successfully created using Google.</p>  

                <p>You can now log in anytime using your Google account and start shopping.</p>  

                <p>If you did not create this account, please contact our support team immediately.</p>  

                <p>Need assistance? We're here to help!</p>  

                <p>Happy shopping!</p>  

                <p>Best regards,</p>  
                <p><strong>The PALIT Team</strong></p>");

            var displayPicture = user.Display_Picture ?? "images/profile-user.png";

            // Proceed with login
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("DisplayPicture", displayPicture),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            // Store session details
            HttpContext.Session.SetString("UserClaims", JsonConvert.SerializeObject(claims.Select(c => new { c.Type, c.Value })));
            HttpContext.Session.SetString("UserEmail", email);
            HttpContext.Session.SetString("UserName", name ?? "User");

            return RedirectToAction("ProfileSetup");
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    ModelState.AddModelError("Email", "Email is required.");
                    return View();
                }
                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("Password", "Password is required.");
                    return View();
                }

                try
                {
                    var host = model.Email.Split('@').Last();
                    var mxRecords = Dns.GetHostEntry(host).AddressList;
                    if (mxRecords == null || mxRecords.Length == 0)
                    {
                        ModelState.AddModelError("Email", "The email domain is not valid.");
                        return View();
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("Email", "The email domain is not valid or could not be resolved.");
                    return View();
                }

                var user = _context.Customers.FirstOrDefault(u => u.Email == model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("Email", "These credentials do not match our records.");
                    return View();
                }

                if (user.Is_Deactivated)
                {
                    user.Is_Deactivated = false;
                    _context.SaveChanges();
                }

                if (user.IsVerified == false)
                {
                    ModelState.AddModelError("Email", "Please verify your email before logging in.");
                    return RedirectToAction("VerifyNotice");
                }

                var cacheKey = $"LoginAttempts_{model.Email}";
                if (_cache.TryGetValue<int>(cacheKey, out var failedAttempts) && failedAttempts >= 3)
                {
                    ViewBag.IsLockedOut = true;
                    ModelState.AddModelError("", "Too many attempts. Try again later.");
                    return View();
                }
                else
                {
                    ViewBag.IsLockedOut = false;
                }


                var result = _passwordHasher.VerifyHashedPassword(null, user.Password, model.Password);
                var displayPicture = user.Display_Picture ?? "images/profile-user.png";
    
                if (result == PasswordVerificationResult.Success)
                {
                    _cache.Remove(cacheKey);

                    _ = _emailService.SendEmailAsync(user.Email, "Login Notification",
                        $"<p>Your PALIT account was successfully accessed on {DateTime.Now}. " +
                        $"If this wasn’t you, please change your password immediately.</p>" +
                        "<p><strong> The PALIT Team </strong ></p>");

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("DisplayPicture", displayPicture),
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    };

                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    TempData["message"] = "Welcome to PALIT!";
                    TempData["messageType"] = "success";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    failedAttempts++;
                    _cache.Set(cacheKey, failedAttempts, TimeSpan.FromMinutes(5));
                    _ = _emailService.SendEmailAsync(user.Email, "Failed Login Attempt",
                        "<p>Someone tried to log in to your PALIT account but failed. If this wasn't you, change your password immediately.</p>" +
                        "<p><strong> The PALIT Team </strong ></p>");
                    ModelState.AddModelError("Password", "The password you have entered is incorrect. Forgot password?");
                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                return View();
            }
        }



        [HttpGet]
        public IActionResult VerifyNotice()
        {
            return View();
        }

        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result?.Principal == null)
                return RedirectToAction("Index", "Home");

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                TempData["Message"] = "Google login failed. Please try again.";
                return RedirectToAction("Login", "Auth");
            }

            var user = _context.Customers.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                string sanitizedUsername = name.ToLower().Replace(" ", "");
                string generatedUsername = $"{sanitizedUsername}{new Random().Next(100, 999)}";

                string randomPassword = GenerateRandomPassword();
                string hashedPassword = _passwordHasher.HashPassword(null, randomPassword);

                user = new Customer
                {
                    Username = generatedUsername,
                    Password = hashedPassword,
                    Name = name,
                    Email = email,
                    IsVerified = true,
                    VerificationToken = null,
                    Display_Picture = "images/profile-user.png",

                };

                _context.Customers.Add(user);
                await _context.SaveChangesAsync();
            }


            if (user.IsVerified == false)
            {
                ModelState.AddModelError("Email", "Please verify your email before logging in.");
                return RedirectToAction("VerifyNotice");
            }

            var displayPicture = user.Display_Picture ?? "images/profile-user.png";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("DisplayPicture", displayPicture)
            };

            await _emailService.SendEmailAsync(user.Email, "Welcome to PALIT!",
                $@"<p>Hi {user.Name},</p>
                <p>Your account has been successfully created via Google Login on <strong>PALIT</strong>.</p>

                <p>You can log in anytime using your Google account associated with this email: <strong>{user.Email}</strong>.</p>

                <p>If you want to set a manual password, go to your account settings after logging in and choose “Change Password.”</p>

                <p>Happy shopping!</p>
                <p>– The PALIT Team</p>");


            // Proceed with login
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            if(user.Contact_Number == null || user.Address == null)
            {
                return RedirectToAction("ProfileSetup");

            }
            else
            {
                return RedirectToAction("Index", "Home");

            }


        }

        [HttpPost]
        public async Task<IActionResult> ResendVerification(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["message"] = "Please enter your email.";
                TempData["messageType"] = "error";
                return RedirectToAction("VerifyNotice");
            }

            var user = await _context.Customers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["message"] = "Email not found. Please register first.";
                TempData["messageType"] = "error";
                return RedirectToAction("Registration");
            }

            if (user.IsVerified == true)
            {
                TempData["message"] = "This email is already verified. Please log in.";
                TempData["messageType"] = "info";
                return RedirectToAction("Login");
            }

            // Generate a new verification link
            user.VerificationToken = Guid.NewGuid().ToString();
            _context.Customers.Update(user);
            await _context.SaveChangesAsync();

            var verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify?token={user.VerificationToken}";

            await _emailService.SendEmailAsync(user.Email, "Verify Your Email - PALIT",
                $@"<p>Dear {user.Name},</p>  
                <p>We noticed that you requested a new verification email for your <strong>PALIT</strong> account.</p>  

                <p>To complete your registration and gain access to all features, please verify your email address by clicking the link below:</p>  

                <p><a href='{verificationLink}' style='color: blue; font-weight: bold;'>Verify My Email</a></p>  

                <p>If you did not request this verification email, you can ignore this message. Your account will remain inactive until verification is completed.</p>  

                <p>For any issues or questions, feel free to contact our support team.</p>  

                <p>Best regards,</p>  
                <p><strong>The PALIT Team</strong></p>");


            TempData["message"] = "A new verification email has been sent. Please check your inbox.";
            TempData["messageType"] = "success";
            return RedirectToAction("VerifyNotice");
        }

        [HttpGet]
        public IActionResult ProfileSetup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProfileSetup(string contactNumber, string address)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Customers.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            user.Contact_Number = contactNumber;
            user.Address = address;

            _context.Customers.Update(user);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Welcome aboard! Let's start shopping!";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            var customer = _context.Customers.FirstOrDefault(x => x.Email == email);

            if (customer == null)
            {
                TempData["email"] = "Email not found.";
                return View();
            }

            var token = Guid.NewGuid().ToString();
            customer.PasswordResetToken = token;
            customer.ResetTokenExpiry = DateTime.Now.AddMinutes(15);

            _context.SaveChanges();

            var resetLink = Url.Action("ResetPassword", "Auth", new { token = token }, Request.Scheme);

            _ = _emailService.SendEmailAsync(customer.Email, "Reset Password",
               $@"<p>Dear {customer.Name},</p>  
                <p>We noticed that you requested to reset your password for your <strong>PALIT</strong> account.</p>  

                <p>To reset your password, please click the link below:</p>  

                <p><a href='{resetLink}' style='color: blue; font-weight: bold;'>Reset My Password</a></p>  

                <p>This link will expire in 15 minutes. If you did not request a password reset, please ignore this email. Your account will remain secure.</p>  

                <p>If you need further assistance, feel free to contact our support team.</p>  

                <p>Best regards,</p>  
                <p><strong>The PALIT Team</strong></p>");

            TempData["email"] = "Reset link has been sent to your email. Please check your inbox.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Message"] = "Reset token is required.";
                TempData["MessageType"] = "error";
                return RedirectToAction("ForgotPassword");
            }

            var customer = _context.Customers.FirstOrDefault(x => x.PasswordResetToken == token);
            if (customer == null || customer.ResetTokenExpiry < DateTime.Now)
            {
                TempData["Message"] = "Invalid or expired reset token. Please try again.";
                TempData["MessageType"] = "error";
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.ResetToken = token;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string token, string password, string confirmPassword)
        {
            var customer = _context.Customers.FirstOrDefault(x => x.PasswordResetToken == token);
            if (customer == null || customer.ResetTokenExpiry < DateTime.Now)
            {
                TempData["Message"] = "Invalid or expired reset token. Please try again.";
                TempData["MessageType"] = "error";
                return RedirectToAction("ForgotPassword");
            }

            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");

            if (string.IsNullOrEmpty(password) || !passwordRegex.IsMatch(password))
            {
                TempData["password"] = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one number.";
                ViewBag.ResetToken = token;
                return View();
            }

            if (password != confirmPassword)
            {
                TempData["password"] = "Passwords do not match.";
                TempData["MessageType"] = "error";
                ViewBag.ResetToken = token;
                return View();
            }

            customer.Password = _passwordHasher.HashPassword(null, password);
            customer.PasswordResetToken = null;
            customer.ResetTokenExpiry = null;

            _context.Customers.Update(customer);
            _context.SaveChanges();

            TempData["Message"] = "Your password has been successfully reset. You can now log in with your new password.";
            TempData["MessageType"] = "success";
            return RedirectToAction("Login");
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            using var rng = new RNGCryptoServiceProvider();
            var buffer = new byte[12];
            rng.GetBytes(buffer);
            return new string(buffer.Select(b => chars[b % chars.Length]).ToArray());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // 1. Sign out from authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Clear all session data
            HttpContext.Session.Clear();

            // 3. Expire the authentication cookie
            Response.Cookies.Delete(CookieAuthenticationDefaults.AuthenticationScheme);

            // 4. Clear any other custom cookies you might have set
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            // 5. Reset the user context
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // 6. Add cache control headers to prevent caching
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return RedirectToAction("Login");
        }

    }
}
