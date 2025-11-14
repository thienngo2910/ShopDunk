using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopDunk.Models
{
    public class ProductReview
    {
        [Key]
        public int ProductReviewID { get; set; }

        [Required]
        public int ProductID { get; set; } // Khóa ngoại tới Product

        [Required]
        public int UserID { get; set; } // Khóa ngoại tới User

        [Required(ErrorMessage = "Vui lòng chọn số sao")]
        [Range(1, 5, ErrorMessage = "Vui lòng chọn từ 1 đến 5 sao")]
        public int Rating { get; set; } // 1-5 sao

        [Display(Name = "Nội dung đánh giá")]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set; }

        public DateTime ReviewDate { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}