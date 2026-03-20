using nguyentuanvuduy_2123110226.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nguyentuanvuduy_2123110226.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string ProductCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,0)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal? OriginalPrice { get; set; }

        [StringLength(50)]
        public string? Weight { get; set; }       // VD: "500g", "1kg"

        [StringLength(50)]
        public string? Size { get; set; }         // VD: "Size M", "Size L"

        public int StockQuantity { get; set; } = 0;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "in_stock";
        // in_stock | out_of_stock | pre_order

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }  // Soft delete

        // Foreign key
        [Required]
        public int CategoryId { get; set; }

        // Navigation property
        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;
    }
}

