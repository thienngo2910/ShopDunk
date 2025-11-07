using ShopDunk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShopDunk.Controllers
{
    public class OrderController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Checkout()
        {
            int userId = (int)Session["UserID"];
            var cartItems = db.CartItems.Include("Product").Where(c => c.UserID == userId).ToList();
            return View(cartItems);
        }

        [HttpPost]
        public ActionResult PlaceOrder()
        {
            int userId = (int)Session["UserID"];
            var cartItems = db.CartItems.Include("Product").Where(c => c.UserID == userId).ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction("Index", "Cart");
            }

            var order = new Order
            {
                UserID = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity),
                Status = "Chờ xử lý"
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
            }

            db.CartItems.RemoveRange(cartItems);
            db.SaveChanges();

            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction("Index", "Product");
        }
    }
}
