using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using cce106_palit.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using cce106_palit.Entity;
using System.Text.RegularExpressions;
using cce106_palit.Models;
using cce106_palit.Services;
using MailKit.Search;
using Azure.Identity;

namespace cce106_palit.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AccountController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Profile()
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out var customerId))
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return Unauthorized();
            }

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(int customerId, string Username, string Name, string Address, string Contact_Number)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            customer.Username = Username;
            customer.Name = Name;
            customer.Contact_Number = Contact_Number;
            customer.Address = Address;

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Profile updated successfully!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Profile", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeDisplayPicture(int customerId, IFormFile DisplayPicture)
        {
            if (DisplayPicture == null || DisplayPicture.Length == 0)
            {
                ModelState.AddModelError("", "Please select a valid image.");
                return RedirectToAction("Profile");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".jfif" };
            var extension = Path.GetExtension(DisplayPicture.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("", "Only image files are allowed.");
                return RedirectToAction("EditProfile");
            }

            var customer = await _context.Customers.FindAsync(customerId);

            if (customer == null)
            {
                return NotFound();
            }

            var fileName = $"{customer.Username}_profile{extension}";

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "display_pictures", fileName);

            var directory = Path.GetDirectoryName(filePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }


            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await DisplayPicture.CopyToAsync(stream);
            }

            customer.Display_Picture = $"images/display_pictures/{fileName}";
            _context.Update(customer);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Display picture changed successfully!";

            var claimsIdentity = User.Identity as ClaimsIdentity;

            if (claimsIdentity != null)
            {
                var existingClaim = claimsIdentity.FindFirst("DisplayPicture");
                if (existingClaim != null)
                {
                    claimsIdentity.RemoveClaim(existingClaim);
                }
                claimsIdentity.AddClaim(new Claim("DisplayPicture", customer.Display_Picture));
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            }
            return RedirectToAction("Profile");
        }


        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPatch]
        [Route("Account/ChangePassword/{customerId}")]
        public async Task<IActionResult> ChangePassword(int customerId, [FromBody] AccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data." });
            }

            if (string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.Confirm))
            {
                return Json(new { success = false, message = "Please input all fields." });
            }

            if (model.Password != model.Confirm)
            {
                return Json(new { success = false, field = "password", message = "Passwords do not match." });
            }

            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");
            if (!passwordRegex.IsMatch(model.Password))
            {
                return Json(new
                {
                    success = false,
                    field = "password",
                    message = "Password must be at least 8 characters and include uppercase, lowercase, and a number."
                });
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            var hasher = new PasswordHasher<Customer>();
            customer.Password = hasher.HashPassword(customer, model.Password);

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Password changed successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> ChangeEmail()
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out var customerId))
            {
                return BadRequest("Customer ID not found in claims.");
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            return View(customer);
        }

        [HttpPatch]
        [Route("Account/ChangeEmail/{customerId}")]
        public async Task<IActionResult> ChangeEmail(int customerId, [FromBody] AccountViewModel model)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data." });
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return Json(new { success = false, field="email", message = "Email is required." });
            }

            if (model.Email == customer.Email)
            {
                return Json(new { success = false, field = "email", message = "This is already your current email. If you wish to change your email, please enter a new one." });
            }

            if (await _context.Customers.AnyAsync(c => c.Email == model.Email))
            {
                return Json(new { success = false, field = "email", message = "This email is already in use." });
            }

            customer.Email = model.Email;
            customer.IsVerified = false;
            customer.VerificationToken = Guid.NewGuid().ToString();

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            var verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify?token={customer.VerificationToken}";

            // Send email  
            await _emailService.SendEmailAsync(customer.Email, "Verify Your Email - PALIT",
                $@"<p>Dear {customer.Name},</p>  
                <p>We noticed that you've requested a change to your email address on <strong>PALIT</strong>.</p>  

                <p>To complete the process and verify your new email address, please click the link below:</p>  

                <p><a href='{verificationLink}' style='color: blue; font-weight: bold;'>Verify My Email</a></p>  

                <p>If you did not request this change or you believe this was a mistake, please ignore this email.</p>  

                <p>Thank you for using PALIT!</p>  

                <p>Best regards,</p>  
                <p><strong>The PALIT Team</strong></p>");

            return Json(new { success = true, field = "email", message = "Your email has been updated successfully! A verification email has been sent to your new address. Please check your inbox to verify your email." });
        }

        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateAccount(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);

            if (customer == null)
            {
                return NotFound();
            }

            var hasActiveOrders = await _context.Orders.AnyAsync(o => o.Customer_Id == customerId && o.Status_Id != 4 && o.Status_Id != 5);

            if (hasActiveOrders)
            {
                TempData["Message"] = "You cannot deactivate your account because you still have pending transactions.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Settings");
            }

            customer.Is_Deactivated = true;
            await _context.SaveChangesAsync();
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);

            if (customer == null)
            {
                return NotFound();
            }

            var hasActiveOrders = await _context.Orders.AnyAsync(o => o.Customer_Id == customerId && o.Status_Id != 4 && o.Status_Id != 5);

            if(hasActiveOrders)
            {
                TempData["Message"] = "You cannot delete your account because you still have pending transactions.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Settings");
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out var customerId))
            {
                return BadRequest("Customer ID not found in claims.");
            }

            var orders = await _context.Orders
                .Where(o => o.Customer_Id == customerId)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int orderId)
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out var customerId))
            {
                return NotFound(); ;
            }

            var order = await _context.Orders.FirstOrDefaultAsync(
                 o => o.Id == orderId && o.Customer_Id == customerId
            );

            if (order == null)
            {
                return NotFound();
            }

            order.Status_Id = 4;
            await _context.SaveChangesAsync();

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null && !string.IsNullOrEmpty(customer.Email))
            {
                string subject = $"Order #{order.Id} Completed";
                string body = $@"
                    <h2>Hi {customer.Name},</h2>
                    <p>We're happy to let you know that your order <strong>#{orderId}</strong> has been successfully delivered and completed.</p>
                    <p>Thank you for shopping with PALIT!</p>
                    <br/>
                    <p>- The PALIT Team</p>";

                await _emailService.SendEmailAsync(customer.Email, subject, body);

            }

            TempData["Message"] = "Order has been received. Thank you for shopping with us!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Orders"); 
        }

        public async Task<IActionResult> TrackOrder(int id)
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out var customerId))
            {
                return NotFound(); ;
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payments) 
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id && o.Customer_Id == customerId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
