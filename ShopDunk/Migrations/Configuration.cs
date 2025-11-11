namespace ShopDunk.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using ShopDunk.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<ShopDunk.Models.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        // Hàm băm mật khẩu
        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        protected override void Seed(ShopDunk.Models.AppDbContext context)
        {
            // Thêm tài khoản Admin mặc định
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@gmail.com",
                PasswordHash = HashPassword("admin"), // Mật khẩu là admin
                Role = "Admin"
            };

            context.Users.AddOrUpdate(
                u => u.Username, // Key để kiểm tra trùng lặp
                adminUser
            );

            // Thêm một số sản phẩm mặc định
            context.Products.AddOrUpdate(
                p => p.Name,
                new Product { Name = "iPhone 15 Pro Max", Description = "Màu Titan Tự Nhiên", Price = 34000000m, Stock = 100, ImageUrl = "/Images/Products/iphone15promax.png", Category = "iPhone" },
                new Product { Name = "iPhone 15", Description = "Bản 128GB", Price = 22000000m, Stock = 150, ImageUrl = "/Images/Products/iphone15.png", Category = "iPhone" },
                new Product { Name = "iPad Pro M4", Description = "13-inch, M4, 256GB", Price = 30000000m, Stock = 50, ImageUrl = "/Images/Products/ipadprom4.png", Category = "iPad" },
                new Product { Name = "MacBook Air M3", Description = "13-inch, M3, 8GB/256GB", Price = 27000000m, Stock = 70, ImageUrl = "/Images/Products/macbookairm3.png", Category = "Mac" }
            );
        }
    }
}