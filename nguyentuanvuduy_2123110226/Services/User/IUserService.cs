using nguyentuanvuduy_2123110226.DTOs;

namespace nguyentuanvuduy_2123110226.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserReadDto>> GetAllAsync();
        Task<(bool IsSuccess, int StatusCode, string Message, UserReadDto? Data)> CreateAsync(UserCreateDto dto);
        Task<(bool IsSuccess, string Message, string? Token, string? Role)> LoginAsync(UserLoginDto dto);
    }
}