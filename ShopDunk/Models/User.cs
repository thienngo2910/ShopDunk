using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// Thêm 2 using này nếu chưa có
using System.Collections.Generic;
using ShopDunk.Models;

namespace ShopDunk.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } = "User"; // Mặc định là User

        // --- BẮT ĐẦU SỬA LỖI ---
        // Xóa bỏ các thuộc tính [NotMapped] gây ra lỗi Validation

        // [NotMapped]
        // [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        // public string Password { get; set; }

        // [NotMapped]
        // [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        // public string ConfirmPassword { get; set; }

        // Thêm thuộc tính điều hướng cho Order (để khớp với Order.cs)
        public virtual ICollection<Order> Orders { get; set; }
        // --- KẾT THÚC SỬA LỖI ---
    }
}