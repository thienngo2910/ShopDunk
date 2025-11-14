using ShopDunk.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;

public class AccountController : Controller
{
    private AppDbContext db = new AppDbContext();

    // --- CẬP NHẬT (Thêm returnUrl) ---
    [HttpGet] // (Thêm [HttpGet] cho rõ ràng)
    public ActionResult Login(string returnUrl)
    {
        // Gửi returnUrl này đến View để Form có thể POST ngược lại
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // --- CẬP NHẬT (Thêm returnUrl) ---
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Login(string username, string password, string returnUrl)
    {
        username = username.Trim();
        string hash = HashPassword(password);

        var user = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == hash);

        if (user != null)
        {
            FormsAuthentication.SetAuthCookie(user.Username, false);
            Session["UserID"] = user.UserID;
            Session["Username"] = user.Username;
            Session["Role"] = user.Role;

            // --- LOGIC CHUYỂN HƯỚNG MỚI ---
            if (Url.IsLocalUrl(returnUrl))
            {
                // Nếu có returnUrl hợp lệ, trả về trang đó
                return Redirect(returnUrl);
            }
            else
            {
                // Nếu không, về trang chủ
                return RedirectToAction("Index", "Home");
            }
            // --- KẾT THÚC LOGIC MỚI ---
        }

        ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
        ViewBag.ReturnUrl = returnUrl; // Gửi lại returnUrl nếu đăng nhập thất bại
        return View();
    }

    // (Giữ nguyên các Action: Logout, Register...)
    public ActionResult Logout()
    {
        FormsAuthentication.SignOut();
        Session.Clear();
        return RedirectToAction("Login");
    }

    [HttpGet] // (Thêm [HttpGet] cho rõ ràng)
    public ActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var existing = db.Users.FirstOrDefault(u => u.Username == model.Username);
            if (existing != null)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.";
                return View(model);
            }

            var newUser = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = "User"
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            TempData["Success"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        return View(model);
    }

    [HttpGet] // (Thêm [HttpGet] cho rõ ràng)
    public ActionResult AccessDenied() => View();

    [HttpGet] // (Thêm [HttpGet] cho rõ ràng)
    public ActionResult ChangePassword()
    {
        if (Session["UserID"] == null)
        {
            TempData["Error"] = "Vui lòng đăng nhập để đổi mật khẩu.";
            return RedirectToAction("Login");
        }

        var vm = new ChangePasswordViewModel { UserID = (int)Session["UserID"] };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ChangePassword(ChangePasswordViewModel model)
    {
        if (Session["UserID"] == null)
        {
            TempData["Error"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
            return RedirectToAction("Login");
        }

        int currentUserId = (int)Session["UserID"];
        if (model.UserID != currentUserId)
        {
            return RedirectToAction("AccessDenied");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = db.Users.Find(model.UserID);
        if (user == null) return HttpNotFound();

        user.PasswordHash = HashPassword(model.NewPassword);
        db.SaveChanges();

        TempData["Success"] = "Đổi mật khẩu thành công.";
        return RedirectToAction("Index", "Home");
    }

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