using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Data;
using PayOS;
using PayOS.Models.Webhooks;

namespace nguyentuanvuduy_2123110226.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController(PayOSClient payOSClient, AppDbContext context) : ControllerBase
    {
        [HttpPost("payos")]
        public async Task<IActionResult> ReceivePayOSWebhook([FromBody] Webhook webhookBody) // ✅ Webhook, not WebhookData
        {
            try
            {
                // 1. VerifyAsync nhận Webhook (outer), trả về WebhookData (inner)
                WebhookData verifiedData = await payOSClient.Webhooks.VerifyAsync(webhookBody);

                // Bỏ qua nếu là webhook test
                if (verifiedData.Description == "Ma giao dich thu nghiem" ||
                    verifiedData.Description == "VQĐ TEST")
                {
                    return Ok(new { message = "Test webhook thành công" });
                }

                // 2. Lấy lại ID đơn hàng gốc
                string orderCodeStr = verifiedData.OrderCode.ToString();
                if (orderCodeStr.Length > 10)
                {
                    string idString = orderCodeStr[10..];
                    if (int.TryParse(idString, out int orderId))
                    {
                        var order = await context.Orders
                            .Include(o => o.Payments)
                            .FirstOrDefaultAsync(o => o.Id == orderId);

                        // 3. Mã "00" = thanh toán thành công
                        if (order != null && verifiedData.Code == "00")
                        {
                            order.PaymentStatus = "paid";
                            order.Status = "confirmed";
                            order.UpdatedAt = DateTime.UtcNow;

                            var payment = order.Payments.Count > 0
                                ? order.Payments.FirstOrDefault(p => p.Status == "pending")
                                : null;

                            if (payment != null)
                            {
                                payment.Status = "completed";
                                payment.TransactionId = verifiedData.Reference;
                                payment.PaymentDate = DateTime.UtcNow;
                            }

                            await context.SaveChangesAsync();
                        }
                    }
                }

                return Ok(new { message = "Đã nhận và xử lý Webhook thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi Webhook: " + ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}