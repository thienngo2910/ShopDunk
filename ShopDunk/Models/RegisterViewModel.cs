using System.ComponentModel.DataAnnotations;

namespace ShopDunk.Models
{
 public class RegisterViewModel
 {
 [Required]
 [StringLength(50)]
 public string Username { get; set; }

 [Required]
 [EmailAddress]
 public string Email { get; set; }

 [Required(ErrorMessage = "Vui lòng nh?p m?t kh?u")]
 [DataType(DataType.Password)]
 public string Password { get; set; }

 [DataType(DataType.Password)]
 [Compare("Password", ErrorMessage = "M?t kh?u xác nh?n không kh?p")]
 public string ConfirmPassword { get; set; }
 }
}