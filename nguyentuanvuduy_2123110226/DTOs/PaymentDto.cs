using System.ComponentModel.DataAnnotations;

namespace nguyentuanvuduy_2123110226.DTOs
{
    // Dùng để hiển thị lịch sử trả tiền của 1 đơn hàng
    public record PaymentReadDto(
        int Id,
        string PaymentMethod,
        decimal Amount,
        string Status, // pending | completed | failed
        string? TransactionId,
        DateTime PaymentDate
    );

    // Dùng khi Admin hoặc Webhook của Momo/VnPay báo thanh toán thành công
    public record PaymentStatusUpdateDto(
        [Required] string Status, // completed | failed
        string? TransactionId     // Điền mã giao dịch Momo/Bank vào đây (nếu có)
    );
    public record PaymentSummaryDto(
        int Id, int OrderId, string OrderCode, string PaymentMethod,
        decimal Amount, string Status, string? TransactionId, DateTime PaymentDate
    );
}