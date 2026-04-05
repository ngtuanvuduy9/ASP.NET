using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace nguyentuanvuduy_2123110226.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        // ✅ Đã thêm khai báo biến _environment ở đây
        private readonly IWebHostEnvironment _environment;

        // ✅ Đã tiêm IWebHostEnvironment vào Constructor
        public ProductService(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<(int Total, IEnumerable<ProductReadDto> Data)> GetAllAsync(int page, int size, int? categoryId)
        {
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
                    p.Id, p.ProductCode, p.Name, p.Description, p.Price, p.OriginalPrice,
                    p.Weight, p.Size, p.StockQuantity, p.ImageUrl, p.Status,
                    p.CreatedAt, p.UpdatedAt, p.CategoryId, p.Category.Name
                ))
                .ToListAsync();

            return (total, data);
        }

        public async Task<ProductReadDto?> GetByIdAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == id && p.IsActive)
                .Select(p => new ProductReadDto(
                    p.Id, p.ProductCode, p.Name, p.Description, p.Price, p.OriginalPrice,
                    p.Weight, p.Size, p.StockQuantity, p.ImageUrl, p.Status,
                    p.CreatedAt, p.UpdatedAt, p.CategoryId, p.Category.Name
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message, ProductReadDto? Data)> CreateAsync(ProductCreateDto dto)
        {
            bool categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);
            if (!categoryExists)
                return (false, 404, $"Không tìm thấy category với Id = {dto.CategoryId}", null);

            if (await _context.Products.AnyAsync(p => p.ProductCode == dto.ProductCode))
                return (false, 409, $"ProductCode '{dto.ProductCode}' đã tồn tại", null);

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
                Status = dto.StockQuantity > 0 ? "in_stock" : "out_of_stock",
                IsActive = true,
                CategoryId = dto.CategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var categoryName = await _context.Categories.Where(c => c.Id == dto.CategoryId).Select(c => c.Name).FirstOrDefaultAsync() ?? "";

            var readDto = new ProductReadDto(
                product.Id, product.ProductCode, product.Name, product.Description, product.Price, product.OriginalPrice,
                product.Weight, product.Size, product.StockQuantity, product.ImageUrl, product.Status,
                product.CreatedAt, product.UpdatedAt, product.CategoryId, categoryName
            );

            return (true, 201, "Tạo thành công", readDto);
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message, int Added, int Skipped)> BulkCreateAsync(List<ProductCreateDto> dtos)
        {
            var categoryIds = dtos.Select(d => d.CategoryId).Distinct().ToList();
            var validCategoryIds = await _context.Categories
                .Where(c => categoryIds.Contains(c.Id) && c.IsActive)
                .Select(c => c.Id)
                .ToListAsync();

            var invalidCategoryIds = categoryIds.Except(validCategoryIds).ToList();
            if (invalidCategoryIds.Any())
                return (false, 404, $"CategoryId không hợp lệ: {string.Join(", ", invalidCategoryIds)}", 0, 0);

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
                return (false, 409, "Tất cả mã đều đã tồn tại", 0, dtos.Count);

            _context.Products.AddRange(toAdd);
            await _context.SaveChangesAsync();

            return (true, 200, $"Đã thêm {toAdd.Count} sản phẩm, bỏ qua {dtos.Count - toAdd.Count} sản phẩm trùng", toAdd.Count, dtos.Count - toAdd.Count);
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message)> UpdateAsync(int id, ProductUpdateDto dto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (product == null)
                return (false, 404, $"Không tìm thấy sản phẩm với Id = {id}");

            bool categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);
            if (!categoryExists)
                return (false, 404, $"Không tìm thấy category với Id = {dto.CategoryId}");

            // ✅ THÊM LOGIC XÓA FILE ẢNH CŨ NẾU CÓ ẢNH MỚI
            if (!string.IsNullOrEmpty(dto.ImageUrl) && dto.ImageUrl != product.ImageUrl)
            {
                // Nếu sản phẩm đã từng có ảnh cũ, tiến hành xóa nó
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    // Lấy đường dẫn tuyệt đối của file cũ trên ổ cứng. 
                    // TrimStart('/') để cắt dấu gạch chéo đầu tiên của "/uploads/products/..."
                    var oldFilePath = Path.Combine(_environment.WebRootPath, product.ImageUrl.TrimStart('/'));

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath); // Xóa file
                    }
                }
            }

            product.Name = dto.Name.Trim();
            product.Description = dto.Description?.Trim();
            product.Price = dto.Price;
            product.OriginalPrice = dto.OriginalPrice;
            product.Weight = dto.Weight?.Trim();
            product.Size = dto.Size?.Trim();
            product.StockQuantity = dto.StockQuantity;
            product.ImageUrl = dto.ImageUrl?.Trim(); // Cập nhật link mới vào DB
            product.Status = dto.StockQuantity > 0 ? "in_stock" : "out_of_stock";
            product.CategoryId = dto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, 204, "Cập nhật thành công");
        }
        public async Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (product == null)
                return (false, 404, $"Không tìm thấy sản phẩm với Id = {id}");

            product.IsActive = false;
            product.DeletedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, 204, "Xóa thành công");
        }

        // ✅ HÀM XỬ LÝ LƯU FILE XUỐNG Ổ CỨNG
        public async Task<(bool IsSuccess, string Message, string? FileUrl)> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "Không có file nào được chọn.", null);

            // Kiểm tra định dạng ảnh (chỉ cho phép jpg, png, jpeg, gif)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return (false, "Chỉ cho phép tải lên file ảnh (.jpg, .jpeg, .png, .gif)", null);

            // Kiểm tra dung lượng (VD: giới hạn 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return (false, "Kích thước ảnh không được vượt quá 5MB", null);

            // Tạo thư mục wwwroot/uploads/products nếu chưa có
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "products");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Đổi tên file để tránh trùng lặp (Dùng Guid)
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Copy file vào thư mục
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn tương đối để lưu vào DB (ImageUrl)
            var fileUrl = $"/uploads/products/{fileName}";
            return (true, "Tải ảnh thành công", fileUrl);
        }
    }
}