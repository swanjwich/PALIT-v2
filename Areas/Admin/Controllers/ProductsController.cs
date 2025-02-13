using it15_palit.Data;
using it15_palit.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace it15_palit.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Products(int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var products = _context.Products
                                   .Include(p => p.Category)
                                   .OrderBy(p => p.Id)
                                   .ToPagedList(pageNumber, pageSize);
            ViewBag.Categories = _context.Categories.ToList();
            return View(products);
        }


        [HttpPost]
        public async Task<IActionResult> Create(string Name, string Description, decimal Price, int Stock_quantity, int CategoryId, IFormFile image)
        {
            var product = new Product
            {
                Name = Name,
                Description = Description,
                Price = Price,
                Stock_quantity = Stock_quantity,
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
            TempData["message"] = "Product successfully added!";
            return RedirectToAction("Products");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string Name, string Description, decimal Price, int CategoryId, IFormFile image, int Id)
        {
            var product = await _context.Products.FindAsync(Id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = Name;
            product.Description = Description;
            product.Price = Price;
            product.Category_id = CategoryId;

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

            TempData["message"] = "Product successfully updated!";
            return RedirectToAction("Products");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.Image_url))
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.Image_url);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["message"] = "Product successfully deleted!";
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
