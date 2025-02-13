using Microsoft.AspNetCore.Mvc;
using it15_palit.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using it15_palit.Entity;

namespace it15_palit.Area.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var totalSales = await GetTotalSales();
            var totalOrders = await GetTotalOrders();
            var totalCustomers = await GetTotalCustomers();
            var totalProducts = await GetTotalProducts();

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
            var totalProducts = await _context.Products.CountAsync();
            return totalProducts;
        }

      


    }
}
