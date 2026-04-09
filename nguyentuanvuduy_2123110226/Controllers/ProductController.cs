using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Services;
using System.Security.Claims;

namespace nguyentuanvuduy_2123110226.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        private int? GetCurrentCustomerId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int customerId))
            {
                return customerId;
            }
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? keyword = null)
        {
            if (page < 1 || size < 1)
                return BadRequest(new { message = "page và size phải lớn hơn 0" });

            var customerId = GetCurrentCustomerId();
            var (total, data) = await _productService.GetAllAsync(page, size, categoryId, keyword, customerId);

            return Ok(new { total, page, size, data });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            // ✅ ĐÃ SỬA: Lấy customerId và truyền xuống Service
            var customerId = GetCurrentCustomerId();
            var product = await _productService.GetByIdAsync(id, customerId);

            if (product == null)
                return NotFound(new { message = $"Không tìm thấy sản phẩm với Id = {id}" });

            return Ok(product);
        }

        // [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _productService.CreateAsync(dto);
            if (!result.IsSuccess)
            {
                if (result.StatusCode == 404) return NotFound(new { message = result.Message });
                if (result.StatusCode == 409) return Conflict(new { message = result.Message });
            }
            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, new { result.Data.Id, result.Data.ProductCode });
        }

        // [Authorize(Roles = "admin")]
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate([FromBody] List<ProductCreateDto> dtos)
        {
            if (dtos == null || !dtos.Any()) return BadRequest(new { message = "Danh sách trống" });
            var result = await _productService.BulkCreateAsync(dtos);
            if (!result.IsSuccess)
            {
                if (result.StatusCode == 404) return NotFound(new { message = result.Message });
                if (result.StatusCode == 409) return Conflict(new { message = result.Message });
            }
            return Ok(new { message = result.Message });
        }

        // [Authorize(Roles = "admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _productService.UpdateAsync(id, dto);
            if (!result.IsSuccess)
            {
                if (result.StatusCode == 404) return NotFound(new { message = result.Message });
            }
            return NoContent();
        }

        // [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                if (result.StatusCode == 404) return NotFound(new { message = result.Message });
            }
            return NoContent();
        }

        // [Authorize(Roles = "admin")]
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var result = await _productService.UploadImageAsync(file);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(new { url = result.FileUrl });
        }
    }
}