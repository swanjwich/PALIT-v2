using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using cce106_palit.Data;
using Microsoft.AspNetCore.Identity;
using cce106_palit.Entity;
using Microsoft.EntityFrameworkCore;

namespace cce106_palit.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("admin")]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Username, string Password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == Username);

            if (user != null)
            {
                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(null, user.Password, Password);

                if (passwordVerificationResult == PasswordVerificationResult.Success)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, user.Role ?? "Super Admin"),
                        new Claim(ClaimTypes.Email, user.Email ?? "NO Email"),
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    return RedirectToAction("Index", "Dashboard");
                }
            }

            TempData["Message"] = "Invalid username or password.";
            TempData["MessageType"] = "error.";
            return RedirectToAction("Login");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
