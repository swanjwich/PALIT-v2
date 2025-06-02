using cce106_palit.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace cce106_palit.Areas.Admin.Controllers
{
    [Authorize(Roles = "Super Admin, Admin, Staff")]
    [Area("Admin")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("admin/customers")]
        [HttpGet]
        public IActionResult Index(int? page, string searchTerm)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm)); 
            }

            var customers = query.OrderBy(c => c.Id)
                                  .ToPagedList(pageNumber, pageSize);

            ViewBag.Page = pageNumber;
            ViewBag.SearchTerm = searchTerm;

            return View(customers);
        }


        [Route("admin/customer/details/{customerId}")]
        [HttpGet]
        public IActionResult Details(int? customerId, int? page)
        {
            if (customerId == null)
            {
                return BadRequest("Customer ID is required.");
            }

            int pageSize = 10; 
            int pageNumber = page ?? 1; 

            var orders = _context.Orders
                .Where(o => o.Customer_Id == customerId)
                .Include(o => o.Status)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.Statuses = _context.Statuses.ToList();
            ViewBag.Customer = _context.Customers.FirstOrDefault(c => c.Id == customerId);

            return View(orders);
        }


    }
}
