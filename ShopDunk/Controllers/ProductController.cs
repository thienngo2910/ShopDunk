using ShopDunk.Models;
using System.Linq;
using System.Web.Mvc;

public class ProductController : Controller
{
    private AppDbContext db = new AppDbContext();

    public ActionResult Index()
    {
        var products = db.Products.ToList();
        return View(products);
    }

    public ActionResult Details(int id)
    {
        var product = db.Products.Find(id);
        if (product == null) return HttpNotFound();
        return View(product);
    }
}