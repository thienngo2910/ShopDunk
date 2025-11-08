using ShopDunk.Models;
using System.Linq;
using System.Web.Mvc;

public class ProductController : Controller
{
    private AppDbContext db = new AppDbContext();

    // Hiển thị tất cả sản phẩm
    public ActionResult Index()
    {
        var products = db.Products.ToList();
        return View(products);
    }

    // Hiển thị chi tiết sản phẩm
    public ActionResult Details(int id)
    {
        var product = db.Products.Find(id);
        if (product == null) return HttpNotFound();
        return View(product);
    }

    // Hiển thị sản phẩm theo danh mục
    public ActionResult Category(string name)
    {
        var products = db.Products
                         .Where(p => p.Category != null && p.Category.ToLower() == name.ToLower())
                         .ToList();

        ViewBag.CategoryName = name;
        return View(products);
    }
}