using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Models;

namespace nguyentuanvuduy_2123110226.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(int Total, IEnumerable<CategoryReadDto> Data)> GetAllAsync(int page, int size)
        {
            var query = _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive);

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(c => new CategoryReadDto(c.Id, c.CategoryCode, c.Name, c.Description, c.CreatedAt))
                .ToListAsync();

            return (total, data);
        }

        public async Task<CategoryReadDto?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.Id == id && c.IsActive)
                .Select(c => new CategoryReadDto(c.Id, c.CategoryCode, c.Name, c.Description, c.CreatedAt))
                .FirstOrDefaultAsync();
        }

        public async Task<(bool IsSuccess, string Message, CategoryReadDto? Data)> CreateAsync(CategoryCreateDto dto)
        {
            if (await _context.Categories.AnyAsync(c => c.CategoryCode == dto.CategoryCode))
                return (false, $"CategoryCode '{dto.CategoryCode}' đã tồn tại", null);

            var category = new Category
            {
                CategoryCode = dto.CategoryCode.Trim().ToUpper(),
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var result = new CategoryReadDto(category.Id, category.CategoryCode, category.Name, category.Description, category.CreatedAt);
            return (true, "Thành công", result);
        }

        public async Task<(int Added, int Skipped, string Message)> BulkCreateAsync(List<CategoryCreateDto> dtos)
        {
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
                return (0, dtos.Count, "Tất cả mã đều đã tồn tại");

            _context.Categories.AddRange(toAdd);
            await _context.SaveChangesAsync();

            return (toAdd.Count, dtos.Count - toAdd.Count, $"Đã thêm {toAdd.Count} bản ghi, bỏ qua {dtos.Count - toAdd.Count} bản ghi trùng");
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message)> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (category == null)
                return (false, 404, $"Không tìm thấy category với Id = {id}");

            category.Name = dto.Name.Trim();
            category.Description = dto.Description?.Trim();

            await _context.SaveChangesAsync();
            return (true, 204, "Cập nhật thành công");
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (category == null)
                return (false, 404, $"Không tìm thấy category với Id = {id}");

            bool hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);

            if (hasProducts)
                return (false, 409, "Không thể xóa, vẫn còn sản phẩm thuộc danh mục này");

            category.IsActive = false;
            await _context.SaveChangesAsync();

            return (true, 204, "Xóa thành công");
        }
    }
}