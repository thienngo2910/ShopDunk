using System;
using System.Collections.Generic; // THÊM MỚI
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // THÊM MỚI

namespace ShopDunk.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // Ví dụ: "Chờ xử lý", "Đã giao", "Đã hủy"

        // --- BẮT ĐẦU SỬA LỖI ---
        // Thêm các thuộc tính điều hướng (Navigation Properties)
        // mà Entity Framework cần để liên kết các bảng.

        // Lỗi CS1061 (item.User) xảy ra vì thiếu thuộc tính này:
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        // Lỗi CS1061 (Model.OrderDetails) xảy ra vì thiếu thuộc tính này:
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        // --- KẾT THÚC SỬA LỖI ---
    }
}