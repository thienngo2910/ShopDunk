using ShopDunk.Models;
using System.Linq;
using System.Web.Mvc;

public class HomeController : Controller
{
    private AppDbContext db = new AppDbContext();

    public ActionResult Index()
    {
        ViewBag.iPhones = db.Products.Where(p => p.Category == "iPhone").Take(4).ToList();
        ViewBag.iPads = db.Products.Where(p => p.Category == "iPad").Take(4).ToList();
        ViewBag.Macs = db.Products.Where(p => p.Category == "Mac").Take(4).ToList();
        return View();
    }
}