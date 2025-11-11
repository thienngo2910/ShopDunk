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

    public ActionResult Login() => View();

    [HttpPost]
    public ActionResult Login(string username, string password)
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
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
        return View();
    }

    public ActionResult Logout()
    {
        FormsAuthentication.SignOut();
        Session.Clear();
        return RedirectToAction("Login");
    }

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
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View(model);
            }

            var user = new User
            {
                Username = model.Username.Trim(),
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = model.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User"
            };

            db.Users.Add(user);
            db.SaveChanges();

            TempData["Success"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        return View(model);
    }

    public ActionResult AccessDenied() => View();

    public ActionResult ChangePassword()
    {
        if (Session["UserID"] == null)
        {
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

    private string HashPassword(string password)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}