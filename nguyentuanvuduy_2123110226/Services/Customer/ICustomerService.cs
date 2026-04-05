using nguyentuanvuduy_2123110226.DTOs;

namespace nguyentuanvuduy_2123110226.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerReadDto>> GetAllAsync();
        Task<CustomerReadDto?> GetByIdAsync(int id);
        Task<(bool IsSuccess, int StatusCode, string Message, CustomerReadDto? Data)> CreateAsync(CustomerCreateDto dto);
        Task<(bool IsSuccess, int StatusCode, string Message)> UpdateAsync(int id, CustomerUpdateDto dto);
        Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id);
    }
}