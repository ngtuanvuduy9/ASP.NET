using nguyentuanvuduy_2123110226.DTOs;

namespace nguyentuanvuduy_2123110226.Services
{
    public interface IBlogService
    {
        Task<(int Total, IEnumerable<BlogReadDto> Data)> GetAllAsync(int page, int size, int? blogCategoryId);
        Task<BlogReadDto?> GetByIdAsync(int id);
        Task<(bool IsSuccess, int StatusCode, string Message, BlogReadDto? Data)> CreateAsync(BlogCreateDto dto);
        Task<(bool IsSuccess, int StatusCode, string Message)> UpdateAsync(int id, BlogUpdateDto dto);
        Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id);
    }
}