using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public string Role { get; set; } = "User";

        public virtual ICollection<Order> Orders { get; set; }

        // --- THÊM DÒNG NÀY ---
        public virtual ICollection<ProductReview> Reviews { get; set; }
    }
}