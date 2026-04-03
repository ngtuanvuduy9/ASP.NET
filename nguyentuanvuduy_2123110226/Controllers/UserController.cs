using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Models;

namespace nguyentuanvuduy_2123110226.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config; // Bổ sung dòng này

        // Bổ sung IConfiguration vào Constructor
        public UserController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: api/User
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Select(u => new UserReadDto(u.Id, u.Username, u.FullName, u.Role, u.CreatedAt))
                .ToListAsync();

            return Ok(users);
        }

        // POST: api/User (Chỉ Admin mới được dùng API này — tính sau)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Kiểm tra Role hợp lệ
            var validRoles = new[] { "admin", "staff" };
            if (!validRoles.Contains(dto.Role.ToLower()))
                return BadRequest(new { message = "Role không hợp lệ. Chỉ chấp nhận: admin, staff" });

            // Kiểm tra trùng Username
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return Conflict(new { message = $"Username '{dto.Username}' đã tồn tại!" });

            var user = new User
            {
                Username = dto.Username.ToLower().Trim(),
                PasswordHash = dto.Password, // CHÚ Ý: Cần mã hóa chỗ này ở bước sau!
                FullName = dto.FullName.Trim(),
                Role = dto.Role.ToLower().Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = new UserReadDto(user.Id, user.Username, user.FullName, user.Role, user.CreatedAt);
            return StatusCode(201, result); // 201 Created
        }
        // THÊM HÀM NÀY VÀO: POST: api/User/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            // 1. Kiểm tra user trong DB
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username && u.PasswordHash == dto.Password && u.IsActive);

            if (user == null)
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu!" });

            // 2. Tạo Claims (Gắn thông tin Role vào Token)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role) // Quan trọng: Phải có Role thì [Authorize(Roles="admin")] mới hiểu
            };

            // 3. Lấy Secret Key từ appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 4. Sinh Token
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            // 5. Trả về Token cho Client
            return Ok(new
            {
                message = "Đăng nhập thành công!",
                token = new JwtSecurityTokenHandler().WriteToken(token),
                role = user.Role
            });
        }
    }
}