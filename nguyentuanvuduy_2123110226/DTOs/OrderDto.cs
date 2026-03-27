// DTOs/OrderDto.cs
using System.ComponentModel.DataAnnotations;

namespace nguyentuanvuduy_2123110226.DTOs
{
    // DTO tạo đơn hàng
    public record OrderCreateDto(
        [Required][StringLength(100)] string FullName,
        [Required][StringLength(15)] string Phone,
        [StringLength(100)] string? Email,
        [Required][StringLength(100)] string Province,
        [Required][StringLength(100)] string District,
        [Required][StringLength(255)] string Address,
        [StringLength(500)] string? Note,
        [Required] string PaymentMethod,   // "cod" | "bank_transfer"
        [Required] List<OrderDetailCreateDto> Items
    );

    public record OrderDetailCreateDto(
        [Required] int ProductId,
        [Required][Range(1, int.MaxValue)] int Quantity
    );

    // DTO đọc đơn hàng
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
        List<OrderDetailReadDto> Items
    );

    public record OrderDetailReadDto(
        int ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity,
        decimal SubTotal
    );

    // DTO cập nhật trạng thái đơn (dùng cho admin)
    public record OrderStatusUpdateDto(
        [Required] string Status   // pending | confirmed | shipping | delivered | cancelled
    );
}