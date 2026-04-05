using nguyentuanvuduy_2123110226.DTOs;

namespace nguyentuanvuduy_2123110226.Services
{
    public interface IBlogCategoryService
    {
        Task<IEnumerable<BlogCategoryReadDto>> GetAllAsync();
        Task<BlogCategoryReadDto?> GetByIdAsync(int id);
        Task<(bool IsSuccess, int StatusCode, string Message, BlogCategoryReadDto? Data)> CreateAsync(BlogCategoryCreateDto dto);
        Task<(bool IsSuccess, int StatusCode, string Message)> UpdateAsync(int id, BlogCategoryUpdateDto dto);
        Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id);
    }
}