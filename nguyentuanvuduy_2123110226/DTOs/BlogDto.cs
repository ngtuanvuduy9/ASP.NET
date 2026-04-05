using System.ComponentModel.DataAnnotations;

namespace nguyentuanvuduy_2123110226.DTOs
{
    public record BlogReadDto(
        int Id,
        string Title,
        string Content,
        string? ImageUrl, // ✅ Có dấu ?
        DateTime CreatedAt,
        int BlogCategoryId,
        string BlogCategoryName
    );

    public record BlogCreateDto(
        [Required][StringLength(255)] string Title,
        [Required] string Content,
        [StringLength(500)] string? ImageUrl, // ✅ Thêm dấu ? ở đây
        [Required] int BlogCategoryId
    );

    public record BlogUpdateDto(
        [Required][StringLength(255)] string Title,
        [Required] string Content,
        [StringLength(500)] string? ImageUrl, // ✅ Thêm dấu ? ở đây
        [Required] int BlogCategoryId
    );
}