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

    // *** THÊM ACTION MỚI ĐỂ XỬ LÝ TÌM KIẾM ***
    // GET: /Product/Search?q=...
    public ActionResult Search(string q)
    {
        var products = new List<Product>();

        if (!string.IsNullOrEmpty(q))
        {
            products = db.Products
                         .Where(p => p.Name.Contains(q) || p.Description.Contains(q))
                         .ToList();
        }
        else
        {
            // Nếu không có truy vấn, có thể trả về trang trống hoặc trang chủ
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Query = q;
        return View(products); // Sẽ trả về view mới: Views/Product/Search.cshtml
    }
    // *** KẾT THÚC ACTION MỚI ***

    // GET: /Product/Index (Trang quản lý sản phẩm của Admin)
    public ActionResult Index()
    {
        if (Session["Role"]?.ToString() != "Admin")
        {
            return RedirectToAction("AccessDenied", "Account");
        }
        var products = db.Products.ToList();
        return View(products);
    }

    // GET: /Product/Details/id (Trang chi tiết sản phẩm cho khách hàng)
    public ActionResult Details(int id)
    {
        var product = db.Products.Find(id);
        if (product == null) return HttpNotFound();
        return View(product);
    }

    // GET: /Product/Category/id (Trang danh mục sản phẩm cho khách hàng)
    public ActionResult Category(string id, string sortBy = "default")
    {
        var productsQuery = db.Products
                              .Where(p => p.Category != null && p.Category.ToLower() == id.ToLower());

        switch (sortBy)
        {
            case "price_asc":
                productsQuery = productsQuery.OrderBy(p => p.Price);
                break;
            case "price_desc":
                productsQuery = productsQuery.OrderByDescending(p => p.Price);
                break;
            default:
                productsQuery = productsQuery.OrderByDescending(p => p.ProductID);
                break;
        }

        var products = productsQuery.ToList();

        ViewBag.CategoryName = id;
        ViewBag.SortBy = sortBy;

        ViewBag.Sliders = db.SliderImages
            .Where(s => s.CategoryKey.ToLower() == id.ToLower() && s.IsActive)
            .ToList();

        return View(products);
    }

    // GET: /Product/Create (Trang tạo sản phẩm của Admin)
    public ActionResult Create()
    {
        if (Session["Role"]?.ToString() != "Admin")
        {
            return RedirectToAction("AccessDenied", "Account");
        }
        return View();
    }

    // POST: /Product/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Product product)
    {
        if (Session["Role"]?.ToString() != "Admin")
        {
            return RedirectToAction("AccessDenied", "Account");
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

                string fileName = Path.GetFileName(product.ImageFile.FileName);
                string extension = Path.GetExtension(fileName);
                string newFileName = Guid.NewGuid().ToString() + extension;

                string directoryPath = Server.MapPath("~/Images/Products");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string path = Path.Combine(directoryPath, newFileName);

                product.ImageFile.SaveAs(path);
                product.ImageUrl = "/Images/Products/" + newFileName;
            }
            else
            {
                product.ImageUrl = null;
            }

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

    // GET: /Product/Edit/id (Trang sửa sản phẩm của Admin)
    public ActionResult Edit(int id)
    {
        if (Session["Role"]?.ToString() != "Admin")
        {
            return RedirectToAction("AccessDenied", "Account");
        }
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
        {
            return RedirectToAction("AccessDenied", "Account");
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

                var existingProduct = db.Products.AsNoTracking().FirstOrDefault(p => p.ProductID == product.ProductID);
                if (existingProduct != null && !string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    string oldPath = Server.MapPath(existingProduct.ImageUrl);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                string fileName = Path.GetFileName(product.ImageFile.FileName);
                string extension = Path.GetExtension(fileName);
                string newFileName = Guid.NewGuid().ToString() + extension;

                string directoryPath = Server.MapPath("~/Images/Products");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string path = Path.Combine(directoryPath, newFileName);
                product.ImageFile.SaveAs(path);
                product.ImageUrl = "/Images/Products/" + newFileName;
            }

            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
        }

        return View(product);
    }

    // GET: /Product/Delete/id (Trang xác nhận xóa của Admin)
    public ActionResult Delete(int id)
    {
        if (Session["Role"]?.ToString() != "Admin")
        {
            return RedirectToAction("AccessDenied", "Account");
        }
        var product = db.Products.Find(id);
        if (product == null) return HttpNotFound();
        return View(product);
    }

    // POST: /Product/DeleteConfirmed/id
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        if (Session["Role"]?.ToString() != "Admin")
        {
            return RedirectToAction("AccessDenied", "Account");
        }
        var product = db.Products.Find(id);
        if (product != null)
        {
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                string imagePath = Server.MapPath(product.ImageUrl);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            db.Products.Remove(product);
            db.SaveChanges();
            TempData["Success"] = "Xóa sản phẩm thành công!";
        }
        return RedirectToAction("Index");
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