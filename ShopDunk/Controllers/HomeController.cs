using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShopDunk.Controllers
{
    public class HomeController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            var featured = db.Products.OrderByDescending(p => p.ProductID).Take(4).ToList();
            return View(featured);
        }
    }
}