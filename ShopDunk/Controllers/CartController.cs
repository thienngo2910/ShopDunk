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

    // GET: /Cart/Add/id
    public ActionResult Add(int id)
    {
        if (Session["UserID"] == null)
        {
            TempData["Error"] = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng.";
            return RedirectToAction("Login", "Account");
        }

        int userId = (int)Session["UserID"];
        var item = db.CartItems.FirstOrDefault(c => c.ProductID == id && c.UserID == userId);

        if (item != null)
        {
            item.Quantity++;
        }
        else
        {
            db.CartItems.Add(new CartItem { ProductID = id, UserID = userId, Quantity = 1 });
        }

        db.SaveChanges();
        TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng.";
        return RedirectToAction("Index", "Cart");
    }

    // --- BẮT ĐẦU ACTION MỚI ---

    // GET: /Cart/IncreaseQuantity/id (id ở đây là CartItemID)
    public ActionResult IncreaseQuantity(int id)
    {
        if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

        var item = db.CartItems.Find(id);
        if (item != null && item.UserID == (int)Session["UserID"])
        {
            item.Quantity++;
            db.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    // GET: /Cart/DecreaseQuantity/id (id ở đây là CartItemID)
    public ActionResult DecreaseQuantity(int id)
    {
        if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

        var item = db.CartItems.Find(id);
        if (item != null && item.UserID == (int)Session["UserID"])
        {
            item.Quantity--; // Giảm số lượng

            if (item.Quantity <= 0)
            {
                // Nếu số lượng là 0, xóa luôn sản phẩm khỏi giỏ
                db.CartItems.Remove(item);
                TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            }
            db.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    // --- KẾT THÚC ACTION MỚI ---


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