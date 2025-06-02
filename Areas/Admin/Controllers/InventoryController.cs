using cce106_palit.Data;
using cce106_palit.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace cce106_palit.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Super Admin, Admin, Inventory Manager")]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("admin/inventory")]
        [HttpGet]
        public IActionResult Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var stockins = _context.StockIns
                .Include(s => s.Product)
                .ToPagedList(pageNumber, pageSize);

            return View(stockins);
        }

        [HttpGet]
        public IActionResult StockIn(string searchTerm, int? page, int? category)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var productsQuery = _context.Products
                .Where(p => !p.Is_Deleted)
                .Include(p => p.Category)
                .AsQueryable(); 

            if (!string.IsNullOrEmpty(searchTerm))
            {
                productsQuery = productsQuery.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()));
            }

            if (category.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Category_id == category.Value);
            }

            var products = productsQuery.OrderBy(p => p.Id).ToPagedList(pageNumber, pageSize);

            ViewBag.Categories = _context.Categories.Where(c => !c.Is_Deleted).ToList();
            ViewBag.Page = pageNumber;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategoryId = category;

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(List<int> selectedProductIds, List<int> stockAmounts, List<DateOnly> expirationDates, string remarks)
        {
            if (selectedProductIds == null || stockAmounts == null || expirationDates == null || !selectedProductIds.Any() || !stockAmounts.Any() || !expirationDates.Any())
            {
                TempData["Message"] = "Please select a product and enter an amount first.";
                TempData["MessageType"] = "info";
                return RedirectToAction("StockIn");
            }

            if (selectedProductIds.Count != stockAmounts.Count)
            {
                TempData["Message"] = "Mismatch between selected products and stock amounts.";
                TempData["MessageType"] = "error";
                return RedirectToAction("StockIn");
            }

            // 1. Get the latest batch number from existing records
            int latestBatchNum = 0;

            var latestBatch = await _context.StockIns
                .OrderByDescending(s => s.Id)
                .Select(s => s.BatchNumber)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(latestBatch) && latestBatch.StartsWith("BATCH-"))
            {
                if (int.TryParse(latestBatch.Replace("BATCH-", ""), out int batchNum))
                {
                    latestBatchNum = batchNum;
                }
            }

            // 2. Create new batch number
            string newBatchNumber = $"BATCH-{latestBatchNum + 1}";

            // 3. Process each selected product with the same batch number
            for (int i = 0; i < selectedProductIds.Count && i < stockAmounts.Count && i < expirationDates.Count ; i++)
            {
                if (stockAmounts[i] > 0)
                {
                    var stock = new StockIn
                    {
                        ProductId = selectedProductIds[i],
                        Amount = stockAmounts[i],
                        ExpirationDate = expirationDates[i],
                        DateReceived = DateTime.Now,
                        BatchNumber = newBatchNumber,
                        Remarks = remarks,
                        ReceivedBy = User.Identity.Name
                    };

                    _context.StockIns.Add(stock);

                    var product = await _context.Products.FindAsync(selectedProductIds[i]);
                    if (product != null)
                    {
                        product.StockQuantity += stockAmounts[i];
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Stocks successfully recorded!";
            TempData["MessageType"] = "success";
            return RedirectToAction("StockIn");
        }



    }
}
