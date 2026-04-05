using System.ComponentModel.DataAnnotations;

namespace nguyentuanvuduy_2123110226.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int Points { get; set; } // Điểm tích lũy khi mua bánh
    }
}
