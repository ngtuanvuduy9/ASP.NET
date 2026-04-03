using System.ComponentModel.DataAnnotations;

namespace nguyentuanvuduy_2123110226.DTOs
{
    // Dùng để tạo User mới
    public record UserCreateDto(
        [Required][StringLength(50)] string Username,
        [Required][StringLength(50)] string Password,
        [Required][StringLength(100)] string FullName,
        [Required] string Role // "admin" hoặc "staff"
    );

    // Dùng để trả về thông tin (không có password)
    public record UserReadDto(
        int Id,
        string Username,
        string FullName,
        string Role,
        DateTime CreatedAt
    );
    // Dùng để đăng nhập
    public record UserLoginDto(
        [Required] string Username,
        [Required] string Password
    );
}