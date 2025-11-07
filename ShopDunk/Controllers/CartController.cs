using ShopDunk.Models;
using System.Linq;
using System.Web.Mvc;

public class CartController : Controller
{
    private AppDbContext db = new AppDbContext();

    public ActionResult Index()
    {
        int userId = (int)Session["UserID"];
        var cartItems = db.CartItems.Include("Product").Where(c => c.UserID == userId).ToList();
        return View(cartItems);
    }

    public ActionResult Add(int productId)
    {
        int userId = (int)Session["UserID"];
        var item = db.CartItems.FirstOrDefault(c => c.ProductID == productId && c.UserID == userId);
        if (item != null)
            item.Quantity++;
        else
            db.CartItems.Add(new CartItem { ProductID = productId, UserID = userId, Quantity = 1 });

        db.SaveChanges();
        return RedirectToAction("Index");
    }

    public ActionResult Remove(int id)
    {
        var item = db.CartItems.Find(id);
        db.CartItems.Remove(item);
        db.SaveChanges();
        return RedirectToAction("Index");
    }
}