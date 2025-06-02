using cce106_palit.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using cce106_palit.Entity;

namespace cce106_palit.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("admin/products")]
        [HttpGet]
        public IActionResult Products(int? page, string? searchTerm, int? categoryFilter)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var query = _context.Products
                        .Where(c => !c.Is_Deleted)
                        .Include(p => p.Category)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm));
            }

            if (categoryFilter.HasValue)
            {
                query = query.Where(p => p.Category_id == categoryFilter.Value);
            }

            var products = query.OrderBy(p => p.Id)
                            .ToPagedList(pageNumber, pageSize);

            ViewBag.Categories = _context.Categories.Where(c => !c.Is_Deleted).ToList();
            ViewBag.Page = pageNumber;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategoryId = categoryFilter;

            return View(products);
        }


        [HttpPost]
        public async Task<IActionResult> Create(string Name, string Description, decimal Price, int CategoryId, IFormFile image)
        {
            bool productExists = await _context.Products
                .AnyAsync(p => p.Name.ToLower() == Name.ToLower());

            if (productExists)
            {
                TempData["Message"] = "A product with this name already exists.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            var product = new Product
            {
                Name = Name,
                Description = Description,
                Price = Price,
                Category_id = CategoryId,
            };

            if (image != null && image.Length > 0)
            {
                string categoryFolder = GetCategoryFolder(CategoryId);
                if (categoryFolder == null)
                {
                    ModelState.AddModelError("", "Invalid category.");
                    return View();
                }

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", categoryFolder);
                Directory.CreateDirectory(uploadsFolder);

                string safeFileName = Path.GetFileNameWithoutExtension(image.FileName)
                                   .Replace(" ", "-")
                                   .ToLower();
                string uniqueFileName = $"{safeFileName}_{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                product.Image_url = $"images/products/{categoryFolder}/{uniqueFileName}";
            }
            _context.Products.Add(product);
            _context.SaveChanges();

            var totalBefore = await _context.Products
               .Where(c => c.Id < product.Id)
               .CountAsync();

            int pageSize = 10;
            int page = (totalBefore / pageSize) + 1;

            TempData["Message"] = "Product successfully added!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Products", new { page });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string Name, string Description, decimal Price, int CategoryId, IFormFile image, int Id, int page = 1)
        {
            var product = await _context.Products.FindAsync(Id);
            if (product == null)
            {
                return NotFound();
            }

            bool duplicateExists = await _context.Products
            .AnyAsync(p => p.Id != Id && p.Name.ToLower() == Name.ToLower());

            if (duplicateExists)
            {
                TempData["Message"] = "Another product with this name already exists.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Products");
            }

            product.Name = Name;
            product.Description = Description;
            product.Price = Price;
            product.Category_id = CategoryId;
            product.Updated_at = DateTime.Now;

            if (image != null && image.Length > 0)
            {
                string categoryFolder = GetCategoryFolder(CategoryId);
                if (categoryFolder == null)
                {
                    ModelState.AddModelError("", "Invalid category.");
                    return View(product);
                }

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", categoryFolder);
                Directory.CreateDirectory(uploadsFolder);

                string safeFileName = Path.GetFileNameWithoutExtension(image.FileName)
                                       .Replace(" ", "-")
                                       .ToLower();
                string uniqueFileName = $"{safeFileName}_{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                product.Image_url = $"images/products/{categoryFolder}/{uniqueFileName}";
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Product No. {product.Id} has been successfully updated!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Products", new { page });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            //if (!string.IsNullOrEmpty(product.Image_url))
            //{
            //    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.Image_url);
            //    if (System.IO.File.Exists(filePath))
            //    {
            //        System.IO.File.Delete(filePath);
            //    }
            //}

            product.Is_Deleted = true;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Product No. {product.Id} has been successfully deleted!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Products");
        }



        private string GetCategoryFolder(int categoryId)
        {
            switch (categoryId)
            {
                case 1: return "baby_care";
                case 2: return "snacks";
                case 3: return "beverages";
                case 4: return "alcoholic_beverages";
                case 5: return "personal_care";
                case 6: return "laundry_home_care";
                case 7: return "pet_care";
                case 8: return "canned_goods";
                case 9: return "fruits";
                case 10: return "vegetables";
                case 11: return "dairy";
                case 12: return "meat";
                case 13: return "condiments";
                case 14: return "bread";
                case 15: return "frozen";
                case 16: return "packaged";
                default: return null;
            }
        }

    }
}
