using Microsoft.EntityFrameworkCore;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.DTOs;
using nguyentuanvuduy_2123110226.Models;
using PayOS;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;

namespace nguyentuanvuduy_2123110226.Services
{
    public class OrderService(AppDbContext context, PayOSClient payOSClient) : IOrderService
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
                    o.Id, o.OrderCode, o.ReceiverName, o.ReceiverPhone,
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
                    o.Id, o.OrderCode, o.CustomerId, o.ReceiverName, o.ReceiverPhone, o.ReceiverEmail,
                    o.Province, o.District, o.Address, o.Note,
                    o.SubTotal, o.PointsUsed, o.DiscountAmount, o.ShippingFee, o.TotalAmount,
                    o.PaymentMethod, o.PaymentStatus, o.Status, o.CreatedAt,
                    o.OrderDetails.Select(d => new OrderDetailReadDto(
                        d.ProductId, d.ProductName, d.UnitPrice, d.Quantity, d.SubTotal
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
                    o.OrderCode, o.ReceiverName, o.Status, o.PaymentMethod,
                    o.PaymentStatus, o.TotalAmount, o.CreatedAt,
                    o.OrderDetails.Select(d => new OrderTrackItemDto(
                        d.ProductName, d.UnitPrice, d.Quantity, d.SubTotal
                    )).ToList()
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetMyOrdersAsync(int customerId)
        {
            return await context.Orders
                .AsNoTracking()
                .Where(o => o.CustomerId == customerId && o.IsActive)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderSummaryDto(
                    o.Id, o.OrderCode, o.ReceiverName, o.ReceiverPhone,
                    o.TotalAmount, o.PaymentMethod, o.PaymentStatus,
                    o.Status, o.CreatedAt
                ))
                .ToListAsync();
        }

        public async Task<(bool IsSuccess, int StatusCode, string Message, OrderCreateResponseDto? Data)> CreateAsync(int? customerId, OrderCreateDto dto)
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
            decimal discountAmount = 0;
            int pointsUsed = 0;

            if (customerId.HasValue && dto.PointsToUse > 0)
            {
                var customer = await context.Customers.FindAsync(customerId.Value);
                if (customer != null && customer.Points >= dto.PointsToUse)
                {
                    pointsUsed = dto.PointsToUse;
                    discountAmount = pointsUsed * 1000m;
                    customer.Points -= pointsUsed;
                }
                else
                {
                    return (false, 400, "Điểm tích lũy không hợp lệ hoặc không đủ!", null);
                }
            }

            var totalAmount = subTotal + shippingFee - discountAmount;
            if (totalAmount < 0) totalAmount = 0;

            var orderCode = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
            var order = new Order
            {
                OrderCode = orderCode,
                CustomerId = customerId,
                ReceiverName = dto.ReceiverName.Trim(),
                ReceiverPhone = dto.ReceiverPhone.Trim(),
                ReceiverEmail = dto.ReceiverEmail?.Trim(),
                Province = dto.Province.Trim(),
                District = dto.District.Trim(),
                Address = dto.Address.Trim(),
                Note = dto.Note?.Trim(),
                SubTotal = subTotal,
                PointsUsed = pointsUsed,
                DiscountAmount = discountAmount,
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

            string? checkoutUrl = null;
            if (dto.PaymentMethod == "bank_transfer" && totalAmount > 0)
            {
                try
                {
                    // ✅ GIẢI PHÁP: Tạo mã orderCode duy nhất cho PayOS để tránh lỗi Duplicate
                    long uniquePayosCode = long.Parse(DateTime.Now.ToString("yyMMddHHmm") + order.Id.ToString());

                    var paymentRequest = new CreatePaymentLinkRequest
                    {
                        OrderCode = uniquePayosCode,
                        Amount = (int)totalAmount,
                        Description = $"Thanh toan don {order.Id}",
                        ReturnUrl = "http://localhost:5173/my-orders?status=success",
                        CancelUrl = "http://localhost:5173/checkout?status=cancel"
                    };

                    var paymentLink = await payOSClient.PaymentRequests.CreateAsync(paymentRequest);
                    checkoutUrl = paymentLink.CheckoutUrl;
                }
                catch (Exception ex)
                {
                    // Log lỗi để debug nếu cần
                    Console.WriteLine("PayOS Error: " + ex.Message);
                    return (true, 201, $"Đặt hàng thành công, nhưng QR đang gặp lỗi: {ex.Message}",
                        new OrderCreateResponseDto(order.Id, order.OrderCode, order.TotalAmount, "cod", null));
                }
            }

            var result = new OrderCreateResponseDto(order.Id, order.OrderCode, order.TotalAmount, order.PaymentMethod, checkoutUrl);
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
                if (order.CustomerId.HasValue && order.PointsUsed > 0)
                {
                    var customer = await context.Customers.FindAsync(order.CustomerId.Value);
                    if (customer != null) customer.Points += order.PointsUsed;
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