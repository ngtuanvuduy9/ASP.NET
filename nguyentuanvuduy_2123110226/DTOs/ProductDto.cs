// DTOs/ProductDto.cs
using System.ComponentModel.DataAnnotations;

namespace nguyentuanvuduy_2123110226.DTOs
{
    public record ProductReadDto(
        int Id,
        string ProductCode,
        string Name,
        string? Description,
        decimal Price,
        decimal? OriginalPrice,
        string? Weight,
        string? Size,
        int StockQuantity,
        string? ImageUrl,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        int CategoryId,
        string CategoryName,
        bool IsFavorite// ✅ Join từ Category
    );

    public record ProductCreateDto(
        [Required][StringLength(20)] string ProductCode,
        [Required][StringLength(150)] string Name,
        [StringLength(1000)] string? Description,
        [Required][Range(0, double.MaxValue)] decimal Price,
        [Range(0, double.MaxValue)] decimal? OriginalPrice,
        [StringLength(50)] string? Weight,
        [StringLength(50)] string? Size,
        int StockQuantity,
        [StringLength(500)] string? ImageUrl,
        [Required] int CategoryId
    );

    // ✅ Không có ProductCode — bất biến như CategoryCode
    public record ProductUpdateDto(
        [Required][StringLength(150)] string Name,
        [StringLength(1000)] string? Description,
        [Required][Range(0, double.MaxValue)] decimal Price,
        [Range(0, double.MaxValue)] decimal? OriginalPrice,
        [StringLength(50)] string? Weight,
        [StringLength(50)] string? Size,
        int StockQuantity,
        [StringLength(500)] string? ImageUrl,
        [Required] int CategoryId
    );
}