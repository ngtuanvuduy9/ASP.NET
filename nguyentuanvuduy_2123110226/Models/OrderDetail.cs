using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nguyentuanvuduy_2123110226.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(150)]
        public string ProductName { get; set; } = string.Empty;
        // Snapshot tên SP lúc đặt — tránh bị đổi tên sau này

        [Column(TypeName = "decimal(18,0)")]
        public decimal UnitPrice { get; set; }
        // Snapshot giá lúc đặt

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal SubTotal { get; set; }   // UnitPrice * Quantity

        // Navigation
        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
    }
}