using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace nguyentuanvuduy_2123110226.Services
{
    public class BlogService : IBlogService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public BlogService(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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

            // ✅ LOGIC XÓA ẢNH CŨ KHI CẬP NHẬT ẢNH MỚI
            if (!string.IsNullOrEmpty(dto.ImageUrl) && dto.ImageUrl != blog.ImageUrl)
            {
                // Chỉ xóa nếu ảnh cũ là file lưu trên ổ cứng (không bắt đầu bằng http)
                if (!string.IsNullOrEmpty(blog.ImageUrl) && !blog.ImageUrl.StartsWith("http"))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, blog.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
            }

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

            // ✅ LOGIC XÓA ẢNH KHI XÓA BÀI VIẾT
            if (!string.IsNullOrEmpty(blog.ImageUrl) && !blog.ImageUrl.StartsWith("http"))
            {
                var oldFilePath = Path.Combine(_environment.WebRootPath, blog.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
            return (true, 204, "Xóa thành công");
        }

        // XỬ LÝ LƯU ẢNH BLOG XUỐNG Ổ CỨNG
        public async Task<(bool IsSuccess, string Message, string? FileUrl)> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "Không có file nào được chọn.", null);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return (false, "Chỉ cho phép tải lên file ảnh (.jpg, .jpeg, .png, .gif, .webp)", null);

            if (file.Length > 5 * 1024 * 1024)
                return (false, "Kích thước ảnh không được vượt quá 5MB", null);

            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "blogs");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = $"blog-{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/blogs/{fileName}";
            return (true, "Tải ảnh thành công", fileUrl);
        }
    }
}