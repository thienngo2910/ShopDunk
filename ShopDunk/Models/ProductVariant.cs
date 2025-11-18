using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopDunk.Models
{
    public class ProductVariant
    {
        [Key]
        public int VariantID { get; set; }

        public int ProductID { get; set; }

        [Required]
        [Display(Name = "Màu sắc")]
        public string Color { get; set; } // Ví dụ: Titan Tự nhiên, Đen...

        [Required]
        [Display(Name = "Dung lượng")]
        public string Storage { get; set; } // Ví dụ: 128GB, 256GB...

        [Required]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; } // Giá riêng cho phiên bản này

        [Required]
        [Display(Name = "Tồn kho")]
        public int Stock { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }
    }
}