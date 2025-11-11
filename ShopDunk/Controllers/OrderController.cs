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
        [ValidateAntiForgeryToken] // Thêm AntiForgeryToken
        public ActionResult PlaceOrder()
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

            // 1. Tạo đơn hàng (Order)
            var order = new Order
            {
                UserID = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity),
                Status = "Chờ xử lý"
            };
            db.Orders.Add(order);
            db.SaveChanges(); // Lưu để lấy được OrderID

            // 2. Tạo chi tiết đơn hàng (OrderDetail)
            foreach (var item in cartItems)
            {
                db.OrderDetails.Add(new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price // Lưu giá tại thời điểm đặt hàng
                });
            }

            // 3. Xóa giỏ hàng
            db.CartItems.RemoveRange(cartItems);
            db.SaveChanges();

            TempData["Success"] = "Đặt hàng thành công! Đơn hàng của bạn đang chờ xử lý.";
            return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ hoặc trang Order History
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