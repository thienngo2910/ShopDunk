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

    // Helper: Chuyển file sang byte[]
    private byte[] ConvertToBytes(HttpPostedFileBase image)
    {
        if (image == null) return null;
        using (var reader = new BinaryReader(image.InputStream))
        {
            return reader.ReadBytes((int)image.ContentLength);
        }
    }

    [HttpGet]
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

    [HttpGet]
    public ActionResult Index(string categoryFilter = "all", string sortBy = "name_asc")
    {
        if (Session["Role"]?.ToString() != "Admin") return RedirectToAction("AccessDenied", "Account");

        var categories = db.Products
                           .Select(p => p.Category)
                           .Where(c => c != null && c != "")
                           .Distinct()
                           .ToList();
        ViewBag.Categories = new SelectList(categories, categoryFilter);
        ViewBag.CurrentCategory = categoryFilter;

        var productsQuery = db.Products.AsQueryable();
        if (categoryFilter != "all" && !string.IsNullOrEmpty(categoryFilter))
        {
            productsQuery = productsQuery.Where(p => p.Category == categoryFilter);
        }

        ViewBag.SortBy = sortBy;
        switch (sortBy)
        {
            case "price_desc": productsQuery = productsQuery.OrderByDescending(p => p.Price); break;
            case "price_asc": productsQuery = productsQuery.OrderBy(p => p.Price); break;
            case "name_desc": productsQuery = productsQuery.OrderByDescending(p => p.Name); break;
            default: productsQuery = productsQuery.OrderBy(p => p.Name); break;
        }

        var products = productsQuery.ToList();
        return View(products);
    }

    [HttpGet]
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

    [HttpGet]
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

    [HttpGet]
    public ActionResult Create()
    {
        if (Session["Role"]?.ToString() != "Admin") return RedirectToAction("AccessDenied", "Account");
        return View();
    }

    // POST: /Product/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Product product)
    {
        if (Session["Role"]?.ToString() != "Admin") return RedirectToAction("AccessDenied", "Account");

        // --- SỬA LỖI: Kiểm tra danh mục ---
        if (string.IsNullOrEmpty(product.Category))
        {
            ModelState.AddModelError("Category", "Vui lòng chọn danh mục.");
        }

        try
        {
            if (product.ImageFile != null && product.ImageFile.ContentLength > 0)
            {
                if (product.ImageFile.ContentLength > MAX_FILE_SIZE)
                {
                    ModelState.AddModelError("ImageFile", "Ảnh không được vượt quá 5MB.");
                    return View(product);
                }
                product.ImageData = ConvertToBytes(product.ImageFile);
            }

            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges(); // Lúc này sản phẩm đã có ID

                TempData["Success"] = "Đã tạo sản phẩm! Hãy thêm các phiên bản màu sắc/dung lượng.";

                // --- SỬA ĐỔI: Chuyển thẳng sang trang thêm biến thể ---
                return RedirectToAction("ManageVariants", new { id = product.ProductID });
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi: " + ex.Message);
        }
        return View(product);
    }

    [HttpGet]
    public ActionResult Edit(int id)
    {
        if (Session["Role"]?.ToString() != "Admin") return RedirectToAction("AccessDenied", "Account");
        var product = db.Products.Find(id);
        if (product == null) return HttpNotFound();
        return View(product);
    }

    // POST: /Product/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(Product product)
    {
        if (Session["Role"]?.ToString() != "Admin") return RedirectToAction("AccessDenied", "Account");

        // --- SỬA LỖI: Kiểm tra danh mục ---
        if (string.IsNullOrEmpty(product.Category))
        {
            ModelState.AddModelError("Category", "Vui lòng chọn danh mục.");
        }

        if (product.ImageFile != null && product.ImageFile.ContentLength > 0)
        {
            try
            {
                if (product.ImageFile.ContentLength > MAX_FILE_SIZE)
                { ModelState.AddModelError("ImageFile", "Ảnh không được vượt quá 5MB."); }
                else
                {
                    product.ImageData = ConvertToBytes(product.ImageFile);
                }
            }
            catch (Exception ex)
            { ModelState.AddModelError("ImageFile", "Lỗi tải lên ảnh: " + ex.Message); }
        }
        else
        {
            // Giữ lại ảnh cũ nếu không upload file mới
            var oldItem = db.Products.AsNoTracking().FirstOrDefault(p => p.ProductID == product.ProductID);
            product.ImageData = oldItem?.ImageData;
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
            {
                // Bắt lỗi DB nếu có (ví dụ lỗi NULL Category)
                ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
            }
        }

        // Fix lỗi mất giá khi validation fail
        if (!ModelState.IsValidField("Price"))
        {
            ModelState.Remove("Price");
            var oldPrice = db.Products.AsNoTracking().Where(p => p.ProductID == product.ProductID).Select(p => p.Price).FirstOrDefault();
            product.Price = oldPrice;
        }

        return View(product);
    }

    [HttpGet]
    public ActionResult Delete(int id)
    {
        if (Session["Role"]?.ToString() != "Admin") return RedirectToAction("AccessDenied", "Account");
        var product = db.Products.Find(id);
        if (product == null) return HttpNotFound();
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        if (Session["Role"]?.ToString() != "Admin") return RedirectToAction("AccessDenied", "Account");
        var product = db.Products.Find(id);
        if (product != null)
        {
            db.Products.Remove(product);
            db.SaveChanges();
            TempData["Success"] = "Xóa sản phẩm thành công!";
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult AddReview(int ProductID, int Rating, string Comment)
    {
        // Kiểm tra 1: Đã đăng nhập chưa
        if (Session["UserID"] == null)
        {
            TempData["ReviewError"] = "Vui lòng đăng nhập.";
            return RedirectToAction("Details", new { id = ProductID });
        }

        int userId = (int)Session["UserID"];

        // --- KIỂM TRA 2: LOGIC XÁC MINH MUA HÀNG ---
        // Kiểm tra xem UserID này đã từng có OrderDetail cho ProductID này
        // VÀ trạng thái của Order phải là "Đã giao"
        var hasPurchased = db.OrderDetails
            .Include(od => od.Order) // Liên kết với bảng Order
            .Any(od =>
                od.ProductID == ProductID &&
                od.Order.UserID == userId &&
                od.Order.Status == "Đã giao"); // Chỉ chấp nhận đơn đã giao

        if (!hasPurchased)
        {
            TempData["ReviewError"] = "Xin lỗi, chỉ khách hàng đã mua và nhận sản phẩm này mới được đánh giá.";
            return RedirectToAction("Details", new { id = ProductID });
        }
        // --- KẾT THÚC LOGIC XÁC MINH ---


        // KIỂM TRA 3: Chưa đánh giá sản phẩm này lần nào
        if (!db.ProductReviews.Any(r => r.ProductID == ProductID && r.UserID == userId))
        {
            db.ProductReviews.Add(new ProductReview
            {
                ProductID = ProductID,
                UserID = userId,
                Rating = Rating,
                Comment = Comment,
                ReviewDate = DateTime.Now
            });
            db.SaveChanges();
            TempData["ReviewSuccess"] = "Đánh giá thành công!";
        }
        else
        {
            TempData["ReviewError"] = "Bạn đã đánh giá sản phẩm này rồi.";
        }

        return RedirectToAction("Details", new { id = ProductID });
    }
    [HttpGet]
    public ActionResult ManageVariants(int id)
    {
        if (Session["Role"]?.ToString() != "Admin") return RedirectToAction("AccessDenied", "Account");
        var product = db.Products.Include("Variants").FirstOrDefault(p => p.ProductID == id);
        if (product == null) return HttpNotFound();
        return View(product);
    }

    // POST: /Product/AddVariant
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult AddVariant(int ProductID, string Color, string Storage, decimal Price, int Stock)
    {
        if (Session["Role"]?.ToString() != "Admin") return RedirectToAction("AccessDenied", "Account");

        var variant = new ProductVariant
        {
            ProductID = ProductID,
            Color = Color,
            Storage = Storage,
            Price = Price,
            Stock = Stock
        };
        db.ProductVariants.Add(variant);
        db.SaveChanges();
        TempData["Success"] = "Thêm biến thể thành công!";
        return RedirectToAction("ManageVariants", new { id = ProductID });
    }

    // POST: /Product/DeleteVariant/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteVariant(int id)
    {
        if (Session["Role"]?.ToString() != "Admin") return RedirectToAction("AccessDenied", "Account");
        var variant = db.ProductVariants.Find(id);
        int productId = variant.ProductID;
        if (variant != null)
        {
            db.ProductVariants.Remove(variant);
            db.SaveChanges();
            TempData["Success"] = "Xóa biến thể thành công!";
        }
        return RedirectToAction("ManageVariants", new { id = productId });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { db.Dispose(); }
        base.Dispose(disposing);
    }
}