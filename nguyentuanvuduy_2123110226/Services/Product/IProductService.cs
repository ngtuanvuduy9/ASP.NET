using Microsoft.AspNetCore.Http; // Thêm thư viện này để dùng IFormFile
using nguyentuanvuduy_2123110226.DTOs;

namespace nguyentuanvuduy_2123110226.Services
{
    public interface IProductService
    {
        Task<(int Total, IEnumerable<ProductReadDto> Data)> GetAllAsync(int page, int size, int? categoryId, string? keyword = null); Task<ProductReadDto?> GetByIdAsync(int id);
        Task<(bool IsSuccess, int StatusCode, string Message, ProductReadDto? Data)> CreateAsync(ProductCreateDto dto);
        Task<(bool IsSuccess, int StatusCode, string Message, int Added, int Skipped)> BulkCreateAsync(List<ProductCreateDto> dtos);
        Task<(bool IsSuccess, int StatusCode, string Message)> UpdateAsync(int id, ProductUpdateDto dto);
        Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id);

        // ✅ THÊM HÀM UPLOAD ẢNH
        Task<(bool IsSuccess, string Message, string? FileUrl)> UploadImageAsync(IFormFile file);
    }
}