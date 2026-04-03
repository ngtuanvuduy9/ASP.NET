using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Models;

namespace nguyentuanvuduy_2123110226.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasQueryFilter(c => c.IsActive);
            modelBuilder.Entity<Product>().HasQueryFilter(p => p.IsActive);
            modelBuilder.Entity<Order>().HasQueryFilter(o => o.IsActive);

            // ✅ Fix warning — filter khớp với Order
            modelBuilder.Entity<OrderDetail>()
                .HasQueryFilter(od => od.Order.IsActive);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            // Filter cho User
            modelBuilder.Entity<User>().HasQueryFilter(u => u.IsActive);

            // Đảm bảo Username là duy nhất (Unique)
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Tạo sẵn 1 tài khoản Admin khi chạy Migration
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "123456", // Ở bài toán thực tế, chỗ này phải là chuỗi Hash (VD: BCrypt)
                    FullName = "Administrator",
                    Role = "admin",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}