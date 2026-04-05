using System.ComponentModel.DataAnnotations;

namespace nguyentuanvuduy_2123110226.Models
{
    public class BlogCategory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
