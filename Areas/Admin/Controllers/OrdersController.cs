using it15_palit.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace it15_palit.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Orders(int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .Include(o => o.Status)
                .OrderBy(o => o.OrderDate)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.Statuses = _context.Statuses.ToList();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int OrderId, int StatusId)
        {
            var order = await _context.Orders.FindAsync(OrderId);
            if (order == null)
            {
                return NotFound();
            }

            order.Status_Id = StatusId;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            TempData["message"] = "Order status updated successfully!";
            return RedirectToAction("Orders", "Orders"); 
        }

        public IActionResult OrderDetails(int? orderId, int? page)
        {
            if (orderId == null)
            {
                return BadRequest("Order ID is required.");
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;

            var orders = _context.OrderDetails
                .Include(o => o.Product)
                .Include(od => od.Order)
                .Where(od => od.Order_Id == orderId)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.Statuses = _context.Statuses.ToList();
            return View(orders);
        }
    }
}
