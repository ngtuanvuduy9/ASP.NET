using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.DTOs;

namespace nguyentuanvuduy_2123110226.Services
{
    public class PaymentService(AppDbContext context) : IPaymentService
    {
        public async Task<IEnumerable<PaymentSummaryDto>> GetAllAsync()
        {
            return await context.Payments
                .AsNoTracking()
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentSummaryDto(
                    p.Id, p.OrderId, p.Order.OrderCode, p.PaymentMethod,
                    p.Amount, p.Status, p.TransactionId, p.PaymentDate
                ))
                .ToListAsync();
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message)> UpdatePaymentStatusAsync(int id, PaymentStatusUpdateDto dto)
        {
            var validStatuses = new[] { "completed", "failed" };
            if (!validStatuses.Contains(dto.Status))
                return (false, 400, "Status phải là completed hoặc failed");

            // Chỉ đích danh Models.Payment nếu có xung đột tên
            var payment = await context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return (false, 404, $"Không tìm thấy giao dịch Id = {id}");

            if (payment.Status == "completed")
                return (false, 409, "Giao dịch này đã hoàn tất từ trước!");

            payment.Status = dto.Status;
            if (!string.IsNullOrEmpty(dto.TransactionId))
            {
                payment.TransactionId = dto.TransactionId;
            }
            payment.PaymentDate = DateTime.UtcNow;

            if (dto.Status == "completed" && payment.Order != null)
            {
                payment.Order.PaymentStatus = "paid";
                payment.Order.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            return (true, 204, "Cập nhật thành công");
        }
    }
}