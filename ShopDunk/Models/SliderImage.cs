using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System;
using System.IO;

namespace ShopDunk.Models
{
    public class SliderImage
    {
        [Key]
        public int SliderImageID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Key danh mục")]
        [Display(Name = "Key Danh mục (VD: Home, iPhone, iPad)")]
        public string CategoryKey { get; set; }

        // --- THAY ĐỔI: Dùng ImageData ---
        public byte[] ImageData { get; set; }

        [Display(Name = "Tiêu đề (Alt Text)")]
        [DataType(DataType.MultilineText)]
        public string Title { get; set; }

        [Display(Name = "Đang hoạt động?")]
        public bool IsActive { get; set; } = true;

        [NotMapped]
        [Display(Name = "Tải ảnh mới (Có thể chọn nhiều ảnh)")]
        public IEnumerable<HttpPostedFileBase> ImageFiles { get; set; }

        // --- THÊM MỚI: Thuộc tính hiển thị ảnh ---
        [NotMapped]
        public string ImageBase64
        {
            get
            {
                if (ImageData != null && ImageData.Length > 0)
                {
                    return "data:image/jpeg;base64," + Convert.ToBase64String(ImageData);
                }
                return "https://placehold.co/1200x400/1C1C1E/3a3a3c?text=No+Image";
            }
        }
    }
}