using ShopDunk.Models;
using System;
using System.Data.Entity;
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
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Product product)
    {
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
                product.ImageFile.SaveAs(fullPath);
                product.ImageUrl = "/Images/products/" + fileName;
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi khi lưu ảnh: " + ex.Message);
        }

        if (ModelState.IsValid)
        {
            db.Products.Add(product);
            db.SaveChanges();
            TempData["Success"] = "Thêm sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        return View(product);
    }

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
            try
            {
                // Lấy sản phẩm hiện tại từ database
                var existingProduct = db.Products.Find(product.ProductID);
                if (existingProduct == null)
                {
                    return HttpNotFound();
                }

                // Xử lý upload ảnh mới (nếu có)
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
                ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
            }
        }

        return View(product);
    }

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