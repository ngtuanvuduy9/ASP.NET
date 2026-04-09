using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Models;

namespace nguyentuanvuduy_2123110226.Services;

public class CustomerService(AppDbContext context) : ICustomerService
{
    public async Task<IEnumerable<CustomerReadDto>> GetAllAsync()
    {
        return await context.Customers
            .AsNoTracking()
            .Select(c => new CustomerReadDto(c.Id, c.FullName, c.Email, c.Points))
            .ToListAsync();
    }

    public async Task<CustomerReadDto?> GetByIdAsync(int id)
    {
        return await context.Customers
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CustomerReadDto(c.Id, c.FullName, c.Email, c.Points))
            .FirstOrDefaultAsync();
    }

    public async Task<(bool IsSuccess, int StatusCode, string Message, CustomerReadDto? Data)> CreateAsync(CustomerCreateDto dto)
    {
        if (await context.Customers.AnyAsync(c => c.Email == dto.Email.Trim()))
            return (false, 409, "Email này đã được sử dụng!", null);

        var customer = new Customer
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim(),
            PasswordHash = dto.Password, // ⚠️ Tương lai bạn nhớ băm (Hash) mật khẩu nhé
            Points = 0
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var result = new CustomerReadDto(customer.Id, customer.FullName, customer.Email, customer.Points);
        return (true, 201, "Đăng ký thành công", result);
    }

    public async Task<(bool IsSuccess, int StatusCode, string Message)> UpdateAsync(int id, CustomerUpdateDto dto)
    {
        var customer = await context.Customers.FindAsync(id);
        if (customer == null)
            return (false, 404, $"Không tìm thấy khách hàng với Id = {id}");

        // Kiểm tra xem Email định sửa có bị trùng với khách hàng KHÁC không
        if (await context.Customers.AnyAsync(c => c.Id != id && c.Email == dto.Email.Trim()))
            return (false, 409, "Email này đã được sử dụng bởi khách hàng khác!");

        customer.FullName = dto.FullName.Trim();
        customer.Email = dto.Email.Trim();
        customer.Points = dto.Points;

        await context.SaveChangesAsync();
        return (true, 204, "Cập nhật thành công");
    }

    public async Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id)
    {
        var customer = await context.Customers.FindAsync(id);
        if (customer == null)
            return (false, 404, $"Không tìm thấy khách hàng với Id = {id}");

        // Xóa cứng (Hard Delete)
        context.Customers.Remove(customer);
        await context.SaveChangesAsync();
        return (true, 204, "Xóa thành công");
    }
}