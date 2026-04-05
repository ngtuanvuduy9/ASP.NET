using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Services;

namespace nguyentuanvuduy_2123110226.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController(IContactService contactService) : ControllerBase
    {
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await contactService.GetAllAsync();
            return Ok(data);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var contact = await contactService.GetByIdAsync(id);
            if (contact == null) return NotFound(new { message = $"Không tìm thấy lời nhắn Id = {id}" });
            return Ok(contact);
        }

        [HttpPost] // Mở cửa để ai cũng gửi liên hệ được
        public async Task<IActionResult> Create([FromBody] ContactCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await contactService.CreateAsync(dto);

            // Trả về đúng chuẩn RESTful thay vì chỉ Ok("Thành công") như trước
            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await contactService.DeleteAsync(id);
            if (!result.IsSuccess) return NotFound(new { message = result.Message });

            return NoContent();
        }
    }
}