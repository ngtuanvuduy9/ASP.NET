using nguyentuanvuduy_2123110226.DTOs;

namespace nguyentuanvuduy_2123110226.Services
{
    public interface IContactService
    {
        Task<IEnumerable<ContactReadDto>> GetAllAsync();
        Task<ContactReadDto?> GetByIdAsync(int id);
        Task<(bool IsSuccess, int StatusCode, string Message, ContactReadDto? Data)> CreateAsync(ContactCreateDto dto);
        Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id);
    }
}