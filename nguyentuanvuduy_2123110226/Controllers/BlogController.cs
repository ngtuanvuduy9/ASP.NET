using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; // Bổ sung
using Microsoft.AspNetCore.Mvc;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Services;

namespace nguyentuanvuduy_2123110226.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] int? blogCategoryId = null)
        {
            if (page < 1 || size < 1) return BadRequest(new { message = "page và size phải lớn hơn 0" });

            var (total, data) = await _blogService.GetAllAsync(page, size, blogCategoryId);
            return Ok(new { total, page, size, data });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var blog = await _blogService.GetByIdAsync(id);
            if (blog == null)
                return NotFound(new { message = $"Không tìm thấy bài viết với Id = {id}" });

            return Ok(blog);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BlogCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _blogService.CreateAsync(dto);
            if (!result.IsSuccess) return NotFound(new { message = result.Message });

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] BlogUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _blogService.UpdateAsync(id, dto);
            if (!result.IsSuccess) return NotFound(new { message = result.Message });

            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _blogService.DeleteAsync(id);
            if (!result.IsSuccess) return NotFound(new { message = result.Message });

            return NoContent();
        }

        // ✅ API UPLOAD ẢNH CHO BLOG
        [Authorize(Roles = "admin")]
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var result = await _blogService.UploadImageAsync(file);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { url = result.FileUrl });
        }
    }
}