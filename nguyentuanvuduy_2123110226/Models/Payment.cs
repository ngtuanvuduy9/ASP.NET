// Models/Payment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nguyentuanvuduy_2123110226.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        // Khóa ngoại liên kết với bảng Order
        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "cod";
        // Các giá trị chuẩn: "cod", "bank_transfer", "momo", "vnpay"

        [Column(TypeName = "decimal(18,0)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "pending";
        // Trạng thái giao dịch: "pending" (Đang chờ), "completed" (Thành công), "failed" (Thất bại)

        [StringLength(100)]
        public string? TransactionId { get; set; }
        // Lưu mã giao dịch trả về từ ngân hàng/ví điện tử (VD: mã giao dịch Momo)

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    }
}