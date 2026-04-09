using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Services;
using System.Security.Claims; // 👈 Thêm thư viện này để đọc Token

namespace nguyentuanvuduy_2123110226.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(IOrderService orderService) : ControllerBase // 👈 Giữ nguyên Primary Constructor cho ngầu
    {
        // ✅ HÀM PHỤ TRỢ: Lấy ID khách hàng từ Token (nếu có)
        private int? GetCurrentCustomerId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int customerId))
            {
                return customerId;
            }
            return null; // Trả về null nếu là khách vãng lai
        }

        // GET: api/Order?page=1&size=10&status=pending
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? status = null)
        {
            if (page < 1 || size < 1) return BadRequest(new { message = "page và size phải lớn hơn 0" });

            var (total, data) = await orderService.GetAllAsync(page, size, status);
            return Ok(new { total, page, size, data });
        }

        // GET: api/Order/5
        [Authorize(Roles = "admin")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await orderService.GetByIdAsync(id);
            if (order == null) return NotFound(new { message = $"Không tìm thấy đơn hàng với Id = {id}" });
            return Ok(order);
        }

        // GET: api/Order/track/ORD-20260327-0001
        [HttpGet("track/{orderCode}")]
        public async Task<IActionResult> Track(string orderCode)
        {
            var order = await orderService.TrackAsync(orderCode);
            if (order == null) return NotFound(new { message = $"Không tìm thấy đơn hàng '{orderCode}'" });
            return Ok(order);
        }

        // ✅ TÍNH NĂNG MỚI BỔ SUNG: Lấy danh sách đơn hàng của user đang đăng nhập
        [Authorize] // Bắt buộc đăng nhập
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == null) return Unauthorized(new { message = "Vui lòng đăng nhập để xem đơn hàng." });

            var data = await orderService.GetMyOrdersAsync(customerId.Value);
            return Ok(new { data });
        }

        // POST: api/Order
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // ✅ Tự động lấy ID khách truyền vào Service (null nếu chưa đăng nhập)
            var customerId = GetCurrentCustomerId();
            var result = await orderService.CreateAsync(customerId, dto);

            if (!result.IsSuccess)
            {
                if (result.StatusCode == 404) return NotFound(new { message = result.Message });
                if (result.StatusCode == 409) return Conflict(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }

            // Giữ nguyên kiểu trả về siêu chi tiết của bạn
            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, new
            {
                result.Data.Id,
                result.Data.OrderCode,
                result.Data.TotalAmount,
                result.Data.PaymentMethod,
                result.Data.CheckoutUrl,
                message = result.Message
            });
        }

        // PATCH: api/Order/5/status
        [Authorize(Roles = "admin")]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await orderService.UpdateStatusAsync(id, dto);

            if (!result.IsSuccess)
            {
                if (result.StatusCode == 404) return NotFound(new { message = result.Message });
                if (result.StatusCode == 409) return Conflict(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }

            return NoContent();
        }

        // DELETE: api/Order/5
        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await orderService.DeleteAsync(id);
            if (!result.IsSuccess) return NotFound(new { message = result.Message });

            return NoContent();
        }
    }
}