using Microsoft.AspNetCore.Authorization; // ✅ Thêm thư viện Auth
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.Models;
using nguyentuanvuduy_2123110226.DTOs;

namespace nguyentuanvuduy_2123110226.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Order?page=1&size=10&status=pending
        // 🔒 KHÓA: Chỉ Admin mới được xem toàn bộ danh sách đơn hàng
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? status = null)
        {
            if (page < 1 || size < 1)
                return BadRequest(new { message = "page và size phải lớn hơn 0" });

            var validStatuses = new[] { "pending", "confirmed", "shipping", "delivered", "cancelled" };
            if (status != null && !validStatuses.Contains(status))
                return BadRequest(new { message = $"Status không hợp lệ. Chỉ chấp nhận: {string.Join(", ", validStatuses)}" });

            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.IsActive);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(o => new
                {
                    o.Id,
                    o.OrderCode,
                    o.FullName,
                    o.Phone,
                    o.TotalAmount,
                    o.PaymentMethod,
                    o.PaymentStatus,
                    o.Status,
                    o.CreatedAt
                })
                .ToListAsync();

            return Ok(new { total, page, size, data });
        }

        // GET: api/Order/5
        // 🔒 KHÓA: Chỉ Admin mới được xem chi tiết theo Id (Khách sẽ dùng Track)
        [Authorize(Roles = "admin")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id && o.IsActive)
                .Select(o => new OrderReadDto(
                    o.Id,
                    o.OrderCode,
                    o.FullName,
                    o.Phone,
                    o.Email,
                    o.Province,
                    o.District,
                    o.Address,
                    o.Note,
                    o.SubTotal,
                    o.ShippingFee,
                    o.TotalAmount,
                    o.PaymentMethod,
                    o.PaymentStatus,
                    o.Status,
                    o.CreatedAt,
                    o.OrderDetails.Select(d => new OrderDetailReadDto(
                        d.ProductId,
                        d.ProductName,
                        d.UnitPrice,
                        d.Quantity,
                        d.SubTotal
                    )).ToList()
                ))
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound(new { message = $"Không tìm thấy đơn hàng với Id = {id}" });

            return Ok(order);
        }

        // GET: api/Order/track/ORD-20260327-0001
        // ✅ MỞ CỬA: Khách hàng dùng mã đơn hàng để tự tra cứu
        [HttpGet("track/{orderCode}")]
        public async Task<IActionResult> Track(string orderCode)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderCode == orderCode && o.IsActive)
                .Select(o => new
                {
                    o.OrderCode,
                    o.FullName,
                    o.Status,
                    o.PaymentMethod,
                    o.PaymentStatus,
                    o.TotalAmount,
                    o.CreatedAt,
                    Items = o.OrderDetails.Select(d => new
                    {
                        d.ProductName,
                        d.UnitPrice,
                        d.Quantity,
                        d.SubTotal
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound(new { message = $"Không tìm thấy đơn hàng '{orderCode}'" });

            return Ok(order);
        }

        // POST: api/Order  — Đặt hàng
        // ✅ MỞ CỬA: Ai cũng có thể đặt hàng
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validPayments = new[] { "cod", "bank_transfer" };
            if (!validPayments.Contains(dto.PaymentMethod))
                return BadRequest(new { message = "PaymentMethod chỉ chấp nhận: cod, bank_transfer" });

            var productIds = dto.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id) && p.IsActive)
                .ToListAsync();

            var missingIds = productIds.Except(products.Select(p => p.Id)).ToList();
            if (missingIds.Any())
                return NotFound(new { message = $"Sản phẩm không tồn tại: {string.Join(", ", missingIds)}" });

            // ✅ LÔ-GIC TỒN KHO MỚI (10/10)
            var insufficientStock = new List<string>();
            foreach (var item in dto.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                if (product.StockQuantity < item.Quantity)
                {
                    insufficientStock.Add($"{product.Name} (Còn: {product.StockQuantity}, Đặt: {item.Quantity})");
                }
            }

            if (insufficientStock.Any())
                return Conflict(new { message = $"Sản phẩm không đủ số lượng: {string.Join("; ", insufficientStock)}" });

            var details = new List<OrderDetail>();

            // ✅ TRỪ TỒN KHO VÀ TẠO DETAIL
            foreach (var item in dto.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);

                // Trừ số lượng kho
                product.StockQuantity -= item.Quantity;

                // Cập nhật trạng thái nếu hết hàng
                if (product.StockQuantity == 0)
                {
                    product.Status = "out_of_stock";
                }

                details.Add(new OrderDetail
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity,
                    SubTotal = product.Price * item.Quantity
                });
            }

            var subTotal = details.Sum(d => d.SubTotal);
            var shippingFee = 30000m; // Có thể làm linh hoạt sau
            var totalAmount = subTotal + shippingFee;

            var orderCode = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            var order = new Order
            {
                OrderCode = orderCode,
                FullName = dto.FullName.Trim(),
                Phone = dto.Phone.Trim(),
                Email = dto.Email?.Trim(),
                Province = dto.Province.Trim(),
                District = dto.District.Trim(),
                Address = dto.Address.Trim(),
                Note = dto.Note?.Trim(),
                SubTotal = subTotal,
                ShippingFee = shippingFee,
                TotalAmount = totalAmount,
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = "unpaid",
                Status = "pending",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderDetails = details
            };

            _context.Orders.Add(order);

            // ✅ SaveChangesAsync sẽ tự động lưu Order, OrderDetails và thay đổi Tồn kho của Products trong cùng 1 Transaction
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, new
            {
                order.Id,
                order.OrderCode,
                order.TotalAmount,
                order.PaymentMethod,
                message = "Đặt hàng thành công!"
            });
        }

        // PATCH: api/Order/5/status  — Cập nhật trạng thái
        // 🔒 KHÓA: Chỉ Admin
        [Authorize(Roles = "admin")]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateDto dto)
        {
            var validStatuses = new[] { "pending", "confirmed", "shipping", "delivered", "cancelled" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { message = $"Status không hợp lệ. Chỉ chấp nhận: {string.Join(", ", validStatuses)}" });

            var order = await _context.Orders
                .Include(o => o.OrderDetails) // Lấy Detail để hoàn tồn kho nếu huỷ
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

            if (order == null)
                return NotFound(new { message = $"Không tìm thấy đơn hàng với Id = {id}" });

            if (order.Status is "delivered" or "cancelled")
                return Conflict(new { message = $"Không thể cập nhật đơn hàng đã '{order.Status}'" });

            // ✅ NẾU ADMIN HUỶ ĐƠN -> HOÀN LẠI TỒN KHO
            if (dto.Status == "cancelled")
            {
                var productIds = order.OrderDetails.Select(d => d.ProductId).ToList();
                var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

                foreach (var detail in order.OrderDetails)
                {
                    var product = products.FirstOrDefault(p => p.Id == detail.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += detail.Quantity;
                        if (product.StockQuantity > 0 && product.Status == "out_of_stock")
                            product.Status = "in_stock";
                    }
                }
            }

            order.Status = dto.Status;
            order.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == "delivered")
                order.PaymentStatus = "paid";

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Order/5  — Soft delete (admin huỷ đơn vĩnh viễn)
        // 🔒 KHÓA: Chỉ Admin
        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

            if (order == null)
                return NotFound(new { message = $"Không tìm thấy đơn hàng với Id = {id}" });

            if (order.Status is "shipping" or "delivered")
                return Conflict(new { message = $"Không thể xoá đơn hàng đang '{order.Status}'" });

            order.IsActive = false;
            order.Status = "cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}