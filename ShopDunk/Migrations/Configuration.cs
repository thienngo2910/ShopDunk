namespace ShopDunk.Migrations
{
    using ShopDunk.Models; // Thêm
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq; // Thêm
    using System.Security.Cryptography; // Thêm
    using System.Text; // Thêm

    internal sealed class Configuration : DbMigrationsConfiguration<ShopDunk.Models.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false; // (Hoặc true, tùy dự án của bạn)
        }

        protected override void Seed(ShopDunk.Models.AppDbContext context)
        {
            // Chỉ tạo admin NẾU CHƯA TỒN TẠI
            if (!context.Users.Any(u => u.Username == "admin"))
            {
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@admin.com",
                    PasswordHash = HashPassword("admin123"), // Mật khẩu mặc định
                    Role = "Admin"
                };
                context.Users.Add(adminUser);
            }
        }

        // --- THÊM HÀM NÀY (COPY TỪ ACCOUNTCONTROLLER) ---
        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}