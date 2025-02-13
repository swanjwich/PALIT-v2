using it15_palit.Data;
using it15_palit.Entity;
using it15_palit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace it15_palit.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PayMongoService _payMongoService;

        public ShopController(ApplicationDbContext context, PayMongoService payMongoService)
        {
            _context = context;
            _payMongoService = payMongoService;
        }


        public IActionResult ProductDetails(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.ProductId = id;
            ViewBag.ProductName = product.Name;
            ViewBag.ProductDescription = product.Description;
            ViewBag.ProductPrice = product.Price;
            ViewBag.ProductImage = product.Image_url;

            return View();
        }

        [HttpGet]
        public IActionResult Cart()
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(nameIdentifierClaim, out int customerId))
            {
                var cartItems = _context.Carts
                    .Include(c => c.Product)
                    .Where(c => c.Customer_Id == customerId)
                    .ToList();

                return View(cartItems);
            }
            else
            {
                TempData["Message"] = "Unable to access cart. Please log in again.";
                return RedirectToAction("Login", "Auth");
            }
        }

        [HttpPost]
        public IActionResult AddToCart(string customerId, int productId, int quantity)
        {
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = new Cart
            {
                Customer_Id = int.Parse(customerId),
                Product_Id = productId,
                Quantity = quantity,
                Is_Checked = false,
                Created_At = DateTime.Now,
                Updated_At = DateTime.Now
            };

            var existingCartItem = _context.Carts
                .FirstOrDefault(c => c.Customer_Id == cartItem.Customer_Id && c.Product_Id == cartItem.Product_Id);

            if (existingCartItem != null)
            {
                TempData["Message"] = "Item is already in your cart.";
            }
            else
            {
                _context.Carts.Add(cartItem);
                TempData["Message"] = "Item has been added to your cart.";
            }

            _context.SaveChanges();

            return RedirectToAction("ProductDetails", "Shop", new { id = productId });
        }

        [HttpPost]
        public IActionResult BuyNow(string customerId, int productId, int quantity)
        {
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = new Cart
            {
                Customer_Id = int.Parse(customerId),
                Product_Id = productId,
                Quantity = quantity,
                Is_Checked = true,
                Created_At = DateTime.Now,
                Updated_At = DateTime.Now
            };

            var existingCartItem = _context.Carts
                .FirstOrDefault(c => c.Customer_Id == cartItem.Customer_Id && c.Product_Id == cartItem.Product_Id);

            if (existingCartItem != null)
            {
                TempData["Message"] = "Item is already in your cart.";
            }
            else
            {
                _context.Carts.Add(cartItem);
                TempData["Message"] = "Item has been added to your cart.";
            }

            _context.SaveChanges();

            return RedirectToAction("CheckOut", "Shop", new { id = productId });
        }

        [HttpPost]
        public JsonResult EditQuantity(int productId, int newQuantity)
        {
            var cartItem = _context.Carts.Include(c => c.Product).FirstOrDefault(c => c.Product_Id == productId);
            string message;
            string newTotalPrice = "0.00";

            if (cartItem != null)
            {
                cartItem.Quantity = newQuantity;
                cartItem.Updated_At = DateTime.Now;
                _context.SaveChanges();
                message = "Item quantity updated.";
                newTotalPrice = (cartItem.Product.Price * newQuantity).ToString("F2");

            }
            else
            {
                message = "Item not found in the cart.";
            }

            return Json(new { newTotalPrice, message });
        }

        [HttpPost]
        public JsonResult RemoveProduct(int productId)
        {
            var cartItem = _context.Carts.FirstOrDefault(c => c.Product_Id == productId);
            string message = string.Empty;

            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                _context.SaveChanges();
                message = "Item has been removed from your cart.";
            }

            return Json(new { message });
        }

        [HttpPost]
        public IActionResult RemoveAll(List<int> productIds)
        {
            var cartsToRemove = _context.Carts.Where(c => productIds.Contains(c.Product_Id)).ToList();
            _context.Carts.RemoveRange(cartsToRemove);

            var result = _context.SaveChanges() > 0;

            if (result)
            {
                return Json(new { message = "Selected items have been removed from your cart." });
            }
            return Json(new { message = "Error removing selected items." });
        }

        [HttpPost]
        public JsonResult UpdateCheckedStatus(int productId, bool isChecked)
        {
            var cartItem = _context.Carts.FirstOrDefault(c => c.Product_Id == productId);

            if (cartItem != null)
            {
                cartItem.Is_Checked = isChecked;
                _context.SaveChanges();
            }

            return Json(new { });
        }


        public IActionResult CheckOut()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                TempData["Message"] = "User not logged in or session expired.";
                return RedirectToAction("Login", "Auth");
            }
            int userId = int.Parse(userIdString);

            var cartItems = _context.Carts
                .Include(c => c.Product)
                .Where(c => c.Customer_Id == userId)
                .ToList();

            var customerDetails = _context.Customers
                .FirstOrDefault(c => c.Id == userId); 

            if (customerDetails != null)
            {
                ViewBag.CustomerName = customerDetails.Name;
                ViewBag.Email = customerDetails.Email; 
                ViewBag.Address = customerDetails.Address;
            }

            return View(cartItems);
        }

        public async Task<IActionResult> CreatePayment(decimal amount, string paymentMethod)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                TempData["Message"] = "User not logged in or session expired.";
                return RedirectToAction("Login", "Auth");
            }
            int customerId = int.Parse(userIdString); 

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found. Please log in again.";
                return RedirectToAction("Login", "Auth"); 
            }

            var cartItems = await _context.Carts
                        .Where(c => c.Customer_Id == customerId && c.Is_Checked == true)
                        .Include(c => c.Product)
                        .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Message"] = "No items selected for checkout.";
                return RedirectToAction("Checkout", "Shop");
            }

            string customerName = customer.Name;
            string customerEmail = customer.Email;
            string customerPhone = customer.Contact_Number ?? "N/A"; 
            string orderDescription = "Orders";
            string successUrl = Url.Action("SuccessCheckout", "Shop", null, Request.Scheme) ?? string.Empty;
            string cancelUrl = Url.Action("CheckOutCancelled", "Shop", null, Request.Scheme) ?? string.Empty;

            if (string.IsNullOrEmpty(successUrl) || string.IsNullOrEmpty(cancelUrl))
            {
                TempData["Message"] = "Unable to generate redirect URLs. Please try again.";
                return RedirectToAction("CheckOut", "Shop");
            }

            var (checkoutUrl, checkoutSessionId) = await _payMongoService.CreateCheckoutSession(
               customerName,
               customerEmail,
               customerPhone,
               orderDescription,
               cartItems,
               paymentMethod,
               successUrl,
               cancelUrl
            );

            if (!string.IsNullOrEmpty(checkoutUrl))
            {
                int pending = 1;

                var order = new Order
                {
                    Customer_Id = customerId,
                    Grand_Total = amount,
                    OrderDate = DateTime.Now,
                    Status_Id = pending,
                    Created_At = DateTime.Now,
                    Updated_At = DateTime.Now,
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var cartItem in cartItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        Order_Id = order.Id,
                        Product_Id = cartItem.Product_Id,
                        Quantity = cartItem.Quantity,
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now
                    };

                    _context.OrderDetails.Add(orderDetail);
                    await _context.SaveChangesAsync();
                }

                var payment = new Payment
                {
                    Order_Id = order.Id,
                    Amount = amount,
                    Method = paymentMethod,
                    Transaction_Id = checkoutSessionId ?? string.Empty,
                    Status = "Pending",
                    Created_at = DateTime.Now,
                    Updated_at = DateTime.Now
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                return Redirect(checkoutUrl);
            }

             TempData["Message"] = "Unable to create checkout session.";
                return RedirectToAction("Checkout", "Shop");
        }

        public async Task<IActionResult> CheckOutCancelled()
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out var customerId))
            {
                return BadRequest("Customer ID not found in claims.");
            }

            var order = await _context.Orders
                                      .Where(o => o.Customer_Id == customerId)
                                      .OrderByDescending(o => o.OrderDate)
                                      .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound("No orders found for this customer.");
            }

            var payment = await _context.Payments
                                        .FirstOrDefaultAsync(p => p.Order_Id == order.Id);

            if (payment == null)
            {
                return NotFound("No payment found for this order.");
            }
            order.Status_Id = 5;
            payment.Status = "Cancelled";
            payment.Updated_at = DateTime.Now;
            await _context.SaveChangesAsync();

            return View();
        }

        public async Task<IActionResult> SuccessCheckout()
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out var customerId))
            {
                return BadRequest("Customer ID not found in claims.");
            }

            var order = await _context.Orders
                                      .Where(o => o.Customer_Id == customerId)
                                      .OrderByDescending(o => o.OrderDate)
                                      .FirstOrDefaultAsync();
            if (order == null)
            {
                return NotFound("No orders found for this customer.");
            }

            var payment = await _context.Payments
                                        .FirstOrDefaultAsync(p => p.Order_Id == order.Id);

            if (payment == null)
            {
                return NotFound("No payment found for this order.");
            }

            string sessionId = payment.Transaction_Id;

            var sessionDetails = await _payMongoService.RetrieveCheckoutSession(sessionId);

            if (sessionDetails == null)
            {
                return NotFound("Checkout session not found.");
            }

            var payments = sessionDetails.data.attributes.payments;

            string? paymongoPaymentStatus = (payments?.Count > 0)
                                            ? payments[0].attributes?.status
                                            : null;


            if (string.IsNullOrEmpty(paymongoPaymentStatus))
            {
                return NotFound("Payment status not found in session details.");
            }

            if (paymongoPaymentStatus == "paid")
            {
                order.Status_Id = 2;
                payment.Status = "Paid";
                payment.Updated_at = DateTime.Now;

                var cartItems = await _context.Carts
                                      .Where(c => c.Customer_Id == customerId && c.Is_Checked == true)
                                      .ToListAsync();

                _context.Carts.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
            }

            return View();
        }
        
        

    }
}
