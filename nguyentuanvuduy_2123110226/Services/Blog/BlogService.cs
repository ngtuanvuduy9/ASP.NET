using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Models;

namespace nguyentuanvuduy_2123110226.Services
{
    public class BlogService : IBlogService
    {
        private readonly AppDbContext _context;

        public BlogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(int Total, IEnumerable<BlogReadDto> Data)> GetAllAsync(int page, int size, int? blogCategoryId)
        {
            var query = _context.Blogs.AsNoTracking();

            if (blogCategoryId.HasValue)
                query = query.Where(b => b.BlogCategoryId == blogCategoryId.Value);

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(b => new BlogReadDto(
                    b.Id, b.Title, b.Content, b.ImageUrl, b.CreatedAt,
                    b.BlogCategoryId, b.BlogCategory!.Name
                ))
                .ToListAsync();

            return (total, data);
        }

        public async Task<BlogReadDto?> GetByIdAsync(int id)
        {
            return await _context.Blogs
                .AsNoTracking()
                .Where(b => b.Id == id)
                .Select(b => new BlogReadDto(
                    b.Id, b.Title, b.Content, b.ImageUrl, b.CreatedAt,
                    b.BlogCategoryId, b.BlogCategory!.Name
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message, BlogReadDto? Data)> CreateAsync(BlogCreateDto dto)
        {
            bool categoryExists = await _context.BlogCategories.AnyAsync(bc => bc.Id == dto.BlogCategoryId);
            if (!categoryExists)
                return (false, 404, $"Không tìm thấy danh mục blog với Id = {dto.BlogCategoryId}", null);

            var blog = new Blog
            {
                Title = dto.Title.Trim(),
                Content = dto.Content.Trim(),
                ImageUrl = dto.ImageUrl?.Trim() ?? string.Empty,
                BlogCategoryId = dto.BlogCategoryId,
                CreatedAt = DateTime.Now
            };

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            var categoryName = await _context.BlogCategories
                .Where(bc => bc.Id == dto.BlogCategoryId)
                .Select(bc => bc.Name)
                .FirstOrDefaultAsync() ?? "";

            var readDto = new BlogReadDto(
                blog.Id, blog.Title, blog.Content, blog.ImageUrl, blog.CreatedAt,
                blog.BlogCategoryId, categoryName
            );

            return (true, 201, "Tạo bài viết thành công", readDto);
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message)> UpdateAsync(int id, BlogUpdateDto dto)
        {
            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null)
                return (false, 404, $"Không tìm thấy bài viết với Id = {id}");

            bool categoryExists = await _context.BlogCategories.AnyAsync(bc => bc.Id == dto.BlogCategoryId);
            if (!categoryExists)
                return (false, 404, $"Không tìm thấy danh mục blog với Id = {dto.BlogCategoryId}");

            blog.Title = dto.Title.Trim();
            blog.Content = dto.Content.Trim();
            blog.ImageUrl = dto.ImageUrl?.Trim() ?? string.Empty;
            blog.BlogCategoryId = dto.BlogCategoryId;

            await _context.SaveChangesAsync();
            return (true, 204, "Cập nhật thành công");
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id)
        {
            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null)
                return (false, 404, $"Không tìm thấy bài viết với Id = {id}");

            // XÓA CỨNG (Hard Delete)
            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
            return (true, 204, "Xóa thành công");
        }
    }
}