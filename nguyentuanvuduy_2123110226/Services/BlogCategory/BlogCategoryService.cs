using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Models;

namespace nguyentuanvuduy_2123110226.Services
{
    public class BlogCategoryService : IBlogCategoryService
    {
        private readonly AppDbContext _context;

        public BlogCategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BlogCategoryReadDto>> GetAllAsync()
        {
            return await _context.BlogCategories
                .AsNoTracking()
                .Select(bc => new BlogCategoryReadDto(bc.Id, bc.Name))
                .ToListAsync();
        }

        public async Task<BlogCategoryReadDto?> GetByIdAsync(int id)
        {
            return await _context.BlogCategories
                .AsNoTracking()
                .Where(bc => bc.Id == id)
                .Select(bc => new BlogCategoryReadDto(bc.Id, bc.Name))
                .FirstOrDefaultAsync();
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message, BlogCategoryReadDto? Data)> CreateAsync(BlogCategoryCreateDto dto)
        {
            if (await _context.BlogCategories.AnyAsync(bc => bc.Name.ToLower() == dto.Name.ToLower().Trim()))
                return (false, 409, $"Danh mục blog '{dto.Name}' đã tồn tại", null);

            var blogCategory = new BlogCategory
            {
                Name = dto.Name.Trim()
            };

            _context.BlogCategories.Add(blogCategory);
            await _context.SaveChangesAsync();

            var result = new BlogCategoryReadDto(blogCategory.Id, blogCategory.Name);
            return (true, 201, "Tạo thành công", result);
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message)> UpdateAsync(int id, BlogCategoryUpdateDto dto)
        {
            var category = await _context.BlogCategories.FirstOrDefaultAsync(bc => bc.Id == id);
            if (category == null)
                return (false, 404, $"Không tìm thấy danh mục blog với Id = {id}");

            if (await _context.BlogCategories.AnyAsync(bc => bc.Id != id && bc.Name.ToLower() == dto.Name.ToLower().Trim()))
                return (false, 409, $"Danh mục blog '{dto.Name}' đã tồn tại");

            category.Name = dto.Name.Trim();
            await _context.SaveChangesAsync();

            return (true, 204, "Cập nhật thành công");
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id)
        {
            var category = await _context.BlogCategories.FirstOrDefaultAsync(bc => bc.Id == id);
            if (category == null)
                return (false, 404, $"Không tìm thấy danh mục blog với Id = {id}");

            // Kiểm tra xem có bài blog nào đang dùng danh mục này không
            bool hasBlogs = await _context.Blogs.AnyAsync(b => b.BlogCategoryId == id);
            if (hasBlogs)
                return (false, 409, "Không thể xóa vì vẫn còn bài viết thuộc danh mục này");

            // XÓA CỨNG (Hard Delete)
            _context.BlogCategories.Remove(category);
            await _context.SaveChangesAsync();

            return (true, 204, "Xóa thành công");
        }
    }
}