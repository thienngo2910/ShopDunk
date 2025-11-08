using ShopDunk.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

public class ProductController : Controller
{
    private AppDbContext db = new AppDbContext();

    // Hiển thị sản phẩm chia theo danh mục
    public ActionResult Index()
    {
        ViewBag.iPhones = db.Products.Where(p => p.Category == "iPhone").Take(4).ToList();
        ViewBag.iPads = db.Products.Where(p => p.Category == "iPad").Take(4).ToList();
        ViewBag.Macs = db.Products.Where(p => p.Category == "Mac").Take(4).ToList();
        return View();
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

    // Hiển thị form thêm sản phẩm
    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Product product)
    {
        if (ModelState.IsValid)
        {
            db.Products.Add(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(product);
    }

    // Hiển thị form sửa sản phẩm
    public ActionResult Edit(int id)
    {
        var product = db.Products.Find(id);
        if (product == null) return HttpNotFound();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(Product product)
    {
        if (ModelState.IsValid)
        {
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(product);
    }

    // Hiển thị xác nhận xóa
    public ActionResult Delete(int id)
    {
        var product = db.Products.Find(id);
        if (product == null) return HttpNotFound();
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        var product = db.Products.Find(id);
        db.Products.Remove(product);
        db.SaveChanges();
        return RedirectToAction("Index");
    }
}