using it15_palit.Data;
using it15_palit.Entity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace it15_palit.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Profile()
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

        [HttpPost]
        public async Task<IActionResult> EditProfile(int customerId, string Name, string Email, string Address, string Contact_Number)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            customer.Name = Name;
            customer.Email = Email;
            customer.Contact_Number = Contact_Number;
            customer.Address = Address;

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Profile updated successfully!";

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


        [HttpPost]
        public async Task<IActionResult> ChangePassword(int customerId, string Password, string Confirm)
        {
            if (Password != Confirm)
            {
                TempData["Message"] = "Passwords do not match.";
                return RedirectToAction("Profile", "Account");
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            customer.Password = Password;

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Password changed successfully!";

            return RedirectToAction("Profile");
        }

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
    }
}
