using System;
using System.Linq;
using System.Web.Mvc;
using ShopDunk.Models;
using System.Collections.Generic;

namespace ShopDunk.Controllers
{
    public class HomeController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            var products = db.Products.ToList();

            // Lấy 4 sản phẩm cho mỗi danh mục
            ViewBag.iPhones = products
                .Where(p => !string.IsNullOrEmpty(p.Category) && p.Category.Equals("iPhone", StringComparison.OrdinalIgnoreCase))
                .Take(4)
                .ToList();

            ViewBag.iPads = products
                .Where(p => !string.IsNullOrEmpty(p.Category) && p.Category.Equals("iPad", StringComparison.OrdinalIgnoreCase))
                .Take(4)
                .ToList();

            ViewBag.Macs = products
                .Where(p => !string.IsNullOrEmpty(p.Category) && p.Category.Equals("Mac", StringComparison.OrdinalIgnoreCase))
                .Take(4)
                .ToList();

            // Trả model về view (vẫn giữ nguyên để tương thích)
            return View(products);
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