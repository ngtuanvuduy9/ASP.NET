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

        // ✅ 1. LIÊN KẾT KHÁCH HÀNG (Bắt buộc phải là Nullable 'int?' cho khách vãng lai)
        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        // ✅ 2. THÔNG TIN NGƯỜI NHẬN (Đổi tên thành Receiver cho chuẩn nghĩa Snapshot)
        [Required]
        [StringLength(100)]
        public string ReceiverName { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string ReceiverPhone { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ReceiverEmail { get; set; }

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
        // ✅ THÊM CỘT NÀY ĐỂ LƯU LÝ DO HỦY
        [StringLength(500)]
        public string? CancelReason { get; set; }

        // ✅ 3. XỬ LÝ TIỀN BẠC & ĐIỂM GIẢM GIÁ
        [Column(TypeName = "decimal(18,0)")]
        public decimal SubTotal { get; set; }       // Tạm tính (Tiền hàng)

        public int PointsUsed { get; set; } = 0;    // Số điểm khách đã dùng để trừ tiền

        [Column(TypeName = "decimal(18,0)")]
        public decimal DiscountAmount { get; set; } = 0; // Số tiền được giảm (VD: Dùng 20 điểm = Giảm 20.000đ)

        [Column(TypeName = "decimal(18,0)")]
        public decimal ShippingFee { get; set; } = 30000;

        [Column(TypeName = "decimal(18,0)")]
        public decimal TotalAmount { get; set; }    // Tổng cuối = SubTotal + ShippingFee - DiscountAmount

        // ✅ 4. THANH TOÁN (Giữ nguyên chờ tích hợp VNPAY/MoMo)
        [Required]
        [StringLength(20)]
        public string PaymentMethod { get; set; } = "cod";

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

        // (Nếu bạn đã có class Payment, hãy mở comment dòng này. Nếu chưa thì cứ comment lại)
         public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}