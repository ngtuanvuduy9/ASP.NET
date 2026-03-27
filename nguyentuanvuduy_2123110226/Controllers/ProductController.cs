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
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Product?page=1&size=10&categoryId=1
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] int? categoryId = null)
        {
            if (page < 1 || size < 1)
                return BadRequest(new { message = "page và size phải lớn hơn 0" });

            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(p => new ProductReadDto(
                    p.Id,
                    p.ProductCode,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.OriginalPrice,
                    p.Weight,
                    p.Size,
                    p.StockQuantity,
                    p.ImageUrl,
                    p.Status,
                    p.CreatedAt,
                    p.UpdatedAt,
                    p.CategoryId,
                    p.Category.Name  // ✅ CategoryName
                ))
                .ToListAsync();

            return Ok(new { total, page, size, data });
        }

        // GET: api/Product/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == id && p.IsActive)
                .Select(p => new ProductReadDto(
                    p.Id,
                    p.ProductCode,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.OriginalPrice,
                    p.Weight,
                    p.Size,
                    p.StockQuantity,
                    p.ImageUrl,
                    p.Status,
                    p.CreatedAt,
                    p.UpdatedAt,
                    p.CategoryId,
                    p.Category.Name  // ✅ CategoryName
                ))
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound(new { message = $"Không tìm thấy sản phẩm với Id = {id}" });

            return Ok(product);
        }

        // POST: api/Product
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra Category tồn tại và đang active
            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);

            if (!categoryExists)
                return NotFound(new { message = $"Không tìm thấy category với Id = {dto.CategoryId}" });

            // Kiểm tra ProductCode trùng
            if (await _context.Products.AnyAsync(p => p.ProductCode == dto.ProductCode))
                return Conflict(new { message = $"ProductCode '{dto.ProductCode}' đã tồn tại" });

            var product = new Product
            {
                ProductCode = dto.ProductCode.Trim().ToUpper(),
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                Price = dto.Price,
                OriginalPrice = dto.OriginalPrice,
                Weight = dto.Weight?.Trim(),
                Size = dto.Size?.Trim(),
                StockQuantity = dto.StockQuantity,
                ImageUrl = dto.ImageUrl?.Trim(),
                Status = dto.StockQuantity > 0 ? "in_stock" : "out_of_stock", // ✅ Tự động set status
                IsActive = true,
                CategoryId = dto.CategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, new { product.Id, product.ProductCode });
        }

        // POST: api/Product/bulk
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate([FromBody] List<ProductCreateDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest(new { message = "Danh sách trống" });

            // Kiểm tra tất cả CategoryId hợp lệ
            var categoryIds = dtos.Select(d => d.CategoryId).Distinct().ToList();
            var validCategoryIds = await _context.Categories
                .Where(c => categoryIds.Contains(c.Id) && c.IsActive)
                .Select(c => c.Id)
                .ToListAsync();

            var invalidCategoryIds = categoryIds.Except(validCategoryIds).ToList();
            if (invalidCategoryIds.Any())
                return NotFound(new { message = $"CategoryId không hợp lệ: {string.Join(", ", invalidCategoryIds)}" });

            // Lọc ProductCode trùng
            var codes = dtos.Select(d => d.ProductCode.Trim().ToUpper()).ToList();
            var existingCodes = await _context.Products
                .Where(p => codes.Contains(p.ProductCode))
                .Select(p => p.ProductCode)
                .ToListAsync();

            var toAdd = dtos
                .Where(d => !existingCodes.Contains(d.ProductCode.Trim().ToUpper()))
                .Select(d => new Product
                {
                    ProductCode = d.ProductCode.Trim().ToUpper(),
                    Name = d.Name.Trim(),
                    Description = d.Description?.Trim(),
                    Price = d.Price,
                    OriginalPrice = d.OriginalPrice,
                    Weight = d.Weight?.Trim(),
                    Size = d.Size?.Trim(),
                    StockQuantity = d.StockQuantity,
                    ImageUrl = d.ImageUrl?.Trim(),
                    Status = d.StockQuantity > 0 ? "in_stock" : "out_of_stock",
                    IsActive = true,
                    CategoryId = d.CategoryId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

            if (!toAdd.Any())
                return Conflict(new { message = "Tất cả mã đều đã tồn tại" });

            _context.Products.AddRange(toAdd);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Đã thêm {toAdd.Count} sản phẩm, bỏ qua {dtos.Count - toAdd.Count} sản phẩm trùng"
            });
        }

        // PUT: api/Product/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
                return NotFound(new { message = $"Không tìm thấy sản phẩm với Id = {id}" });

            // Kiểm tra Category mới hợp lệ
            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);

            if (!categoryExists)
                return NotFound(new { message = $"Không tìm thấy category với Id = {dto.CategoryId}" });

            product.Name = dto.Name.Trim();
            product.Description = dto.Description?.Trim();
            product.Price = dto.Price;
            product.OriginalPrice = dto.OriginalPrice;
            product.Weight = dto.Weight?.Trim();
            product.Size = dto.Size?.Trim();
            product.StockQuantity = dto.StockQuantity;
            product.ImageUrl = dto.ImageUrl?.Trim();
            product.Status = dto.StockQuantity > 0 ? "in_stock" : "out_of_stock"; // ✅ Tự động cập nhật
            product.CategoryId = dto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow; // ✅ Cập nhật UpdatedAt

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Product/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
                return NotFound(new { message = $"Không tìm thấy sản phẩm với Id = {id}" });

            product.IsActive = false;
            product.DeletedAt = DateTime.UtcNow; // ✅ Ghi thời điểm xóa
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}