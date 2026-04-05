using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Services;

namespace nguyentuanvuduy_2123110226.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController(IPaymentService paymentService) : ControllerBase
    {
        // GET: api/Payment (Lấy danh sách tất cả dòng tiền)
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await paymentService.GetAllAsync();
            return Ok(data);
        }

        // PATCH: api/Payment/5/status (Báo cáo đã nhận tiền)
        // 🔒 KHÓA: Chỉ Admin mới được đánh dấu nhận tiền (hoặc API của Momo/VNPay gọi vào)
        [Authorize(Roles = "admin")]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] PaymentStatusUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await paymentService.UpdatePaymentStatusAsync(id, dto);

            if (!result.IsSuccess)
            {
                if (result.StatusCode == 400) return BadRequest(new { message = result.Message });
                if (result.StatusCode == 404) return NotFound(new { message = result.Message });
                if (result.StatusCode == 409) return Conflict(new { message = result.Message });
            }

            return NoContent();
        }
    }
}