using nguyentuanvuduy_2123110226.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nguyentuanvuduy_2123110226.Services
{
    public interface IFavoriteService
    {
        Task<IEnumerable<FavoriteReadDto>> GetCustomerFavoritesAsync(int customerId);
        Task<(bool isFavorite, string message)> ToggleFavoriteAsync(int customerId, int productId);
    }
}