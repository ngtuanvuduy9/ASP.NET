using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Services;

namespace nguyentuanvuduy_2123110226.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            if (page < 1 || size < 1)
                return BadRequest(new { message = "page và size phải lớn hơn 0" });

            var (total, data) = await _categoryService.GetAllAsync(page, size);
            return Ok(new { total, page, size, data });
        }

        // GET: api/Category/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);

            if (category == null)
                return NotFound(new { message = $"Không tìm thấy category với Id = {id}" });

            return Ok(category);
        }

        // POST: api/Category
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _categoryService.CreateAsync(dto);
            if (!result.IsSuccess)
                return Conflict(new { message = result.Message });

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        // POST: api/Category/bulk
        [Authorize(Roles = "admin")]
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate([FromBody] List<CategoryCreateDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest(new { message = "Danh sách trống" });

            var result = await _categoryService.BulkCreateAsync(dtos);

            if (result.Added == 0)
                return Conflict(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // PUT: api/Category/5
        [Authorize(Roles = "admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _categoryService.UpdateAsync(id, dto);

            if (!result.IsSuccess)
            {
                if (result.StatusCode == 404) return NotFound(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }

            return NoContent();
        }

        // DELETE: api/Category/5
        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);

            if (!result.IsSuccess)
            {
                if (result.StatusCode == 404) return NotFound(new { message = result.Message });
                if (result.StatusCode == 409) return Conflict(new { message = result.Message });
            }

            return NoContent();
        }
    }
}