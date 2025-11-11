using ShopDunk.Models;
using ShopDunk.Helpers;
using System;
using System.IO;
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

    public ActionResult Category(string name)
    {
        var products = db.Products
                         .Where(p => p.Category != null && p.Category.ToLower() == name.ToLower())
                         .ToList();

        ViewBag.CategoryName = name;
        return View(products);
    }

    public ActionResult Create()
    {
        if (Session["Role"]?.ToString() != "Admin")
        {
            return RedirectToAction("AccessDenied", "Account");
        }
        return View();
    }

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
                string originalName = Path.GetFileNameWithoutExtension(product.ImageFile.FileName);
                string extension = Path.GetExtension(product.ImageFile.FileName);

                // Loại bỏ ký tự đặc biệt
                string safeName = System.Text.RegularExpressions.Regex.Replace(originalName, @"[^a-zA-Z0-9]", "_");

                // Tạo tên file duy nhất
                string fileName = safeName + "_" + Guid.NewGuid() + extension;

                string folderPath = Server.MapPath("~/Images/products");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fullPath = Path.Combine(folderPath, fileName);

                // Save file (cần quyền ghi)
                product.ImageFile.SaveAs(fullPath);
                product.ImageUrl = "/Images/products/" + fileName;
            }
        }
        catch (Exception ex)
        {
            // Log chi tiết để kiểm tra (App_Data/logs.txt)
            Logger.Log("Error saving image in Product/Create: " + ex.ToString());
            ModelState.AddModelError("", "Lỗi khi lưu ảnh: " + ex.Message);
        }

        if (ModelState.IsValid)
        {
            db.Products.Add(product);
            try
            {
                db.SaveChanges();
                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Ghi log đầy đủ để xem nguyên nhân (constraint, connection, timeout...)
                Logger.Log("Error SaveChanges in Product/Create: " + ex.ToString());
                ModelState.AddModelError("", "Có lỗi khi lưu vào cơ sở dữ liệu. Vui lòng kiểm tra logs trên server.");
            }
        }

        return View(product);
    }

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(Product product)
    {
        if (Session["Role"]?.ToString() != "Admin")
        {
            return RedirectToAction("AccessDenied", "Account");
        }
        if (ModelState.IsValid)
        {
            try
            {
                var existingProduct = db.Products.Find(product.ProductID);
                if (existingProduct == null)
                {
                    return HttpNotFound();
                }

                if (product.ImageFile != null && product.ImageFile.ContentLength > 0)
                {
                    string originalName = Path.GetFileNameWithoutExtension(product.ImageFile.FileName);
                    string extension = Path.GetExtension(product.ImageFile.FileName);
                    string safeName = System.Text.RegularExpressions.Regex.Replace(originalName, @"[^a-zA-Z0-9]", "_");
                    string fileName = safeName + "_" + Guid.NewGuid() + extension;

                    string folderPath = Server.MapPath("~/Images/products");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string fullPath = Path.Combine(folderPath, fileName);
                    product.ImageFile.SaveAs(fullPath);

                    // Xóa ảnh cũ (nếu có)
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        string oldImagePath = Server.MapPath(existingProduct.ImageUrl);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    existingProduct.ImageUrl = "/Images/products/" + fileName;
                }

                // Cập nhật các trường khác
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.Category = product.Category;

                db.SaveChanges();
                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Log("Error in Product/Edit: " + ex.ToString());
                ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
            }
        }

        return View(product);
    }

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
            // Xóa ảnh (nếu có)
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