using ShopDunk.Models;
using ShopDunk.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

public class ProductController : Controller
{
    private AppDbContext db = new AppDbContext();

    // GET: /Product/Index (Quản lý sản phẩm cho Admin)
    public ActionResult Index()
    {
        // Kiểm tra Admin
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
    public ActionResult Create(Product product, HttpPostedFileBase ImageFile)
    {
        if (Session["Role"]?.ToString() != "Admin")
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(ImageFile.FileName);
                    string extension = Path.GetExtension(fileName);
                    string newFileName = Guid.NewGuid().ToString() + extension;
                    string path = Path.Combine(Server.MapPath("~/Images/Products"), newFileName);

                    ImageFile.SaveAs(path);
                    product.ImageUrl = "/Images/Products/" + newFileName;
                }
                else
                {
                    // Nếu không có ảnh, dùng ảnh mặc định hoặc để null
                    product.ImageUrl = null;
                }

                db.Products.Add(product);
                db.SaveChanges();
                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi khi thêm sản phẩm: " + ex.Message);
            Logger.Log("Product Creation Error: " + ex.ToString());
        }

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
    public ActionResult Edit(Product product, HttpPostedFileBase ImageFile)
    {
        if (Session["Role"]?.ToString() != "Admin")
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingProduct = db.Products.AsNoTracking().FirstOrDefault(p => p.ProductID == product.ProductID);

                // Xử lý upload ảnh mới
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    // Xóa ảnh cũ (nếu có)
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        string oldPath = Server.MapPath(existingProduct.ImageUrl);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // Lưu ảnh mới
                    string fileName = Path.GetFileName(ImageFile.FileName);
                    string extension = Path.GetExtension(fileName);
                    string newFileName = Guid.NewGuid().ToString() + extension;
                    string path = Path.Combine(Server.MapPath("~/Images/Products"), newFileName);

                    ImageFile.SaveAs(path);
                    product.ImageUrl = "/Images/Products/" + newFileName;
                }
                else
                {
                    // Giữ lại ảnh cũ nếu không upload ảnh mới
                    product.ImageUrl = existingProduct.ImageUrl;
                }

                db.Entry(product).State = EntityState.Modified;
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