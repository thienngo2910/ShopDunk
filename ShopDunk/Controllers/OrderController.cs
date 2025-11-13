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
                return RedirectToAction("Login", "Account");
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
        public ActionResult PlaceOrder(string shippingAddress, string phoneNumber, string paymentMethod)
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

            // --- BƯỚC 1: KIỂM TRA TỒN KHO (THÊM MỚI) ---
            foreach (var item in cartItems)
            {
                // item.Product.Stock là số lượng tồn kho
                // item.Quantity là số lượng user muốn mua
                if (item.Product.Stock < item.Quantity)
                {
                    TempData["Error"] = $"Sản phẩm '{item.Product.Name}' không đủ hàng (Chỉ còn {item.Product.Stock} sản phẩm).";
                    return View("Checkout", cartItems); // Trả về trang checkout với lỗi
                }
            }

            // (Validation thông tin giao hàng - giữ nguyên)
            if (string.IsNullOrEmpty(shippingAddress) || string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(paymentMethod))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin giao hàng và thanh toán.";
                return View("Checkout", cartItems);
            }

            // --- BƯỚC 2: TẠO ĐƠN HÀNG (Giữ nguyên) ---
            var order = new Order
            {
                UserID = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity),
                Status = "Chờ xử lý",
                ShippingAddress = shippingAddress,
                PhoneNumber = phoneNumber,
                PaymentMethod = paymentMethod
            };
            db.Orders.Add(order);
            db.SaveChanges(); // Lưu để lấy được OrderID

            // --- BƯỚC 3: TẠO CHI TIẾT VÀ CẬP NHẬT KHO (CẬP NHẬT) ---
            foreach (var item in cartItems)
            {
                // 3a. Thêm chi tiết đơn hàng
                db.OrderDetails.Add(new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                });

                // 3b. Cập nhật tồn kho (THÊM MỚI)
                // (Vì đã Include Product, item.Product là 1 entity được theo dõi)
                item.Product.Stock -= item.Quantity;
                db.Entry(item.Product).State = EntityState.Modified;
            }

            // --- BƯỚC 4: XÓA GIỎ HÀNG (Giữ nguyên) ---
            db.CartItems.RemoveRange(cartItems);

            // --- BƯỚC 5: LƯU TẤT CẢ THAY ĐỔI (Giữ nguyên) ---
            db.SaveChanges();

            TempData["Success"] = "Đặt hàng thành công! Đơn hàng của bạn đang chờ xử lý.";
            return RedirectToAction("History", "Order"); // Chuyển đến trang Lịch sử đơn hàng
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