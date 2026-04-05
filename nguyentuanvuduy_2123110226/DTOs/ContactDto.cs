using System.ComponentModel.DataAnnotations;

namespace nguyentuanvuduy_2123110226.DTOs
{
    public record ContactReadDto(int Id, string Name, string Email, string Message, DateTime SentAt);

    // Khách hàng gửi liên hệ thì không cần truyền Id hay SentAt
    public record ContactCreateDto(
        [Required][StringLength(100)] string Name,
        [Required][EmailAddress][StringLength(100)] string Email,
        [Required][StringLength(1000)] string Message
    );
}