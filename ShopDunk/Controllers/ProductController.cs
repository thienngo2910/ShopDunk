using ShopDunk.Models;
//using ShopDunk.Helpers; // Vô hiệu hóa 'Helpers' vì chúng ta không có file Logger.cs
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

public class ProductController : Controller
{
    private AppDbContext db = new AppDbContext();

    // Đặt giới hạn kích thước file (ví dụ: 5MB)
    private const int MAX_FILE_SIZE = 5 * 1024 * 1024;

    // GET: /Product/Index
    public ActionResult Index()
    {
        if (Session["Role"]?.ToString() != "Admin")
        {
            return RedirectToAction("AccessDenied", "Account");
        }
        var products = db.Products.ToList();
        return View(products);
    }

    // GET: /Product/Details/id
    public ActionResult Details(int id)
    {
        var product = db.Products.Find(id);
        if (product == null) return HttpNotFound();
        return View(product);
    }

    // GET: /Product/Category/name
    public ActionResult Category(string name)
    {
        var products = db.Products
                         .Where(p => p.Category != null && p.Category.ToLower() == name.ToLower())
                         .ToList();
        ViewBag.CategoryName = name;
        return View(products);
    }

    // GET: /Product/Create
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
    // *** SỬA LỖI BINDING: Xóa tham số 'ImageFile' thừa ***
    public ActionResult Create(Product product)
    {
        if (Session["Role"]?.ToString() != "Admin")
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            // *** SỬA LỖI BINDING: Đọc file từ product.ImageFile ***
            if (product.ImageFile != null && product.ImageFile.ContentLength > 0)
            {
                // *** THÊM MỚI: Kiểm tra kích thước file ***
                if (product.ImageFile.ContentLength > MAX_FILE_SIZE)
                {
                    ModelState.AddModelError("ImageFile", "Ảnh không được vượt quá 5MB.");
                    // Trả về View mà không làm sập server
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

            // Chỉ lưu nếu Model hợp lệ (bao gồm cả lỗi kích thước file ở trên)
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
            // *** VẪN GIỮ: Vô hiệu hóa Logger.Log để tránh sập server ***
            // Logger.Log("Product Creation Error: " + ex.ToString());

            ModelState.AddModelError("", "LỖI HỆ THỐNG KHI LƯU FILE: " + ex.Message);
        }

        // Quay lại View nếu có lỗi
        return View(product);
    }

    // GET: /Product/Edit/id
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
    // *** SỬA LỖI BINDING: Xóa tham số 'ImageFile' thừa ***
    public ActionResult Edit(Product product)
    {
        if (Session["Role"]?.ToString() != "Admin")
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            // *** SỬA LỖI BINDING: Đọc file từ product.ImageFile ***
            if (product.ImageFile != null && product.ImageFile.ContentLength > 0)
            {
                // *** THÊM MỚI: Kiểm tra kích thước file ***
                if (product.ImageFile.ContentLength > MAX_FILE_SIZE)
                {
                    ModelState.AddModelError("ImageFile", "Ảnh không được vượt quá 5MB.");
                    return View(product);
                }

                // Lấy sản phẩm cũ để xóa ảnh (nếu có)
                var existingProduct = db.Products.AsNoTracking().FirstOrDefault(p => p.ProductID == product.ProductID);
                if (existingProduct != null && !string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    string oldPath = Server.MapPath(existingProduct.ImageUrl);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                // Lưu ảnh mới
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
            // (Không có 'else' ở đây, nếu không upload ảnh mới thì giữ nguyên ảnh cũ đã được bind)

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

    // GET: /Product/Delete/id
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