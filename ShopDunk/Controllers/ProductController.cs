using ShopDunk.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;

public class ProductController : Controller
{
    private AppDbContext db = new AppDbContext();

    // Hiển thị sản phẩm chia theo danh mục
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

    // Hiển thị form thêm sản phẩm
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