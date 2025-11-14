using ShopDunk.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Collections.Generic;
using System.Diagnostics;

public class ProductController : Controller
{
    private AppDbContext db = new AppDbContext();

    private const int MAX_FILE_SIZE = 5 * 1024 * 1024;

    [HttpGet] // <-- ĐÃ THÊM
    public ActionResult Search(string q)
    {
        var products = new List<Product>();
        if (!string.IsNullOrEmpty(q))
        {
            products = db.Products.Where(p => p.Name.Contains(q) || p.Description.Contains(q)).ToList();
        }
        else { return RedirectToAction("Index", "Home"); }
        ViewBag.Query = q;
        return View(products);
    }

    // GET: /Product/Index
    [HttpGet] // <-- ĐÃ THÊM
    public ActionResult Index()
    {
        if (Session["Role"]?.ToString() != "Admin")
        { return RedirectToAction("AccessDenied", "Account"); }
        var products = db.Products.ToList();
        return View(products);
    }

    // GET: /Product/Details/id
    [HttpGet] // <-- SỬA LỖI Ở ĐÂY
    public ActionResult Details(int id)
    {
        var product = db.Products.Find(id);
        if (product == null) return HttpNotFound();

        var suggestedProducts = db.Products
            .Where(p => p.Category == product.Category && p.ProductID != id)
            .OrderBy(p => p.ProductID)
            .Take(4)
            .ToList();
        ViewBag.SuggestedProducts = suggestedProducts;

        var reviews = db.ProductReviews
            .Where(r => r.ProductID == id)
            .Include(r => r.User)
            .OrderByDescending(r => r.ReviewDate)
            .ToList();
        ViewBag.Reviews = reviews;

        if (reviews.Any())
        {
            ViewBag.AverageRating = reviews.Average(r => r.Rating);
            ViewBag.ReviewCount = reviews.Count;
        }
        else
        {
            ViewBag.AverageRating = 0;
            ViewBag.ReviewCount = 0;
        }

        return View(product);
    }

    // GET: /Product/Category/id
    [HttpGet] // <-- ĐÃ THÊM
    public ActionResult Category(string id, string sortBy = "default")
    {
        var productsQuery = db.Products
                              .Where(p => p.Category != null && p.Category.ToLower() == id.ToLower());

        switch (sortBy)
        {
            case "price_asc": productsQuery = productsQuery.OrderBy(p => p.Price); break;
            case "price_desc": productsQuery = productsQuery.OrderByDescending(p => p.Price); break;
            default: productsQuery = productsQuery.OrderByDescending(p => p.ProductID); break;
        }
        var products = productsQuery.ToList();
        ViewBag.CategoryName = id;
        ViewBag.SortBy = sortBy;
        ViewBag.Sliders = db.SliderImages.Where(s => s.CategoryKey.ToLower() == id.ToLower() && s.IsActive).ToList();
        return View(products);
    }

    // GET: /Product/Create
    [HttpGet] // <-- ĐÃ THÊM
    public ActionResult Create()
    {
        if (Session["Role"]?.ToString() != "Admin")
        { return RedirectToAction("AccessDenied", "Account"); }
        return View();
    }

    // POST: /Product/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Product product)
    {
        if (Session["Role"]?.ToString() != "Admin")
        { return RedirectToAction("AccessDenied", "Account"); }
        try
        {
            if (product.ImageFile != null && product.ImageFile.ContentLength > 0)
            {
                if (product.ImageFile.ContentLength > MAX_FILE_SIZE)
                {
                    ModelState.AddModelError("ImageFile", "Ảnh không được vượt quá 5MB.");
                    return View(product);
                }
                string fileName = Path.GetFileName(product.ImageFile.FileName);
                string extension = Path.GetExtension(fileName);
                string newFileName = Guid.NewGuid().ToString() + extension;
                string directoryPath = Server.MapPath("~/Images/Products");
                if (!Directory.Exists(directoryPath))
                { Directory.CreateDirectory(directoryPath); }
                string path = Path.Combine(directoryPath, newFileName);
                product.ImageFile.SaveAs(path);
                product.ImageUrl = "/Images/Products/" + newFileName;
            }
            else { product.ImageUrl = null; }

            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "LỖI HỆ THỐNG KHI LƯU FILE: " + ex.Message);
        }
        return View(product);
    }

    // GET: /Product/Edit/id
    [HttpGet] // <-- ĐÃ THÊM
    public ActionResult Edit(int id)
    {
        if (Session["Role"]?.ToString() != "Admin")
        { return RedirectToAction("AccessDenied", "Account"); }
        var product = db.Products.Find(id);
        if (product == null) return HttpNotFound();
        return View(product);
    }

    // POST: /Product/Edit/id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(Product product)
    {
        if (Session["Role"]?.ToString() != "Admin")
        { return RedirectToAction("AccessDenied", "Account"); }

        if (product.ImageFile != null && product.ImageFile.ContentLength > 0)
        {
            try
            {
                if (product.ImageFile.ContentLength > MAX_FILE_SIZE)
                { ModelState.AddModelError("ImageFile", "Ảnh không được vượt quá 5MB."); }
                else
                {
                    var existingProduct = db.Products.AsNoTracking().FirstOrDefault(p => p.ProductID == product.ProductID);
                    if (existingProduct != null && !string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        string oldPath = Server.MapPath(existingProduct.ImageUrl);
                        if (System.IO.File.Exists(oldPath))
                        { System.IO.File.Delete(oldPath); }
                    }
                    string fileName = Path.GetFileName(product.ImageFile.FileName);
                    string extension = Path.GetExtension(fileName);
                    string newFileName = Guid.NewGuid().ToString() + extension;
                    string directoryPath = Server.MapPath("~/Images/Products");
                    if (!Directory.Exists(directoryPath))
                    { Directory.CreateDirectory(directoryPath); }
                    string path = Path.Combine(directoryPath, newFileName);
                    product.ImageFile.SaveAs(path);
                    product.ImageUrl = "/Images/Products/" + newFileName;
                }
            }
            catch (Exception ex)
            { ModelState.AddModelError("ImageFile", "Lỗi tải lên ảnh: " + ex.Message); }
        }
        else
        {
            var existingProduct = db.Products.AsNoTracking().FirstOrDefault(p => p.ProductID == product.ProductID);
            product.ImageUrl = existingProduct?.ImageUrl;
        }

        if (ModelState.IsValid)
        {
            try
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            { ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message); }
        }

        if (!ModelState.IsValidField("Price"))
        {
            ModelState.Remove("Price");
            var originalPrice = db.Products.AsNoTracking()
                                    .Where(p => p.ProductID == product.ProductID)
                                    .Select(p => p.Price)
                                    .FirstOrDefault();
            product.Price = originalPrice;
        }
        return View(product);
    }

    // GET: /Product/Delete/id
    [HttpGet] // <-- ĐÃ THÊM
    public ActionResult Delete(int id)
    {
        if (Session["Role"]?.ToString() != "Admin")
        { return RedirectToAction("AccessDenied", "Account"); }
        var product = db.Products.Find(id);
        if (product == null) return HttpNotFound();
        return View(product);
    }

    // POST: /Product/Delete/id
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        if (Session["Role"]?.ToString() != "Admin")
        { return RedirectToAction("AccessDenied", "Account"); }
        var product = db.Products.Find(id);
        if (product != null)
        {
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                string imagePath = Server.MapPath(product.ImageUrl);
                if (System.IO.File.Exists(imagePath))
                { System.IO.File.Delete(imagePath); }
            }
            db.Products.Remove(product);
            db.SaveChanges();
            TempData["Success"] = "Xóa sản phẩm thành công!";
        }
        return RedirectToAction("Index");
    }

    // POST: /Product/AddReview
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult AddReview(int ProductID, int Rating, string Comment)
    {
        if (Session["UserID"] == null)
        {
            TempData["ReviewError"] = "Vui lòng đăng nhập để đánh giá.";
            return RedirectToAction("Details", new { id = ProductID });
        }

        int userId = (int)Session["UserID"];

        bool hasReviewed = db.ProductReviews.Any(r => r.ProductID == ProductID && r.UserID == userId);
        if (hasReviewed)
        {
            TempData["ReviewError"] = "Bạn đã đánh giá sản phẩm này rồi.";
            return RedirectToAction("Details", new { id = ProductID });
        }

        var review = new ProductReview
        {
            ProductID = ProductID,
            UserID = userId,
            Rating = Rating,
            Comment = Comment,
            ReviewDate = DateTime.Now
        };

        db.ProductReviews.Add(review);
        db.SaveChanges();

        TempData["ReviewSuccess"] = "Cảm ơn bạn đã đánh giá!";
        return RedirectToAction("Details", new { id = ProductID });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        { db.Dispose(); }
        base.Dispose(disposing);
    }
}