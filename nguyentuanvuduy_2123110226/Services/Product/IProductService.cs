using Microsoft.AspNetCore.Http;
using nguyentuanvuduy_2123110226.DTOs;

namespace nguyentuanvuduy_2123110226.Services
{
    public interface IProductService
    {
        Task<(int Total, IEnumerable<ProductReadDto> Data)> GetAllAsync(int page, int size, int? categoryId, string? keyword = null, int? customerId = null);

        // ✅ ĐÃ SỬA: Thêm customerId vào đây để trang chi tiết cũng biết ai đang xem
        Task<ProductReadDto?> GetByIdAsync(int id, int? customerId = null);

        Task<(bool IsSuccess, int StatusCode, string Message, ProductReadDto? Data)> CreateAsync(ProductCreateDto dto);
        Task<(bool IsSuccess, int StatusCode, string Message, int Added, int Skipped)> BulkCreateAsync(List<ProductCreateDto> dtos);
        Task<(bool IsSuccess, int StatusCode, string Message)> UpdateAsync(int id, ProductUpdateDto dto);
        Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id);
        Task<(bool IsSuccess, string Message, string? FileUrl)> UploadImageAsync(IFormFile file);
    }
}