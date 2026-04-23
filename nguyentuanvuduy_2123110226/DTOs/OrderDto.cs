using System.ComponentModel.DataAnnotations;
namespace nguyentuanvuduy_2123110226.DTOs
{
    public record OrderCreateDto(
        [Required][StringLength(100)] string ReceiverName,
        [Required][StringLength(15)] string ReceiverPhone,
        [StringLength(100)] string? ReceiverEmail,
        [Required][StringLength(100)] string Province,
        [Required][StringLength(100)] string District,
        [Required][StringLength(255)] string Address,
        [StringLength(500)] string? Note,
        [Required] string PaymentMethod,
        int PointsToUse,
        [Required] List<OrderDetailCreateDto> Items
    );

    public record OrderDetailCreateDto(
        [Required] int ProductId,
        [Required][Range(1, int.MaxValue)] int Quantity
    );

    // ✅ THÊM MỚI: DTO thông tin khách hàng trong đơn hàng
    public record OrderCustomerDto(
        int Id,
        string FullName,
        string? Email,
        int Points
    );

    // ✅ SỬA: Thêm CustomerInfo vào OrderReadDto
    public record OrderReadDto(
        int Id,
        string OrderCode,
        int? CustomerId,
        OrderCustomerDto? CustomerInfo, // ✅ THÊM MỚI
        string ReceiverName,
        string ReceiverPhone,
        string? ReceiverEmail,
        string Province,
        string District,
        string Address,
        string? Note,
        string? CancelReason,           // ✅ THÊM MỚI
        decimal SubTotal,
        int PointsUsed,
        decimal DiscountAmount,
        decimal ShippingFee,
        decimal TotalAmount,
        string PaymentMethod,
        string PaymentStatus,
        string Status,
        DateTime CreatedAt,
        List<OrderDetailReadDto> Items
    );

    public record OrderDetailReadDto(
        int ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity,
        decimal SubTotal
    );

    public record OrderStatusUpdateDto(
        [Required] string Status,
        string? CancelReason = null
    );

    public record OrderSummaryDto(
        int Id, string OrderCode, string ReceiverName, string ReceiverPhone,
        decimal TotalAmount, string PaymentMethod, string PaymentStatus,
        string Status,
        string? CancelReason,
        DateTime CreatedAt
    );

    public record OrderTrackDto(
        string OrderCode, string ReceiverName, string Status,
        string PaymentMethod, string PaymentStatus, decimal TotalAmount,
        DateTime CreatedAt,
        string? CancelReason,
        List<OrderTrackItemDto> Items
    );

    public record OrderTrackItemDto(string ProductName, decimal UnitPrice, int Quantity, decimal SubTotal);

    public record OrderCreateResponseDto(
        int Id,
        string OrderCode,
        decimal TotalAmount,
        string PaymentMethod,
        string? CheckoutUrl
    );
}