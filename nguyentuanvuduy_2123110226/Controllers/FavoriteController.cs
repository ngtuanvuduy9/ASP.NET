using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nguyentuanvuduy_2123110226.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace nguyentuanvuduy_2123110226.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bắt buộc phải có token JWT mới gọi được
    public class FavoriteController(IFavoriteService favoriteService) : ControllerBase
    {
        // Hàm phụ trợ để lấy ID khách hàng từ Token
        private int GetCurrentCustomerId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");
            if (idClaim != null && int.TryParse(idClaim.Value, out int customerId))
            {
                return customerId;
            }
            throw new System.Exception("Không tìm thấy thông tin xác thực.");
        }

        // GET: api/Favorite
        // Lấy danh sách bánh yêu thích của tôi
        [HttpGet]
        public async Task<IActionResult> GetMyFavorites()
        {
            var customerId = GetCurrentCustomerId();
            var favorites = await favoriteService.GetCustomerFavoritesAsync(customerId);
            return Ok(new { data = favorites });
        }

        // POST: api/Favorite/5
        // Bấm nút tim (Truyền ID của bánh vào URL)
        [HttpPost("{productId:int}")]
        public async Task<IActionResult> ToggleFavorite(int productId)
        {
            var customerId = GetCurrentCustomerId();
            var result = await favoriteService.ToggleFavoriteAsync(customerId, productId);

            return Ok(new
            {
                isFavorite = result.isFavorite,
                message = result.message
            });
        }
    }
}