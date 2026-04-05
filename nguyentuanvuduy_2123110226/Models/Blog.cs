using System.ComponentModel.DataAnnotations;

namespace nguyentuanvuduy_2123110226.Models
{
    public class Blog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int BlogCategoryId { get; set; }

        // ✅ ĐÃ SỬA CHỖ NÀY: Bỏ "= new BlogCategory()" đi, thay bằng "= null!"
        public BlogCategory BlogCategory { get; set; } = null!;
    }
}