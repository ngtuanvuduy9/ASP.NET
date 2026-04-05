using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Models;

namespace nguyentuanvuduy_2123110226.Services
{
    public class OrderService(AppDbContext context) : IOrderService
    {
        public async Task<(int Total, IEnumerable<OrderSummaryDto> Data)> GetAllAsync(int page, int size, string? status)
        {
            var query = context.Orders.AsNoTracking().Where(o => o.IsActive);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(o => new OrderSummaryDto(
                    o.Id, o.OrderCode, o.FullName, o.Phone,
                    o.TotalAmount, o.PaymentMethod, o.PaymentStatus,
                    o.Status, o.CreatedAt
                ))
                .ToListAsync();

            return (total, data);
        }

        public async Task<OrderReadDto?> GetByIdAsync(int id)
        {
            return await context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id && o.IsActive)
                .Select(o => new OrderReadDto(
                    o.Id, o.OrderCode, o.FullName, o.Phone, o.Email,
                    o.Province, o.District, o.Address, o.Note,
                    o.SubTotal, o.ShippingFee, o.TotalAmount,
                    o.PaymentMethod, o.PaymentStatus, o.Status, o.CreatedAt,
                    o.OrderDetails.Select(d => new OrderDetailReadDto(
                        d.ProductId, d.ProductName, d.UnitPrice, d.Quantity, d.SubTotal
                    )).ToList(),
                    o.Payments.Select(p => new PaymentReadDto(
                        p.Id, p.PaymentMethod, p.Amount, p.Status, p.TransactionId, p.PaymentDate
                    )).ToList()
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<OrderTrackDto?> TrackAsync(string orderCode)
        {
            return await context.Orders
                .AsNoTracking()
                .Where(o => o.OrderCode == orderCode && o.IsActive)
                .Select(o => new OrderTrackDto(
                    o.OrderCode, o.FullName, o.Status, o.PaymentMethod,
                    o.PaymentStatus, o.TotalAmount, o.CreatedAt,
                    o.OrderDetails.Select(d => new OrderTrackItemDto(
                        d.ProductName, d.UnitPrice, d.Quantity, d.SubTotal
                    )).ToList()
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message, OrderCreateResponseDto? Data)> CreateAsync(OrderCreateDto dto)
        {
            var validPayments = new[] { "cod", "bank_transfer", "momo" };
            if (!validPayments.Contains(dto.PaymentMethod))
                return (false, 400, "PaymentMethod chỉ chấp nhận: cod, bank_transfer, momo", null);

            var productIds = dto.Items.Select(i => i.ProductId).ToList();
            var products = await context.Products.Where(p => productIds.Contains(p.Id) && p.IsActive).ToListAsync();

            var missingIds = productIds.Except(products.Select(p => p.Id)).ToList();
            if (missingIds.Any())
                return (false, 404, $"Sản phẩm không tồn tại: {string.Join(", ", missingIds)}", null);

            var insufficientStock = new List<string>();
            foreach (var item in dto.Items)
            {
                var p = products.First(x => x.Id == item.ProductId);
                if (p.StockQuantity < item.Quantity) insufficientStock.Add($"{p.Name}");
            }
            if (insufficientStock.Any())
                return (false, 409, $"Hết hàng: {string.Join("; ", insufficientStock)}", null);

            var details = new List<OrderDetail>();
            foreach (var item in dto.Items)
            {
                var p = products.First(x => x.Id == item.ProductId);
                p.StockQuantity -= item.Quantity;
                if (p.StockQuantity == 0) p.Status = "out_of_stock";

                details.Add(new OrderDetail
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    UnitPrice = p.Price,
                    Quantity = item.Quantity,
                    SubTotal = p.Price * item.Quantity
                });
            }

            var subTotal = details.Sum(d => d.SubTotal);
            var shippingFee = 30000m;
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

            var payment = new Models.Payment
            {
                PaymentMethod = dto.PaymentMethod,
                Amount = totalAmount,
                Status = "pending",
                PaymentDate = DateTime.UtcNow
            };
            order.Payments.Add(payment);

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var result = new OrderCreateResponseDto(order.Id, order.OrderCode, order.TotalAmount, order.PaymentMethod);
            return (true, 201, "Đặt hàng thành công!", result);
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message)> UpdateStatusAsync(int id, OrderStatusUpdateDto dto)
        {
            var validStatuses = new[] { "pending", "confirmed", "shipping", "delivered", "cancelled" };
            if (!validStatuses.Contains(dto.Status))
                return (false, 400, "Status không hợp lệ.");

            var order = await context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

            if (order == null) return (false, 404, $"Không tìm thấy đơn hàng Id = {id}");
            if (order.Status is "delivered" or "cancelled")
                return (false, 409, $"Đã {order.Status}, không thể sửa.");

            if (dto.Status == "cancelled")
            {
                var productIds = order.OrderDetails.Select(d => d.ProductId).ToList();
                var products = await context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();
                foreach (var detail in order.OrderDetails)
                {
                    var p = products.FirstOrDefault(x => x.Id == detail.ProductId);
                    if (p != null)
                    {
                        p.StockQuantity += detail.Quantity;
                        if (p.StockQuantity > 0 && p.Status == "out_of_stock") p.Status = "in_stock";
                    }
                }
                var pendingPayments = order.Payments.Where(p => p.Status == "pending");
                foreach (var p in pendingPayments) p.Status = "failed";
            }

            order.Status = dto.Status;
            order.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == "delivered")
            {
                order.PaymentStatus = "paid";
                var paymentToComplete = order.Payments.FirstOrDefault(p => p.Status == "pending");
                if (paymentToComplete != null)
                {
                    paymentToComplete.Status = "completed";
                    paymentToComplete.PaymentDate = DateTime.UtcNow;
                }
            }

            await context.SaveChangesAsync();
            return (true, 204, "Cập nhật thành công");
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id)
        {
            var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.IsActive);
            if (order == null) return (false, 404, $"Không tìm thấy đơn hàng với Id = {id}");

            order.IsActive = false;
            order.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return (true, 204, "Xóa (soft delete) thành công");
        }
    }
}