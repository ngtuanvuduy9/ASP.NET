using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Models;

namespace nguyentuanvuduy_2123110226.Services
{
    public class ContactService(AppDbContext context) : IContactService
    {
        public async Task<IEnumerable<ContactReadDto>> GetAllAsync()
        {
            return await context.Contacts
                .AsNoTracking()
                .OrderByDescending(c => c.SentAt)
                .Select(c => new ContactReadDto(c.Id, c.Name, c.Email, c.Message, c.SentAt))
                .ToListAsync();
        }

        public async Task<ContactReadDto?> GetByIdAsync(int id)
        {
            return await context.Contacts
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new ContactReadDto(c.Id, c.Name, c.Email, c.Message, c.SentAt))
                .FirstOrDefaultAsync();
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message, ContactReadDto? Data)> CreateAsync(ContactCreateDto dto)
        {
            var contact = new Contact
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim(),
                Message = dto.Message.Trim(),
                SentAt = DateTime.UtcNow // Dùng UtcNow sẽ chuẩn hơn khi deploy lên server thật
            };

            context.Contacts.Add(contact);
            await context.SaveChangesAsync();

            var result = new ContactReadDto(contact.Id, contact.Name, contact.Email, contact.Message, contact.SentAt);
            return (true, 201, "Gửi lời nhắn thành công!", result);
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id)
        {
            var contact = await context.Contacts.FindAsync(id);
            if (contact == null)
                return (false, 404, $"Không tìm thấy lời nhắn với Id = {id}");

            // Xóa cứng (Hard Delete)
            context.Contacts.Remove(contact);
            await context.SaveChangesAsync();
            return (true, 204, "Xóa lời nhắn thành công");
        }
    }
}