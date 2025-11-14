using ShopDunk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace ShopDunk.Controllers
{
    public class OrderController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: /Order/History
        public ActionResult History()
        {
            if (Session["UserID"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem lịch sử mua hàng.";
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserID"];
            var orders = db.Orders
                .Where(o => o.UserID == userId)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // GET: /Order/Checkout
        public ActionResult Checkout()
        {
            if (Session["UserID"] == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thanh toán.";
                // --- CẬP NHẬT: Gửi kèm returnUrl ---
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Order") });
            }

            int userId = (int)Session["UserID"];
            var cartItems = db.CartItems.Include(c => c.Product).Where(c => c.UserID == userId).ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống. Không thể thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            return View(cartItems);
        }

        // POST: /Order/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        // --- THÊM "note" VÀO DANH SÁCH THAM SỐ ---
        public ActionResult PlaceOrder(string shippingAddress, string phoneNumber, string paymentMethod, string note)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserID"];
            var cartItems = db.CartItems.Include(c => c.Product).Where(c => c.UserID == userId).ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction("Index", "Cart");
            }

            if (string.IsNullOrEmpty(shippingAddress) || string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(paymentMethod))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin giao hàng và thanh toán.";
                return View("Checkout", cartItems);
            }

            var order = new Order
            {
                UserID = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity),
                Status = "Chờ xử lý",
                ShippingAddress = shippingAddress,
                PhoneNumber = phoneNumber,
                PaymentMethod = paymentMethod,

                // --- THÊM DÒNG NÀY ---
                Note = note // Lưu ghi chú
            };
            db.Orders.Add(order);
            db.SaveChanges();

            foreach (var item in cartItems)
            {
                db.OrderDetails.Add(new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                });

                // Trừ tồn kho
                item.Product.Stock -= item.Quantity;
                db.Entry(item.Product).State = EntityState.Modified;
            }

            db.CartItems.RemoveRange(cartItems);
            db.SaveChanges();

            TempData["Success"] = "Đặt hàng thành công! Đơn hàng của bạn đang chờ xử lý.";
            return RedirectToAction("History", "Order");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}