// DTOs/CategoryDto.cs
using System.ComponentModel.DataAnnotations;

namespace nguyentuanvuduy_2123110226.DTOs
{
    public record CategoryReadDto(
        int Id,
        string CategoryCode,
        string Name,
        string? Description,
        DateTime CreatedAt
    );

    public record CategoryCreateDto(
        [Required][StringLength(50)] string CategoryCode,
        [Required][StringLength(100)] string Name,
        [StringLength(255)] string? Description
    );

    public record CategoryUpdateDto(
        [Required][StringLength(100)] string Name,
        [StringLength(255)] string? Description
    );
}