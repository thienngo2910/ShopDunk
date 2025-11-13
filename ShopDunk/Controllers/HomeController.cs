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

            // Lấy 8 sản phẩm cho mỗi danh mục
            ViewBag.iPhones = products
                .Where(p => !string.IsNullOrEmpty(p.Category) && p.Category.Equals("iPhone", StringComparison.OrdinalIgnoreCase))
                .Take(8)
                .ToList();

            ViewBag.iPads = products
                .Where(p => !string.IsNullOrEmpty(p.Category) && p.Category.Equals("iPad", StringComparison.OrdinalIgnoreCase))
                .Take(8)
                .ToList();

            ViewBag.Macs = products
                .Where(p => !string.IsNullOrEmpty(p.Category) && p.Category.Equals("Mac", StringComparison.OrdinalIgnoreCase))
                .Take(8)
                .ToList();

            ViewBag.Watch = products
                .Where(p => !string.IsNullOrEmpty(p.Category) && p.Category.Equals("Watch", StringComparison.OrdinalIgnoreCase))
                .Take(8)
                .ToList();

            ViewBag.Audio = products
                .Where(p => !string.IsNullOrEmpty(p.Category) && p.Category.Equals("Âm thanh", StringComparison.OrdinalIgnoreCase))
                .Take(8)
                .ToList();

            ViewBag.Accessories = products
                .Where(p => !string.IsNullOrEmpty(p.Category) && p.Category.Equals("Phụ kiện", StringComparison.OrdinalIgnoreCase))
                .Take(8)
                .ToList();

            // Lấy slider từ CSDL cho trang chủ ("Home")
            ViewBag.Sliders = db.SliderImages
                .Where(s => s.CategoryKey == "Home" && s.IsActive)
                .ToList();

            // *** DÒNG QUAN TRỌNG NHẤT LÀ DÒNG NÀY ***
            // Nó trả về model List<Product> cho View "Index.cshtml"
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