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
            // Lấy toàn bộ danh sách products làm model cho view (Index.cshtml có @model IEnumerable<ShopDunk.Models.Product>)
            var products = db.Products.ToList();

            // Chuẩn bị các danh sách theo danh mục (có kiểm tra null và so sánh không phân biệt hoa thường)
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

            // Trả model về view để khớp với @model trong Views/Home/Index.cshtml
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