using ShopDunk.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Collections.Generic;
using System.IO;

public class AdminController : Controller
{
    private AppDbContext db = new AppDbContext();

    private bool IsAdmin()
    {
        return Session["Role"]?.ToString() == "Admin";
    }

    private byte[] ConvertToBytes(HttpPostedFileBase image)
    {
        if (image == null) return null;
        using (var reader = new BinaryReader(image.InputStream))
        {
            return reader.ReadBytes((int)image.ContentLength);
        }
    }

    // --- HÀM HELPER MỚI: Lấy danh sách Key cho Dropdown ---
    private SelectList GetCategoryKeyList(string selectedKey = null)
    {
        // 1. Tạo danh sách mặc định có "Home"
        var keys = new List<string> { "Home" };

        // 2. Lấy thêm các danh mục từ bảng Product (iPhone, iPad...)
        var productCategories = db.Products
                                  .Select(p => p.Category)
                                  .Where(c => c != null && c != "")
                                  .Distinct()
                                  .ToList();

        keys.AddRange(productCategories);

        // 3. Trả về SelectList
        return new SelectList(keys, selectedKey);
    }
    // ------------------------------------------------------

    // Trang chủ Admin (Dashboard)
    public ActionResult Index()
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        ViewBag.TotalUsers = db.Users.Count();
        ViewBag.TotalProducts = db.Products.Count();
        ViewBag.TotalOrders = db.Orders.Count();
        ViewBag.TotalRevenue = db.Orders.Where(o => o.Status == "Đã giao").Sum(o => (decimal?)o.TotalAmount) ?? 0;
        ViewBag.PendingOrders = db.Orders.Count(o => o.Status == "Chờ xử lý");
        return View();
    }

    public ActionResult Products()
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        return RedirectToAction("Index", "Product");
    }

    public ActionResult Orders(string statusFilter, int page = 1, int pageSize = 10)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        var ordersQuery = db.Orders.Include(o => o.User).AsQueryable();
        if (!string.IsNullOrEmpty(statusFilter))
        {
            ordersQuery = ordersQuery.Where(o => o.Status == statusFilter);
        }
        int totalOrders = ordersQuery.Count();
        var orders = ordersQuery.OrderByDescending(o => o.OrderDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
        ViewBag.StatusFilter = statusFilter;
        ViewBag.StatusList = new SelectList(new[] { "Chờ xử lý", "Đang giao", "Đã giao", "Đã hủy" });
        return View(orders);
    }

    public ActionResult OrderDetails(int id)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        var order = db.Orders.Include(o => o.User).Include(o => o.OrderDetails.Select(od => od.Product)).FirstOrDefault(o => o.OrderID == id);
        if (order == null) return HttpNotFound();
        ViewBag.StatusList = new SelectList(new[] { "Chờ xử lý", "Đang giao", "Đã giao", "Đã hủy" }, order.Status);
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult UpdateOrderStatus(int orderID, string status)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        var order = db.Orders.Find(orderID);
        if (order == null) return HttpNotFound();
        order.Status = status;
        db.Entry(order).State = EntityState.Modified;
        db.SaveChanges();
        TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công.";
        return RedirectToAction("OrderDetails", new { id = orderID });
    }

    public ActionResult Users(string q, int page = 1, int pageSize = 10)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        var usersQuery = db.Users.AsQueryable();
        if (!string.IsNullOrEmpty(q))
        {
            usersQuery = usersQuery.Where(u => u.Username.Contains(q) || u.Email.Contains(q));
        }
        int totalUsers = usersQuery.Count();
        var users = usersQuery.OrderByDescending(u => u.Role == "Admin").ThenBy(u => u.UserID).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
        ViewBag.Query = q;
        return View(users);
    }

    public ActionResult EditUser(int id)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        var user = db.Users.Find(id);
        if (user == null) return HttpNotFound();
        var model = new User { UserID = user.UserID, Username = user.Username, Email = user.Email, Role = user.Role };
        ViewBag.Roles = new SelectList(new[] { "Admin", "User" }, user.Role);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult EditUser(User model)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        if (ModelState.IsValid)
        {
            var user = db.Users.Find(model.UserID);
            if (user == null) return HttpNotFound();
            user.Username = model.Username;
            user.Email = model.Email;
            user.Role = model.Role;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Cập nhật người dùng thành công.";
            return RedirectToAction("Users");
        }
        ViewBag.Roles = new SelectList(new[] { "Admin", "User" }, model.Role);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteUser(int id)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        var user = db.Users.Find(id);
        if (user == null) return HttpNotFound();
        var currentUsername = Session["Username"]?.ToString();
        if (string.Equals(currentUsername, user.Username, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Bạn không thể xóa tài khoản đang đăng nhập.";
            return RedirectToAction("Users");
        }
        db.Users.Remove(user);
        db.SaveChanges();
        TempData["Success"] = "Xóa người dùng thành công.";
        return RedirectToAction("Users");
    }

    #region Quản lý Slider

    public ActionResult Sliders(string filterKey = "all")
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        ViewBag.Title = "Quản lý Slider";
        var allKeys = db.SliderImages.Select(s => s.CategoryKey).Distinct().ToList();
        ViewBag.CategoryKeys = new SelectList(allKeys, filterKey);
        var slidersQuery = db.SliderImages.AsQueryable();
        if (filterKey != "all" && !string.IsNullOrEmpty(filterKey))
        {
            slidersQuery = slidersQuery.Where(s => s.CategoryKey == filterKey);
        }
        return View(slidersQuery.OrderBy(s => s.CategoryKey).ToList());
    }

    // GET: CreateSlider
    public ActionResult CreateSlider()
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        ViewBag.Title = "Thêm Slider mới";

        // --- SỬA LỖI: Dùng hàm helper mới ---
        ViewBag.CategoryKeyList = GetCategoryKeyList();

        return View(new SliderImage());
    }

    // POST: CreateSlider
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult CreateSlider(SliderImage model)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");

        if (model.ImageFiles == null || !model.ImageFiles.Any(f => f != null && f.ContentLength > 0))
        {
            ModelState.AddModelError("ImageFiles", "Vui lòng chọn ít nhất một ảnh.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                int successfulUploads = 0;
                foreach (var file in model.ImageFiles)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        var newSlider = new SliderImage
                        {
                            CategoryKey = model.CategoryKey,
                            Title = model.Title,
                            IsActive = model.IsActive,
                            ImageData = ConvertToBytes(file)
                        };
                        db.SliderImages.Add(newSlider);
                        successfulUploads++;
                    }
                }
                db.SaveChanges();
                TempData["Success"] = $"Đã thêm {successfulUploads} slider thành công!";
                return RedirectToAction("Sliders");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu: " + ex.Message);
            }
        }

        ViewBag.Title = "Thêm Slider mới";
        // --- SỬA LỖI: Gửi lại danh sách nếu lỗi ---
        ViewBag.CategoryKeyList = GetCategoryKeyList(model.CategoryKey);
        return View(model);
    }

    // GET: EditSlider
    public ActionResult EditSlider(int id)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        ViewBag.Title = "Sửa Slider";
        var slider = db.SliderImages.Find(id);
        if (slider == null) return HttpNotFound();

        // --- SỬA LỖI: Dùng hàm helper mới ---
        ViewBag.CategoryKeyList = GetCategoryKeyList(slider.CategoryKey);

        return View(slider);
    }

    // POST: EditSlider
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult EditSlider(SliderImage model)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");

        try
        {
            var newImageFile = model.ImageFiles?.FirstOrDefault();
            if (newImageFile != null && newImageFile.ContentLength > 0)
            {
                model.ImageData = ConvertToBytes(newImageFile);
            }
            else
            {
                var old = db.SliderImages.AsNoTracking().FirstOrDefault(s => s.SliderImageID == model.SliderImageID);
                model.ImageData = old?.ImageData;
            }

            if (ModelState.IsValid)
            {
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction("Sliders");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
        }

        ViewBag.Title = "Sửa Slider";
        // --- SỬA LỖI: Gửi lại danh sách nếu lỗi ---
        ViewBag.CategoryKeyList = GetCategoryKeyList(model.CategoryKey);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteSlider(int id)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        var slider = db.SliderImages.Find(id);
        if (slider != null)
        {
            db.SliderImages.Remove(slider);
            db.SaveChanges();
            TempData["Success"] = "Xóa thành công.";
        }
        return RedirectToAction("Sliders");
    }
    #endregion

    protected override void Dispose(bool disposing)
    {
        if (disposing) { db.Dispose(); }
        base.Dispose(disposing);
    }
}