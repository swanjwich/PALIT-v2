using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using cce106_palit.Entity;
using Microsoft.AspNetCore.Authorization;
using cce106_palit.Data;
using System.Globalization;

namespace cce106_palit.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("admin/dashboard")]
        public async Task<IActionResult> IndexAsync()
        {
            var totalSales = await GetTotalSales();
            var totalOrders = await GetTotalOrders();
            var totalCustomers = await GetTotalCustomers();
            var totalProducts = await GetTotalProducts();

            var latestOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new
                {
                    FullName = o.Customer.Name,
                    Email = o.Customer.Email,
                    OrderDate = o.OrderDate,
                    Total = o.Grand_Total
                })
            .ToListAsync();

            ViewBag.LatestOrders = latestOrders;

            ViewBag.TotalSales = totalSales;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.TotalProducts = totalProducts;
            return View();
        }

        public async Task<decimal> GetTotalSales()
        {
            var totalSales = await _context.Orders.SumAsync(o => o.Grand_Total);
            return totalSales;
        }

        public async Task<int> GetTotalOrders()
        {
            var totalOrders = await _context.Orders.CountAsync();
            return totalOrders;
        }

        public async Task<int> GetTotalCustomers()
        {
            var totalCustomers = await _context.Customers.CountAsync();
            return totalCustomers;
        }

        public async Task<int> GetTotalProducts()
        {
            var totalProducts = await _context.Products.Where(p => !p.Is_Deleted).CountAsync();
            return totalProducts;
        }

        [Route("admin/dashboard/GetMonthlySales")]
        [HttpGet]
        public IActionResult GetMonthlySales()
        {
            try
            {
                var monthlySales = _context.Orders
                    .Where(o => o.Status_Id == 4 && o.OrderDate != null)
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        TotalSales = g.Sum(x => x.Grand_Total)
                    })
                    .ToList() // switch to client-side here
                    .Select(g => new
                    {
                        Month = new DateTime(g.Year, g.Month, 1).ToString("MMM yyyy"),
                        g.TotalSales
                    })
                    .OrderBy(g => g.Month)
                    .ToList();

                return Json(monthlySales);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMonthlySales: {ex}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [Route("admin/dashboard/GetTopSellingProducts")]
        [HttpGet]
        public IActionResult GetTopSellingProducts()
        {
            var data = _context.OrderDetails
                .GroupBy(od => od.Product.Name)
                .Select(g => new {
                    ProductName = g.Key,
                    QuantitySold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5) // Top 5 products
                .ToList();

            return Json(data);
        }

        [Route("admin/dashboard/GetLatestOrders")]
        [HttpGet]
        public IActionResult GetLatestOrders()
        {
            try
            {
                var latestOrders = _context.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .Select((o) => new
                    {
                        OrderId = o.Id,
                        Name = o.Customer.Name,
                        Email = o.Customer.Email,
                        OrderDate = o.OrderDate.ToString("MMM dd, yyyy"),
                        Total = o.Grand_Total
                    })
                    .ToList();

                return Json(latestOrders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLatestOrders: {ex}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
