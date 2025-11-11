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
        System.Diagnostics.Debug.WriteLine("Hash nhập vào: " + hash);
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
    public ActionResult Register(User user)
    {
        if (ModelState.IsValid)
        {
            var existing = db.Users.FirstOrDefault(u => u.Username == user.Username);
            if (existing != null)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View();
            }

            user.PasswordHash = HashPassword(user.Password);

            // Nếu đăng ký tài khoản tên "admin", gán quyền Admin
            if (user.Username.ToLower() == "admin")
                user.Role = "Admin";

            db.Users.Add(user);
            db.SaveChanges();

            TempData["Success"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        return View(user);
    }

    public ActionResult AccessDenied() => View();

    private string HashPassword(string password)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}