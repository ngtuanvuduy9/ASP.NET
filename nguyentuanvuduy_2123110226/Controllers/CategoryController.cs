using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Models;
using System.Linq;

namespace nguyentuanvuduy_2123110226.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            if (page < 1 || size < 1)
                return BadRequest(new { message = "page và size phải lớn hơn 0" });

            var query = _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive); // ✅ Không dùng Global Filter — tường minh hơn

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(c => new CategoryReadDto(c.Id, c.CategoryCode, c.Name, c.Description, c.CreatedAt))
                .ToListAsync();

            return Ok(new { total, page, size, data });
        }

        // GET: api/Category/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Id == id && c.IsActive)
                .Select(c => new CategoryReadDto(c.Id, c.CategoryCode, c.Name, c.Description, c.CreatedAt))
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound(new { message = $"Không tìm thấy category với Id = {id}" });

            return Ok(category);
        }

        // POST: api/Category
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Categories.AnyAsync(c => c.CategoryCode == dto.CategoryCode))
                return Conflict(new { message = $"CategoryCode '{dto.CategoryCode}' đã tồn tại" });

            var category = new Category
            {
                CategoryCode = dto.CategoryCode.Trim().ToUpper(), // ✅ Chuẩn hóa luôn
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var result = new CategoryReadDto(category.Id, category.CategoryCode, category.Name, category.Description, category.CreatedAt);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, result);
        }

        // POST: api/Category/bulk
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate([FromBody] List<CategoryCreateDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest(new { message = "Danh sách trống" });

            var codes = dtos.Select(d => d.CategoryCode.Trim().ToUpper()).ToList();

            var existingCodes = await _context.Categories
                .Where(c => codes.Contains(c.CategoryCode))
                .Select(c => c.CategoryCode)
                .ToListAsync();

            var toAdd = dtos
                .Where(d => !existingCodes.Contains(d.CategoryCode.Trim().ToUpper()))
                .Select(d => new Category
                {
                    CategoryCode = d.CategoryCode.Trim().ToUpper(),
                    Name = d.Name.Trim(),
                    Description = d.Description?.Trim(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

            if (!toAdd.Any())
                return Conflict(new { message = "Tất cả mã đều đã tồn tại" });

            _context.Categories.AddRange(toAdd);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Đã thêm {toAdd.Count} bản ghi, bỏ qua {dtos.Count - toAdd.Count} bản ghi trùng"
            });
        }

        // PUT: api/Category/5  — CHỈ cho sửa Name & Description, CategoryCode bất biến
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (category == null)
                return NotFound(new { message = $"Không tìm thấy category với Id = {id}" });

            category.Name = dto.Name.Trim();
            category.Description = dto.Description?.Trim();

            await _context.SaveChangesAsync();
            return NoContent(); // 204
        }

        // DELETE: api/Category/5  — Soft delete
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (category == null)
                return NotFound(new { message = $"Không tìm thấy category với Id = {id}" });

            // ✅ Kiểm tra còn Product đang dùng không
            bool hasProducts = await _context.Products
                .AnyAsync(p => p.CategoryId == id);

            if (hasProducts)
                return Conflict(new { message = $"Không thể xóa, vẫn còn sản phẩm thuộc danh mục này" });

            category.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent(); // 204
        }
    }
}