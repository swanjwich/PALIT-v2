using cce106_palit.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using cce106_palit.Entity;
using MailKit.Search;

namespace cce106_palit.Areas.Admin.Controllers
{
    [Authorize(Roles = "Super Admin, Admin, Inventory Manager")]
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("admin/category")]
        [HttpGet]
        public IActionResult Index(int? page, string? searchTerm)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;

            var query = _context.Categories.Where(c => !c.Is_Deleted).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm));
            }

            var categories = query.OrderBy(c => c.Id)
                                  .ToPagedList(pageNumber, pageSize);

            ViewBag.Page = pageNumber;
            ViewBag.SearchTerm = searchTerm;

            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string Name, string Description, IFormFile Image_url)
        {
            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == Name.ToLower());

            if (categoryExists)
            {
                TempData["Message"] = "A category with this name already exists.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            var category = new Category
            {
                Name = Name,
                Description = Description,
                Created_at = DateTime.Now,
                Updated_at = DateTime.Now,
            };

            if(Image_url != null && Image_url.Length > 0)
            {
                var sanitizedFileName = string.Concat(Name.ToLower().Replace(" ", "-").Split(Path.GetInvalidFileNameChars()));
                var fileExtension = Path.GetExtension(Image_url.FileName);
                var fileName = $"{sanitizedFileName}{fileExtension}";

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/categories", fileName);
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Image_url.CopyToAsync(stream);
                }

                category.Image_url = $"images/categories/{fileName}";
            }

            _context.Categories.Add(category);
            _context.SaveChanges();

            var totalBefore = await _context.Categories
                .Where(c => c.Id < category.Id)
                .CountAsync();

            int pageSize = 8;
            int page = (totalBefore / pageSize) + 1;

            TempData["Message"] = "Category successfully added!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Index", new { page });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string Name, string Description, IFormFile Image_url, int Id, int page = 1)
        {
            var category = await _context.Categories.FindAsync(Id);
            if (category == null)
            {
                return NotFound();
            }

            bool duplicateExists = await _context.Categories
            .AnyAsync(p => p.Id != Id && p.Name.ToLower() == Name.ToLower());

            if (duplicateExists)
            {
                TempData["Message"] = "Another category with this name already exists.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            category.Name = Name;
            category.Description = Description;
            category.Updated_at = DateTime.Now;

            if (Image_url != null && Image_url.Length > 0)
            {
                var sanitizedFileName = string.Concat(Name.ToLower().Replace(" ", "-").Split(Path.GetInvalidFileNameChars()));
                var fileExtension = Path.GetExtension(Image_url.FileName);
                var fileName = $"{sanitizedFileName}{fileExtension}";

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/categories", fileName);
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Image_url.CopyToAsync(stream);
                }

                category.Image_url = $"images/categories/{fileName}";
            }

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Category No. {category.Id} has been successfully updated!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Index", new { page });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                        .Include(c => c.Products)
                        .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            category.Is_Deleted = true;

            foreach (var product in category.Products)
            {
                product.Is_Deleted = true;
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Category '{category.Name}' and all its products have been successfully deleted!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Index");
        }

    }
}
