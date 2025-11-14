using System.Collections.Generic; // <-- THÊM DÒNG NÀY
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace ShopDunk.Models
{
    public class SliderImage
    {
        [Key]
        public int SliderImageID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Key danh mục")]
        [Display(Name = "Key Danh mục (VD: Home, iPhone, iPad)")]
        public string CategoryKey { get; set; }

        [Display(Name = "Đường dẫn ảnh")]
        public string ImageUrl { get; set; }

        [Display(Name = "Tiêu đề (Alt Text)")]
        public string Title { get; set; }

        [Display(Name = "Đang hoạt động?")]
        public bool IsActive { get; set; } = true;

        [NotMapped]
        [Display(Name = "Tải ảnh mới (Có thể chọn nhiều ảnh)")]
        // --- SỬA LỖI: Đổi từ 1 file sang danh sách file ---
        public IEnumerable<HttpPostedFileBase> ImageFiles { get; set; }
    }
}