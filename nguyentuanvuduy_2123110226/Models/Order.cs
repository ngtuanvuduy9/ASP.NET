// Models/Order.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nguyentuanvuduy_2123110226.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string OrderCode { get; set; } = string.Empty;
        // VD: "ORD-20260327-0001"

        // Thông tin khách hàng (guest checkout — không cần FK)
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Email { get; set; }

        // Địa chỉ giao hàng
        [Required]
        [StringLength(100)]
        public string Province { get; set; } = string.Empty;   // Tỉnh/Thành phố

        [Required]
        [StringLength(100)]
        public string District { get; set; } = string.Empty;   // Quận/Huyện

        [Required]
        [StringLength(255)]
        public string Address { get; set; } = string.Empty;    // Số nhà, tên đường

        [StringLength(500)]
        public string? Note { get; set; }

        // Tiền
        [Column(TypeName = "decimal(18,0)")]
        public decimal SubTotal { get; set; }       // Tạm tính

        [Column(TypeName = "decimal(18,0)")]
        public decimal ShippingFee { get; set; } = 30000;

        [Column(TypeName = "decimal(18,0)")]
        public decimal TotalAmount { get; set; }    // Tổng = SubTotal + ShippingFee

        // Thanh toán: "cod" | "bank_transfer"
        [Required]
        [StringLength(20)]
        public string PaymentMethod { get; set; } = "cod";

        // Trạng thái thanh toán: "unpaid" | "paid"
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "unpaid";

        // Trạng thái đơn: "pending" | "confirmed" | "shipping" | "delivered" | "cancelled"
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "pending";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}