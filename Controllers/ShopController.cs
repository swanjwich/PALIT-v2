using cce106_palit.Data;
using cce106_palit.Entity;
using cce106_palit.Models;
using cce106_palit.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace cce106_palit.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PayMongoService _payMongoService;
        private readonly EmailService _emailService;

        public ShopController(ApplicationDbContext context, PayMongoService payMongoService, EmailService emailService)
        {
            _context = context;
            _payMongoService = payMongoService;
            _emailService = emailService;
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
            ViewBag.Stock = product.StockQuantity;

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
                return RedirectToAction("Login", "Auth");
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddToCart(string customerId, int productId, int quantity)
        {
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                TempData["Message"] = "Product not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("ProductDetails", "Shop", new { id = productId });
            }

            if (quantity > product.StockQuantity)
            {
                TempData["Message"] = $"Only {product.StockQuantity} items are available in stock.";
                TempData["MessageType"] = "warning";
                return RedirectToAction("ProductDetails", "Shop", new { id = productId });
            }

            int parsedCustomerId = int.Parse(customerId);

            var existingCartItem = _context.Carts
                .FirstOrDefault(c => c.Customer_Id == parsedCustomerId && c.Product_Id == productId);

            if (existingCartItem != null)
            {
                int totalRequested = existingCartItem.Quantity + quantity;

                if (totalRequested > product.StockQuantity)
                {
                    TempData["Message"] = $"You already have {existingCartItem.Quantity} in your cart. Only {product.StockQuantity - existingCartItem.Quantity} more can be added.";
                    TempData["MessageType"] = "warning";
                }
                else
                {
                    existingCartItem.Quantity += quantity;
                    existingCartItem.Updated_At = DateTime.Now;
                    TempData["Message"] = "Item quantity updated in your cart.";
                    TempData["MessageType"] = "success";
                }
            }
            else
            {
                var cartItem = new Cart
                {
                    Customer_Id = parsedCustomerId,
                    Product_Id = productId,
                    Quantity = quantity,
                    Is_Checked = false,
                    Created_At = DateTime.Now,
                    Updated_At = DateTime.Now
                };

                _context.Carts.Add(cartItem);
                TempData["Message"] = "Item has been added to your cart.";
                TempData["MessageType"] = "success";
            }

            _context.SaveChanges();

            return RedirectToAction("ProductDetails", "Shop", new { id = productId });
        }


        [Authorize]
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
                TempData["MessageType"] = "info";
            }
            else
            {
                _context.Carts.Add(cartItem);
                TempData["Message"] = "Item has been added to your cart.";
                TempData["MessageType"] = "success";
            }

            _context.SaveChanges();

            return RedirectToAction("CheckOut", "Shop", new { id = productId });
        }

        [Authorize]
        [HttpPost]
        public IActionResult BuyAgain(string customerId, List<int> productIds, List<int> quantities)
        {
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customerIdInt = int.Parse(customerId);
            var messages = new List<string>();
            var messageTypes = new List<string>();
            int? firstProductId = null;

            // Loop through all products
            for (int i = 0; i < productIds.Count; i++)
            {
                var productId = productIds[i];
                var quantity = quantities[i];

                var cartItem = new Cart
                {
                    Customer_Id = customerIdInt,
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
                    messages.Add($"'{_context.Products.Find(productId)?.Name}' is already in your cart");
                    messageTypes.Add("info");
                }
                else
                {
                    _context.Carts.Add(cartItem);
                    messages.Add($"'{_context.Products.Find(productId)?.Name}' added to cart");
                    messageTypes.Add("success");
                }

                firstProductId ??= productId;
            }

            _context.SaveChanges();

            TempData["Message"] = string.Join(". ", messages);
            TempData["MessageType"] = messageTypes.Contains("success") ? "success" : "info";

            return RedirectToAction("CheckOut", "Shop", new { id = firstProductId });
        }

        [Authorize]
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

        [Authorize]
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

        [Authorize]
        [HttpPost]
        public IActionResult RemoveAll(List<int> productIds)
        {
            var cartsToRemove = _context.Carts.Where(c => productIds.Contains(c.Product_Id)).ToList();
            _context.Carts.RemoveRange(cartsToRemove);

            var result = _context.SaveChanges() > 0;

            if (result)
            {
                return Json(new { success = true,message = "Selected items have been removed from your cart." });
            }
            return Json(new { success = false, message = "Error removing selected items." });
        }

        [Authorize]
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

        [Authorize]
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

        [Authorize]
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

            if (string.IsNullOrWhiteSpace(customer.Address))
            {
                TempData["Message"] = "You must add a delivery address before placing an order.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Checkout", "Shop");
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

            foreach (var cartItem in cartItems)
            {
                if (cartItem.Product.StockQuantity < cartItem.Quantity)
                {
                    TempData["Message"] = $"Insufficient stock for {cartItem.Product.Name}.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Checkout", "Shop");
                }
            }

            string customerName = customer.Name;
            string customerEmail = customer.Email;
            string customerPhone = customer.Contact_Number ?? "N/A"; 
            if(!string.IsNullOrEmpty(customerPhone) && customerPhone.StartsWith("0"))
            {
                customerPhone = customerPhone.Substring(1);
            }
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

            if (checkoutUrl == null)
            {
                TempData["Message"] = "Sorry, the minimum order total must be ₱20.00 due to payment provider restrictions.";
                TempData["MessageType"] = "info.";
                return RedirectToAction("CheckOut");
            }

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
                    cartItem.Product.StockQuantity -= cartItem.Quantity;

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

        [Authorize]
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

        [Authorize]
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

            string? paymongoPaymentStatus = payments?.Count > 0
                                            ? payments[0].attributes?.status
                                            : null;

            string? paymentMethod = (payments != null && payments.Count > 0)
                                    ? payments[0]?.attributes?.source?.type
                                    : null;

            if (string.IsNullOrEmpty(paymongoPaymentStatus))
            {
                return NotFound("Payment status not found in session details.");
            }

            if (paymongoPaymentStatus == "paid")
            {
                order.Status_Id = 2;
                payment.Method = paymentMethod;
                payment.Status = "Paid";
                payment.Updated_at = DateTime.Now;

                var cartItems = await _context.Carts
                                      .Where(c => c.Customer_Id == customerId && c.Is_Checked == true)
                                      .ToListAsync();

                _context.Carts.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                var customer = await _context.Customers.FindAsync(customerId);
                if (customer != null && !string.IsNullOrEmpty(customer.Email))
                {
                    string subject = $"Your Order #{order.Id} has been confirmed";
                    string body = $@"
                    <h2>Thank you for your purchase, {customer.Name}!</h2>
                    <p>Your order ID: <strong>{order.Id}</strong> has been successfully placed and paid.</p>
                    <p>We will process and ship your order shortly.</p>
                    <p>Transaction ID: {payment.Transaction_Id}</p>
                    <br/>
                    <p>- The PALIT Team</p>";

                    await _emailService.SendEmailAsync(customer.Email, subject, body);

                }
            }
            return View();
        }      
    }
}
