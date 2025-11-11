using ShopDunk.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Data.Entity; // Cần thêm để dùng EntityState

public class AdminController : Controller
{
    private AppDbContext db = new AppDbContext();

    private bool IsAdmin()
    {
        // Kiểm tra xem người dùng có đang đăng nhập và có Role là "Admin" hay không
        return Session["Role"]?.ToString() == "Admin";
    }

    // Trang chủ Admin (Dashboard)
    public ActionResult Index()
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");

        // Thống kê cho dashboard
        ViewBag.TotalUsers = db.Users.Count();
        ViewBag.TotalProducts = db.Products.Count();
        ViewBag.TotalOrders = db.Orders.Count();
        ViewBag.TotalRevenue = db.Orders.Where(o => o.Status == "Đã giao").Sum(o => (decimal?)o.TotalAmount) ?? 0; // Chỉ tính doanh thu đơn đã giao
        ViewBag.PendingOrders = db.Orders.Count(o => o.Status == "Chờ xử lý");

        return View();
    }

    // Quản lý Sản phẩm
    public ActionResult Products()
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
        // Chức năng quản lý sản phẩm chi tiết nằm ở ProductController (Index, Create, Edit, Delete)
        return RedirectToAction("Index", "Product"); // Chuyển hướng sang ProductController/Index
    }

    // Quản lý Đơn hàng (MỚI)
    public ActionResult Orders(string statusFilter, int page = 1, int pageSize = 10)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");

        var ordersQuery = db.Orders.Include(o => o.User).AsQueryable();

        // Lọc theo trạng thái
        if (!string.IsNullOrEmpty(statusFilter))
        {
            ordersQuery = ordersQuery.Where(o => o.Status == statusFilter);
        }

        // Phân trang
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

    // Chi tiết đơn hàng (MỚI)
    public ActionResult OrderDetails(int id)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");

        var order = db.Orders.Include(o => o.User).Include(o => o.OrderDetails.Select(od => od.Product))
                            .FirstOrDefault(o => o.OrderID == id);

        if (order == null) return HttpNotFound();

        ViewBag.StatusList = new SelectList(new[] { "Chờ xử lý", "Đang giao", "Đã giao", "Đã hủy" }, order.Status);

        return View(order);
    }

    // Cập nhật trạng thái đơn hàng (MỚI)
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

        // Tìm kiếm
        if (!string.IsNullOrEmpty(q))
        {
            usersQuery = usersQuery.Where(u => u.Username.Contains(q) || u.Email.Contains(q));
        }

        // Phân trang
        int totalUsers = usersQuery.Count();
        var users = usersQuery.OrderBy(u => u.UserID)
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

        // Tạo ViewModel để tránh xung đột với các trường NotMapped (Password, ConfirmPassword)
        var model = new User
        {
            UserID = user.UserID,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            // PasswordHash không cần thiết phải hiển thị/sửa trực tiếp
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

            // Cập nhật thông tin người dùng
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

    // GET: /Admin/ChangeUserPassword/5
    public ActionResult ChangeUserPassword(int id)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");

        var user = db.Users.Find(id);
        if (user == null) return HttpNotFound();

        var vm = new ChangePasswordViewModel { UserID = id };
        ViewBag.Username = user.Username;

        return View(vm);
    }

    // POST: /Admin/ChangeUserPassword/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ChangeUserPassword(ChangePasswordViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");

        if (!ModelState.IsValid)
        {
            var user = db.Users.Find(model.UserID);
            ViewBag.Username = user?.Username;
            return View(model);
        }

        var targetUser = db.Users.Find(model.UserID);
        if (targetUser == null) return HttpNotFound();

        // Ngăn chặn Admin tự đổi mật khẩu cho mình thông qua trang này (nên dùng Account/ChangePassword)
        if (targetUser.UserID == (int?)Session["UserID"])
        {
            TempData["Error"] = "Vui lòng dùng chức năng đổi mật khẩu cá nhân.";
            return RedirectToAction("Users");
        }

        targetUser.PasswordHash = HashPassword(model.NewPassword);
        db.SaveChanges();

        TempData["Success"] = "Đổi mật khẩu thành công.";
        return RedirectToAction("Users");
    }

    // POST: /Admin/DeleteUser/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteUser(int id)
    {
        if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");

        var user = db.Users.Find(id);
        if (user == null) return HttpNotFound();

        // Ngăn chặn xóa tài khoản Admin đang đăng nhập
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

    // Hàm băm mật khẩu (được copy từ AccountController)
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