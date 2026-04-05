using Microsoft.AspNetCore.Mvc;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Services;

namespace nguyentuanvuduy_2123110226.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/User
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        // POST: api/User (Chỉ Admin mới được dùng API này — tính sau)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userService.CreateAsync(dto);

            if (!result.IsSuccess)
            {
                if (result.StatusCode == 400) return BadRequest(new { message = result.Message });
                if (result.StatusCode == 409) return Conflict(new { message = result.Message });
            }

            return StatusCode(201, result.Data); // 201 Created
        }

        // POST: api/User/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userService.LoginAsync(dto);

            if (!result.IsSuccess)
                return Unauthorized(new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
                token = result.Token,
                role = result.Role
            });
        }
    }
}