using System.ComponentModel.DataAnnotations;

namespace nguyentuanvuduy_2123110226.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
