using cce106_palit.Data;
using cce106_palit.Entity;
using cce106_palit.Services;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace cce106_palit.Areas.Admin.Controllers
{
    [Authorize(Roles = "Super Admin, Admin, Staff")]
    [Area("Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public OrdersController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [Route("admin/orders")]
        [HttpGet]
        public IActionResult Orders(int? page, string? searchTerm)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var query = _context.Orders
                        .Include(o => o.Customer)
                        .Include(o => o.OrderDetails)
                        .Include(o => o.Status)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (int.TryParse(searchTerm, out int orderId))
                {
                    query = query.Where(c => c.Id == orderId);
                }
            }

            var orders = query
                .OrderByDescending(o => o.OrderDate)
                .ToPagedList(pageNumber, pageSize);


            ViewBag.Statuses = _context.Statuses.ToList();
            ViewBag.SearchTerm = searchTerm;
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int OrderId, int StatusId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == OrderId);

            if (order == null)
            {
                return NotFound();
            }

            order.Status_Id = StatusId;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            var customer = order.Customer;

            if (customer != null && !string.IsNullOrEmpty(customer.Email))
            {
                var (subject, body) = GetEmailContentByStatus(StatusId, customer.Name, order.Id);
                await _emailService.SendEmailAsync(customer.Email, subject, body);

            }

            TempData["message"] = "Order status updated successfully!";
            return RedirectToAction("Orders", "Orders"); 
        }

        [Route("admin/orders/details/{orderId}")]
        [HttpGet]
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

        private (string subject, string body) GetEmailContentByStatus(int statusId, string customerName, int orderId)
        {
            string subject;
            string body;

            switch (statusId)
            {
                case 1: // Pending
                    subject = $"Order #{orderId} is Pending";
                    body = $@"
                <h2>Hi {customerName},</h2>
                <p>Your order <strong>#{orderId}</strong> has been placed and is now pending confirmation.</p>
                <br/>
                <p>- The PALIT Team</p>";
                    break;

                case 2: // To Ship
                    subject = $"Order #{orderId} is Ready to Ship";
                    body = $@"
                <h2>Hi {customerName},</h2>
                <p>Good news! Your order <strong>#{orderId}</strong> is now being prepared for shipping.</p>
                <p>We'll notify you again once it's on the way!</p>
                <br/>
                <p>- The PALIT Team</p>";
                    break;

                case 3: // To Receive
                    subject = $"Order #{orderId} is On the Way!";
                    body = $@"
                <h2>Hi {customerName},</h2>
                <p>Your order <strong>#{orderId}</strong> is on its way to your delivery address.</p>
                <p>Please keep your phone nearby for delivery updates.</p>
                <br/>
                <p>- The PALIT Team</p>";
                    break;

                case 4: // Completed
                    subject = $"Order #{orderId} Completed";
                    body = $@"
                <h2>Hi {customerName},</h2>
                <p>We're happy to let you know that your order <strong>#{orderId}</strong> has been successfully delivered and completed.</p>
                <p>Thank you for shopping with PALIT!</p>
                <br/>
                <p>- The PALIT Team</p>";
                    break;

                case 5: // Cancelled
                    subject = $"Order #{orderId} Cancelled";
                    body = $@"
                <h2>Hi {customerName},</h2>
                <p>We're sorry to inform you that your order <strong>#{orderId}</strong> has been cancelled.</p>
                <p>If you believe this was a mistake, please contact our support team.</p>
                <br/>
                <p>- The PALIT Team</p>";
                    break;

                default:
                    subject = $"Order #{orderId} Update";
                    body = $@"
                <h2>Hi {customerName},</h2>
                <p>There has been an update to your order <strong>#{orderId}</strong>.</p>
                <br/>
                <p>- The PALIT Team</p>";
                    break;
            }

            return (subject, body);
        }

    }
}
