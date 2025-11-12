using ShopDunk.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

public class CartController : Controller
{
    private AppDbContext db = new AppDbContext();

    // GET: /Cart/Index
    public ActionResult Index()
    {
        if (Session["UserID"] == null)
        {
            TempData["Error"] = "Vui lòng đăng nhập để xem giỏ hàng.";
            return RedirectToAction("Login", "Account");
        }

        int userId = (int)Session["UserID"];
        var cartItems = db.CartItems.Include(c => c.Product).Where(c => c.UserID == userId).ToList();
        return View(cartItems);
    }

    // *** SỬA LỖI: Đổi tên tham số 'productId' thành 'id' ***
    // GET: /Cart/Add/id
    public ActionResult Add(int id) // <-- ĐÃ SỬA TẠI ĐÂY
    {
        if (Session["UserID"] == null)
        {
            TempData["Error"] = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng.";
            return RedirectToAction("Login", "Account");
        }

        int userId = (int)Session["UserID"];

        // *** SỬA LỖI: Dùng 'id' để tìm kiếm ***
        var item = db.CartItems.FirstOrDefault(c => c.ProductID == id && c.UserID == userId);

        if (item != null)
        {
            item.Quantity++;
        }
        else
        {
            // *** SỬA LỖI: Dùng 'id' để thêm mới ***
            db.CartItems.Add(new CartItem { ProductID = id, UserID = userId, Quantity = 1 });
        }

        db.SaveChanges();
        TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng.";
        return RedirectToAction("Index", "Cart"); // Chuyển hướng về trang giỏ hàng
    }

    // GET: /Cart/Remove/id (CartItemID)
    public ActionResult Remove(int id)
    {
        if (Session["UserID"] == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var item = db.CartItems.Find(id);
        if (item != null)
        {
            // Đảm bảo chỉ người dùng sở hữu mới có thể xóa
            if (item.UserID == (int)Session["UserID"])
            {
                db.CartItems.Remove(item);
                db.SaveChanges();
                TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            }
            else
            {
                return RedirectToAction("AccessDenied", "Account");
            }
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