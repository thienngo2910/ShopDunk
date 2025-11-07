using ShopDunk.Models;
using System.Collections.Generic;
using System.Data.Entity;

namespace ShopDunk.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        public AppDbContext() : base("DefaultConnection") { }
    }
}