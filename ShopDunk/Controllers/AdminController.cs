using ShopDunk.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Data.Entity;
using System.Web;
using System.Collections.Generic;
using System.IO;

public class AdminController : Controller
{
    private AppDbContext db = new AppDbContext();

    private bool IsAdmin()
    {
        return Session["Role"]?.ToString() == "Admin";
    }

    // (Giữ nguyên các action: Index, Products, Orders, OrderDetails, UpdateOrderStatus, Users, EditUser, DeleteUser)
    // ...
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

    // Quản lý Sản phẩm
    public ActionResult Products()
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        return RedirectToAction("Index", "Product");
    }

    // Quản lý Đơn hàng
    public ActionResult Orders(string statusFilter, int page = 1, int pageSize = 10)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");

        var ordersQuery = db.Orders.Include(o => o.User).AsQueryable();

        if (!string.IsNullOrEmpty(statusFilter))
        {
            ordersQuery = ordersQuery.Where(o => o.Status == statusFilter);
        }

        int totalOrders = ordersQuery.Count();
        var orders = ordersQuery.OrderByDescending(o => o.OrderDate)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
        ViewBag.StatusFilter = statusFilter;
        ViewBag.StatusList = new SelectList(new[] { "Chờ xử lý", "Đang giao", "Đã giao", "Đã hủy" });


        return View(orders);
    }

    // Chi tiết đơn hàng
    public ActionResult OrderDetails(int id)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");

        var order = db.Orders.Include(o => o.User).Include(o => o.OrderDetails.Select(od => od.Product))
                            .FirstOrDefault(o => o.OrderID == id);

        if (order == null) return HttpNotFound();

        ViewBag.StatusList = new SelectList(new[] { "Chờ xử lý", "Đang giao", "Đã giao", "Đã hủy" }, order.Status);

        return View(order);
    }

    // Cập nhật trạng thái đơn hàng
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


    // Quản lý Người dùng
    public ActionResult Users(string q, int page = 1, int pageSize = 10)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");

        var usersQuery = db.Users.AsQueryable();

        if (!string.IsNullOrEmpty(q))
        {
            usersQuery = usersQuery.Where(u => u.Username.Contains(q) || u.Email.Contains(q));
        }

        int totalUsers = usersQuery.Count();

        var users = usersQuery
            .OrderByDescending(u => u.Role == "Admin")
            .ThenBy(u => u.UserID)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
        ViewBag.Query = q;

        return View(users);
    }

    // GET: /Admin/EditUser/5
    public ActionResult EditUser(int id)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");

        var user = db.Users.Find(id);
        if (user == null) return HttpNotFound();

        var model = new User
        {
            UserID = user.UserID,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
        };

        ViewBag.Roles = new SelectList(new[] { "Admin", "User" }, user.Role);
        return View(model);
    }

    // POST: /Admin/EditUser/5
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

    // POST: /Admin/DeleteUser/5
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

    // --- BẮT ĐẦU CẬP NHẬT (Thêm filterKey) ---
    // GET: /Admin/Sliders
    public ActionResult Sliders(string filterKey = "all")
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        ViewBag.Title = "Quản lý Slider";

        // Lấy danh sách Key để tạo dropdown bộ lọc
        var allKeys = db.SliderImages
            .Select(s => s.CategoryKey)
            .Distinct()
            .ToList();

        // Gửi SelectList và giá trị đang lọc về View
        ViewBag.CategoryKeys = new SelectList(allKeys, filterKey);
        ViewBag.CurrentFilter = filterKey;

        // Lọc danh sách slider
        var slidersQuery = db.SliderImages.AsQueryable();
        if (filterKey != "all" && !string.IsNullOrEmpty(filterKey))
        {
            slidersQuery = slidersQuery.Where(s => s.CategoryKey == filterKey);
        }

        var sliders = slidersQuery.OrderBy(s => s.CategoryKey).ToList();
        return View(sliders);
    }
    // --- KẾT THÚC CẬP NHẬT ---

    // GET: /Admin/CreateSlider
    public ActionResult CreateSlider()
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        ViewBag.Title = "Thêm Slider mới";
        var model = new SliderImage();

        ViewBag.ExistingKeys = db.SliderImages
            .Select(s => s.CategoryKey)
            .Distinct()
            .ToList();

        return View(model);
    }

    // POST: /Admin/CreateSlider
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
                string directoryPath = Server.MapPath("~/Images/Banners");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                foreach (var file in model.ImageFiles)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(file.FileName);
                        string extension = Path.GetExtension(fileName);
                        string newFileName = Guid.NewGuid().ToString() + extension;
                        string path = Path.Combine(directoryPath, newFileName);

                        file.SaveAs(path);

                        var newSlider = new SliderImage
                        {
                            CategoryKey = model.CategoryKey,
                            Title = model.Title,
                            IsActive = model.IsActive,
                            ImageUrl = "/Images/Banners/" + newFileName
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
        ViewBag.ExistingKeys = db.SliderImages.Select(s => s.CategoryKey).Distinct().ToList();
        return View(model);
    }


    // GET: /Admin/EditSlider/5
    public ActionResult EditSlider(int id)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");

        var slider = db.SliderImages.Find(id);
        if (slider == null) return HttpNotFound();

        ViewBag.Title = "Sửa Slider";
        ViewBag.ExistingKeys = db.SliderImages.Select(s => s.CategoryKey).Distinct().ToList();
        return View(slider);
    }

    // POST: /Admin/EditSlider/5
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
                var oldSlider = db.SliderImages.AsNoTracking().FirstOrDefault(s => s.SliderImageID == model.SliderImageID);
                if (oldSlider != null && !string.IsNullOrEmpty(oldSlider.ImageUrl))
                {
                    string oldPath = Server.MapPath(oldSlider.ImageUrl);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                string fileName = Path.GetFileName(newImageFile.FileName);
                string extension = Path.GetExtension(fileName);
                string newFileName = Guid.NewGuid().ToString() + extension;

                string directoryPath = Server.MapPath("~/Images/Banners");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string path = Path.Combine(directoryPath, newFileName);
                newImageFile.SaveAs(path);
                model.ImageUrl = "/Images/Banners/" + newFileName;
            }

            if (ModelState.IsValid)
            {
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật slider thành công!";
                return RedirectToAction("Sliders");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
        }

        ViewBag.Title = "Sửa Slider";
        ViewBag.ExistingKeys = db.SliderImages.Select(s => s.CategoryKey).Distinct().ToList();
        return View(model);
    }

    // POST: /Admin/DeleteSlider/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteSlider(int id)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");

        var slider = db.SliderImages.Find(id);
        if (slider == null) return HttpNotFound();

        if (!string.IsNullOrEmpty(slider.ImageUrl))
        {
            string path = Server.MapPath(slider.ImageUrl);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

        db.SliderImages.Remove(slider);
        db.SaveChanges();
        TempData["Success"] = "Xóa slider thành công.";
        return RedirectToAction("Sliders");
    }

    #endregion

    // Hàm băm mật khẩu
    private string HashPassword(string password)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
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