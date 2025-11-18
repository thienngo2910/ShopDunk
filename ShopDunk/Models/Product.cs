using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System.IO;

namespace ShopDunk.Models
{
    public class Product
    {
        // ... (Giữ nguyên các thuộc tính cũ: ProductID, Name, Price, ImageData...)
        [Key]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; } // Giá mặc định (hiển thị khi chưa chọn)

        [Display(Name = "Dữ liệu ảnh")]
        public byte[] ImageData { get; set; }

        [Required]
        public int Stock { get; set; } // Tồn kho tổng (tùy chọn)

        public string Category { get; set; }

        [NotMapped]
        public HttpPostedFileBase ImageFile { get; set; }

        [NotMapped]
        public string ImageBase64
        {
            get
            {
                if (ImageData != null && ImageData.Length > 0)
                    return "data:image/jpeg;base64," + Convert.ToBase64String(ImageData);
                return "https://placehold.co/300x300/1C1C1E/3a3a3c?text=N/A";
            }
        }

        public virtual ICollection<ProductReview> Reviews { get; set; }

        // --- THÊM DÒNG NÀY ---
        public virtual ICollection<ProductVariant> Variants { get; set; }
    }
}