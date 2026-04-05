using System.ComponentModel.DataAnnotations;

namespace nguyentuanvuduy_2123110226.DTOs
{
    public record BlogCategoryReadDto(int Id, string Name);

    public record BlogCategoryCreateDto(
        [Required][StringLength(100)] string Name
    );

    public record BlogCategoryUpdateDto(
        [Required][StringLength(100)] string Name
    );
}