using System.ComponentModel.DataAnnotations;

namespace ShopDunk.Models
{
 public class ChangePasswordViewModel
 {
 public int UserID { get; set; }

 [Required]
 [DataType(DataType.Password)]
 [MinLength(6, ErrorMessage = "M?t kh?u ph?i ít nh?t6 ký t?")]
 public string NewPassword { get; set; }

 [DataType(DataType.Password)]
 [Compare("NewPassword", ErrorMessage = "M?t kh?u xác nh?n không kh?p")]
 public string ConfirmPassword { get; set; }
 }
}