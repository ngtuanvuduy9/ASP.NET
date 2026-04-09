using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nguyentuanvuduy_2123110226.Services
{
    public class FavoriteService(AppDbContext context) : IFavoriteService
    {
        public async Task<IEnumerable<FavoriteReadDto>> GetCustomerFavoritesAsync(int customerId)
        {
            // Dùng Include để đảm bảo lấy kèm thông tin Product, tránh lỗi null reference
            return await context.Favorites
                .Include(f => f.Product)
                .AsNoTracking()
                .Where(f => f.CustomerId == customerId)
                .OrderByDescending(f => f.AddedAt)
                .Select(f => new FavoriteReadDto(
                    f.ProductId,
                    f.Product != null ? f.Product.Name : "Bánh không xác định",
                    f.Product != null ? f.Product.ImageUrl ?? "" : "",
                    f.Product != null ? f.Product.Price : 0m,
                    f.AddedAt
                ))
                .ToListAsync();
        }

        public async Task<(bool isFavorite, string message)> ToggleFavoriteAsync(int customerId, int productId)
        {
            // 1. Kiểm tra xem sản phẩm có thực sự tồn tại trong DB không
            var productExists = await context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
            {
                return (false, "Sản phẩm không tồn tại!");
            }

            // 2. Tìm xem khách hàng đã thích bánh này chưa
            var existingFavorite = await context.Favorites
                .FirstOrDefaultAsync(f => f.CustomerId == customerId && f.ProductId == productId);

            if (existingFavorite != null)
            {
                // Nếu đã thích rồi thì xóa (Unlike)
                context.Favorites.Remove(existingFavorite);
                await context.SaveChangesAsync();
                return (false, "Đã bỏ yêu thích sản phẩm.");
            }

            // 3. Nếu chưa thích thì thêm mới (Like)
            var newFavorite = new Favorite
            {
                CustomerId = customerId,
                ProductId = productId
            };
            context.Favorites.Add(newFavorite);
            await context.SaveChangesAsync();

            return (true, "Đã thêm vào danh sách yêu thích.");
        }
    }
}