using Microsoft.EntityFrameworkCore;
using System;

namespace nguyentuanvuduy_2123110226.Models
{
    // Cài đặt khóa chính kết hợp (Composite Key) để 1 người không thể "thích" 1 bánh 2 lần
    [PrimaryKey(nameof(CustomerId), nameof(ProductId))]
    public class Favorite
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}