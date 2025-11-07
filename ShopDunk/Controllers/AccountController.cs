using ShopDunk.Models;
using System.Linq;
using System.Web.Mvc;

public class AccountController : Controller
{
    private AppDbContext db = new AppDbContext();

    public ActionResult Login() => View();

    [HttpPost]
    public ActionResult Login(string username, string password)
    {
        var user = db.Users.FirstOrDefault(u => u.Username == username);
        if (user != null && user.PasswordHash == password) // nên dùng mã hóa
        {
            Session["UserID"] = user.UserID;
            Session["Username"] = user.Username;
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
        return View();
    }

    public ActionResult Logout()
    {
        Session.Clear();
        return RedirectToAction("Login");
    }
    public ActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Register(User user)
    {
        if (ModelState.IsValid)
        {
            using (var db = new AppDbContext())
            {
                var existing = db.Users.FirstOrDefault(u => u.Username == user.Username);
                if (existing != null)
                {
                    ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                    return View();
                }

                // Lưu ý: nên mã hóa mật khẩu trước khi lưu
                db.Users.Add(user);
                db.SaveChanges();

                TempData["Success"] = "Đăng ký thành công. Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
        }

        return View();
    }
}