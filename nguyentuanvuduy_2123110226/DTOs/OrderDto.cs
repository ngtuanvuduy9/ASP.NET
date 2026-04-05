using System.ComponentModel.DataAnnotations;

namespace nguyentuanvuduy_2123110226.DTOs
{
    // Dùng khi khách hàng gửi Request Đặt hàng
    public record OrderCreateDto(
        [Required][StringLength(100)] string FullName,
        [Required][StringLength(15)] string Phone,
        [StringLength(100)] string? Email,
        [Required][StringLength(100)] string Province,
        [Required][StringLength(100)] string District,
        [Required][StringLength(255)] string Address,
        [StringLength(500)] string? Note,
        [Required] string PaymentMethod,   // "cod" | "bank_transfer" | "momo"
        [Required] List<OrderDetailCreateDto> Items
    );

    public record OrderDetailCreateDto(
        [Required] int ProductId,
        [Required][Range(1, int.MaxValue)] int Quantity
    );

    // Dùng khi trả dữ liệu Đơn hàng cho Admin/Khách hàng xem
    public record OrderReadDto(
        int Id,
        string OrderCode,
        string FullName,
        string Phone,
        string? Email,
        string Province,
        string District,
        string Address,
        string? Note,
        decimal SubTotal,
        decimal ShippingFee,
        decimal TotalAmount,
        string PaymentMethod,
        string PaymentStatus,
        string Status,
        DateTime CreatedAt,
        List<OrderDetailReadDto> Items,
        List<PaymentReadDto> Payments // Danh sách lịch sử thanh toán
    );

    public record OrderDetailReadDto(
        int ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity,
        decimal SubTotal
    );

    // Dùng khi Admin cập nhật trạng thái Giao hàng
    public record OrderStatusUpdateDto(
        [Required] string Status   // pending | confirmed | shipping | delivered | cancelled
    );
    // DTO gộp cho danh sách GetAll
    public record OrderSummaryDto(
        int Id, string OrderCode, string FullName, string Phone,
        decimal TotalAmount, string PaymentMethod, string PaymentStatus,
        string Status, DateTime CreatedAt
    );

    // DTO cho hàm Track (Theo dõi đơn hàng)
    public record OrderTrackDto(
        string OrderCode, string FullName, string Status,
        string PaymentMethod, string PaymentStatus, decimal TotalAmount,
        DateTime CreatedAt, List<OrderTrackItemDto> Items
    );
    public record OrderTrackItemDto(string ProductName, decimal UnitPrice, int Quantity, decimal SubTotal);

    // DTO trả về khi Create thành công
    public record OrderCreateResponseDto(
        int Id, string OrderCode, decimal TotalAmount, string PaymentMethod
    );
}