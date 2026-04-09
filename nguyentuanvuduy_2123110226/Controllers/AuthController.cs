using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace nguyentuanvuduy_2123110226.Controllers;

[Route("api/[controller]")]
[ApiController]
// Dùng IConfiguration để đọc các thông số Jwt:Key từ file appsettings.json
public class AuthController(AppDbContext context, IConfiguration config) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // 1. Tìm khách hàng
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Email == dto.Email && c.PasswordHash == dto.Password);

        // 2. Nếu không tìm thấy
        if (customer == null)
        {
            return BadRequest(new { message = "Sai email hoặc mật khẩu, vui lòng thử lại!" });
        }

        // 3. TẠO TOKEN JWT THẬT SỰ
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Khai báo các thông tin cất giấu bên trong Token
        var claims = new[]
        {
            // CỰC KỲ QUAN TRỌNG: Đây là chỗ lưu ID để FavoriteController lấy ra xài
            new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new Claim(ClaimTypes.Email, customer.Email),
            new Claim(ClaimTypes.Role, "customer")
        };

        // Đóng gói Token
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7), // Cho phép người dùng đăng nhập trong 7 ngày
            signingCredentials: credentials);

        var realToken = new JwtSecurityTokenHandler().WriteToken(token);

        // 4. Trả kết quả về cho React
        return Ok(new
        {
            message = "Đăng nhập thành công!",
            token = realToken, // Gửi JWT chuẩn về cho React lưu vào localStorage
            user = new { customer.Id, customer.FullName, customer.Email, customer.Points }
        });
    }
}