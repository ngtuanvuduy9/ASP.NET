using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Services;

namespace nguyentuanvuduy_2123110226.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogCategoryController : ControllerBase
    {
        private readonly IBlogCategoryService _blogCategoryService;

        public BlogCategoryController(IBlogCategoryService blogCategoryService)
        {
            _blogCategoryService = blogCategoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _blogCategoryService.GetAllAsync();
            return Ok(data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _blogCategoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound(new { message = $"Không tìm thấy danh mục blog với Id = {id}" });

            return Ok(category);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BlogCategoryCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _blogCategoryService.CreateAsync(dto);
            if (!result.IsSuccess)
                return Conflict(new { message = result.Message });

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] BlogCategoryUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _blogCategoryService.UpdateAsync(id, dto);
            if (!result.IsSuccess)
            {
                if (result.StatusCode == 404) return NotFound(new { message = result.Message });
                return Conflict(new { message = result.Message });
            }

            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _blogCategoryService.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                if (result.StatusCode == 404) return NotFound(new { message = result.Message });
                if (result.StatusCode == 409) return Conflict(new { message = result.Message });
            }

            return NoContent();
        }
    }
}