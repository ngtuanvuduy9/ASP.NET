using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Services;

namespace nguyentuanvuduy_2123110226.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController(ICustomerService customerService) : ControllerBase
    {
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await customerService.GetAllAsync();
            return Ok(data);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await customerService.GetByIdAsync(id);
            if (customer == null) return NotFound(new { message = $"Không tìm thấy khách hàng Id = {id}" });
            return Ok(customer);
        }

        [HttpPost] // Mở cửa cho khách đăng ký
        public async Task<IActionResult> Create([FromBody] CustomerCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await customerService.CreateAsync(dto);
            if (!result.IsSuccess) return Conflict(new { message = result.Message });

            // Trả về kèm theo route GetById vừa tạo
            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await customerService.UpdateAsync(id, dto);

            if (!result.IsSuccess)
            {
                if (result.StatusCode == 404) return NotFound(new { message = result.Message });
                return Conflict(new { message = result.Message });
            }

            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await customerService.DeleteAsync(id);
            if (!result.IsSuccess) return NotFound(new { message = result.Message });

            return NoContent();
        }
    }
}