using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Models;

namespace nguyentuanvuduy_2123110226.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor này bắt buộc phải có để nhận Connection String từ  Program.cs
     public AppDbContext(DbContextOptions<AppDbContext> options) :
    base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}
