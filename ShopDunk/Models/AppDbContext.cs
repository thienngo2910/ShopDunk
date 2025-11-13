using System.Data.Entity;
using ShopDunk.Models;

namespace ShopDunk.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        // --- THÊM DÒNG NÀY ---
        public DbSet<SliderImage> SliderImages { get; set; }

        public AppDbContext() : base("DefaultConnection") { }
    }
}