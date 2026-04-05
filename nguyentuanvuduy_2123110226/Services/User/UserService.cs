using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Models;

namespace nguyentuanvuduy_2123110226.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UserService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<IEnumerable<UserReadDto>> GetAllAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Select(u => new UserReadDto(u.Id, u.Username, u.FullName, u.Role, u.CreatedAt))
                .ToListAsync();
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message, UserReadDto? Data)> CreateAsync(UserCreateDto dto)
        {
            var validRoles = new[] { "admin", "staff" };
            if (!validRoles.Contains(dto.Role.ToLower()))
                return (false, 400, "Role không hợp lệ. Chỉ chấp nhận: admin, staff", null);

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return (false, 409, $"Username '{dto.Username}' đã tồn tại!", null);

            var user = new User
            {
                Username = dto.Username.ToLower().Trim(),
                PasswordHash = dto.Password, // ⚠️ CHÚ Ý: Đồ án thực tế/Đi làm cần dùng BCrypt mã hóa chỗ này!
                FullName = dto.FullName.Trim(),
                Role = dto.Role.ToLower().Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = new UserReadDto(user.Id, user.Username, user.FullName, user.Role, user.CreatedAt);
            return (true, 201, "Tạo tài khoản thành công", result);
        }

        public async Task<(bool IsSuccess, string Message, string? Token, string? Role)> LoginAsync(UserLoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username && u.PasswordHash == dto.Password && u.IsActive);

            if (user == null)
                return (false, "Sai tài khoản hoặc mật khẩu!", null, null);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return (true, "Đăng nhập thành công!", tokenString, user.Role);
        }
    }
}