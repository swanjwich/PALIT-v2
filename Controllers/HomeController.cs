using it15_palit.Data;
using it15_palit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace it15_palit.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> SelectCategory(List<int> selectedCategoryIds)
        {
            return RedirectToAction("Shop", "Home", new { selectedCategoryIds });
        }


        [HttpGet]
        public async Task<IActionResult> Shop(List<int> selectedCategoryIds = null, decimal? minPrice = null, decimal? maxPrice = null, string? sortOrder = null, string? searchQuery = null)
        {
            var categories = await _context.Categories.ToListAsync();
            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            if (selectedCategoryIds != null && selectedCategoryIds.Any())
            {
                productsQuery = productsQuery.Where(p => selectedCategoryIds.Contains(p.Category_id));
            }

            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchQuery));
            }

            switch (sortOrder)
            {
                case "asc":
                    productsQuery = productsQuery.OrderBy(p => p.Price);
                    break;
                case "desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Price);
                    break;
                default:
                    break;
            }


            var products = await productsQuery.ToListAsync();

            var viewModel = new ShopViewModel
            {
                Categories = categories,
                Products = products,
                SelectedCategoryIds = selectedCategoryIds, 
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SearchQuery = searchQuery
            };



            return View(viewModel);
        }




        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
