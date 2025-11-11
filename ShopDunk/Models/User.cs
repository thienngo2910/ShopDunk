using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // Không lưu vào DB
        [NotMapped]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; }

        [NotMapped]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }
    }
}