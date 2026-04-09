using System.ComponentModel.DataAnnotations;

namespace nguyentuanvuduy_2123110226.DTOs
{
    // 1. Dùng khi khách hàng gửi Request Đặt hàng (Đã đổi tên FullName -> ReceiverName và thêm Points)
    public record OrderCreateDto(
        [Required][StringLength(100)] string ReceiverName,
        [Required][StringLength(15)] string ReceiverPhone,
        [StringLength(100)] string? ReceiverEmail,
        [Required][StringLength(100)] string Province,
        [Required][StringLength(100)] string District,
        [Required][StringLength(255)] string Address,
        [StringLength(500)] string? Note,
        [Required] string PaymentMethod,   // "cod" | "bank_transfer" | "momo"
        int PointsToUse, // ✅ BƯỚC MỚI: Số điểm khách muốn dùng (React truyền lên 0 nếu không dùng)
        [Required] List<OrderDetailCreateDto> Items
    );

    public record OrderDetailCreateDto(
        [Required] int ProductId,
        [Required][Range(1, int.MaxValue)] int Quantity
    );

    // 2. Dùng khi trả dữ liệu Đơn hàng cho Admin/Khách hàng xem (Đã thêm CustomerId, PointsUsed, DiscountAmount)
    public record OrderReadDto(
        int Id,
        string OrderCode,
        int? CustomerId, // ✅ Nullable cho khách vãng lai
        string ReceiverName,
        string ReceiverPhone,
        string? ReceiverEmail,
        string Province,
        string District,
        string Address,
        string? Note,
        decimal SubTotal,
        int PointsUsed,       // ✅ Số điểm đã trừ
        decimal DiscountAmount, // ✅ Số tiền được giảm
        decimal ShippingFee,
        decimal TotalAmount,
        string PaymentMethod,
        string PaymentStatus,
        string Status,
        DateTime CreatedAt,
        List<OrderDetailReadDto> Items
    // List<PaymentReadDto> Payments // ⚠️ Tạm thời comment dòng này, khi nào làm tới file Payment thì mở ra nhé!
    );

    public record OrderDetailReadDto(
        int ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity,
        decimal SubTotal
    );

    // 3. Dùng khi Admin cập nhật trạng thái Giao hàng
    public record OrderStatusUpdateDto(
        [Required] string Status,   // pending | confirmed | shipping | delivered | cancelled
        string? CancelReason = null // ✅ Bổ sung thêm dòng này
    );

    // 4. DTO gộp cho danh sách GetAll (Đã đổi FullName thành ReceiverName)
    public record OrderSummaryDto(
        int Id, string OrderCode, string ReceiverName, string ReceiverPhone,
        decimal TotalAmount, string PaymentMethod, string PaymentStatus,
        string Status,
        string? CancelReason,
        DateTime CreatedAt
    );

    // 5. DTO cho hàm Track (Theo dõi đơn hàng - Đã đổi FullName thành ReceiverName)
    public record OrderTrackDto(
        string OrderCode, string ReceiverName, string Status,
        string PaymentMethod, string PaymentStatus, decimal TotalAmount,
        DateTime CreatedAt,
        string? CancelReason, 
        List<OrderTrackItemDto> Items
    );
    public record OrderTrackItemDto(string ProductName, decimal UnitPrice, int Quantity, decimal SubTotal);

    // 6. DTO trả về khi Create thành công
    public record OrderCreateResponseDto(
            int Id,
            string OrderCode,
            decimal TotalAmount,
            string PaymentMethod,
            string? CheckoutUrl // ĐỂ LƯU LINK MỞ MÃ QR CỦA PAYOS
        );
}