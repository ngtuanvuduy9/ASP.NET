using nguyentuanvuduy_2123110226.DTOs;

namespace nguyentuanvuduy_2123110226.Services
{
    public interface ICategoryService
    {
        Task<(int Total, IEnumerable<CategoryReadDto> Data)> GetAllAsync(int page, int size);
        Task<CategoryReadDto?> GetByIdAsync(int id);
        Task<(bool IsSuccess, string Message, CategoryReadDto? Data)> CreateAsync(CategoryCreateDto dto);
        Task<(int Added, int Skipped, string Message)> BulkCreateAsync(List<CategoryCreateDto> dtos);
        Task<(bool IsSuccess, int StatusCode, string Message)> UpdateAsync(int id, CategoryUpdateDto dto);
        Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id);
    }
}