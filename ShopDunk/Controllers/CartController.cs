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
            // --- CẬP NHẬT: Gửi kèm returnUrl ---
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Cart") });
        }

        int userId = (int)Session["UserID"];
        var cartItems = db.CartItems.Include(c => c.Product).Where(c => c.UserID == userId).ToList();
        return View(cartItems);
    }

    // GET: /Cart/Add
    public ActionResult Add(int id, int? variantId) // Thêm tham số variantId
    {
        if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

        int userId = (int)Session["UserID"];
        var product = db.Products.Find(id);

        string color = "Tiêu chuẩn";
        string storage = "Tiêu chuẩn";
        decimal price = product.Price;

        // Nếu có chọn biến thể, lấy thông tin chi tiết
        if (variantId.HasValue)
        {
            var variant = db.ProductVariants.Find(variantId);
            if (variant != null)
            {
                color = variant.Color;
                storage = variant.Storage;
                price = variant.Price; // Lưu ý: CartItem chưa lưu giá riêng, ta tạm lưu thông tin text
            }
        }

        // Logic tìm sản phẩm trong giỏ (Cần so sánh cả Color và Storage)
        var item = db.CartItems.FirstOrDefault(c => c.ProductID == id && c.UserID == userId && c.Color == color && c.Storage == storage);

        if (item != null)
        {
            item.Quantity++;
        }
        else
        {
            db.CartItems.Add(new CartItem
            {
                ProductID = id,
                UserID = userId,
                Quantity = 1,
                Color = color,      // Lưu màu
                Storage = storage   // Lưu dung lượng
            });
        }

        db.SaveChanges();
        TempData["Success"] = "Đã thêm vào giỏ hàng.";
        return RedirectToAction("Index", "Cart");
    }

    // (Giữ nguyên các Action: IncreaseQuantity, DecreaseQuantity, Remove, Dispose)
    // ...
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
            item.Quantity--;

            if (item.Quantity <= 0)
            {
                db.CartItems.Remove(item);
                TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            }
            db.SaveChanges();
        }
        return RedirectToAction("Index");
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