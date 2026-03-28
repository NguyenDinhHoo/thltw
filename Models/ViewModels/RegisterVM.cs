using System.ComponentModel.DataAnnotations;

namespace AspNetMvcProject.Models.ViewModels;

public class RegisterVM
{
    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ tên không được để trống")]
    public string FullName { get; set; } = string.Empty;

    public string? Address { get; set; }

    [Range(1, 120, ErrorMessage = "Tuổi phải từ 1 đến 120")]
    public int? Age { get; set; }
}
