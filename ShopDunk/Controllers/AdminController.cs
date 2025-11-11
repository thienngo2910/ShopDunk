using ShopDunk.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

public class AdminController : Controller
{
 private AppDbContext db = new AppDbContext();

 private bool IsAdmin()
 {
 return Session["Role"]?.ToString() == "Admin";
 }

 public ActionResult Index()
 {
 if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
 return View();
 }

 public ActionResult Products()
 {
 if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
 var products = db.Products.ToList();
 return View(products);
 }

 public ActionResult Users(string q, int page =1, int pageSize =10)
 {
 if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
 var usersQuery = db.Users.AsQueryable();
 if (!string.IsNullOrEmpty(q))
 {
 usersQuery = usersQuery.Where(u => u.Username.Contains(q) || u.Email.Contains(q));
 ViewBag.Query = q;
 }
 int total = usersQuery.Count();
 var users = usersQuery.OrderBy(u => u.Username)
 .Skip((page -1) * pageSize)
 .Take(pageSize)
 .ToList();
 ViewBag.Page = page;
 ViewBag.PageSize = pageSize;
 ViewBag.Total = total;
 return View(users);
 }

 public ActionResult EditUser(int id)
 {
 if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
 var user = db.Users.Find(id);
 if (user == null) return HttpNotFound();
 return View(user);
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

 // Prevent changing username to an existing one
 if (!string.Equals(user.Username, model.Username, StringComparison.OrdinalIgnoreCase))
 {
 if (db.Users.Any(u => u.Username == model.Username && u.UserID != model.UserID))
 {
 ModelState.AddModelError("Username", "Tên ??ng nh?p ?ã t?n t?i.");
 return View(model);
 }
 user.Username = model.Username;
 }

 user.Email = model.Email;
 user.Role = model.Role;

 db.SaveChanges();
 TempData["Success"] = "C?p nh?t ng??i dùng thành công.";
 return RedirectToAction("Users");
 }
 return View(model);
 }

 public ActionResult ChangePassword(int id)
 {
 if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
 var user = db.Users.Find(id);
 if (user == null) return HttpNotFound();
 var vm = new ChangePasswordViewModel { UserID = user.UserID };
 return View(vm);
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public ActionResult ChangePassword(ChangePasswordViewModel model)
 {
 if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
 if (!ModelState.IsValid) return View(model);
 var user = db.Users.Find(model.UserID);
 if (user == null) return HttpNotFound();
 user.PasswordHash = HashPassword(model.NewPassword);
 db.SaveChanges();
 TempData["Success"] = "??i m?t kh?u thành công.";
 return RedirectToAction("Users");
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public ActionResult DeleteUser(int id)
 {
 if (!IsAdmin()) return RedirectToAction("AccessDenied", "Account");
 var user = db.Users.Find(id);
 if (user == null) return HttpNotFound();

 // Prevent deleting currently logged in admin
 var currentUser = Session["Username"]?.ToString();
 if (string.Equals(currentUser, user.Username, StringComparison.OrdinalIgnoreCase))
 {
 TempData["Error"] = "B?n không th? xóa tài kho?n ?ang ??ng nh?p.";
 return RedirectToAction("Users");
 }

 db.Users.Remove(user);
 db.SaveChanges();
 TempData["Success"] = "Xóa ng??i dùng thành công.";
 return RedirectToAction("Users");
 }

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