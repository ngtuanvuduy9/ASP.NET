using nguyentuanvuduy_2123110226.DTOs;

namespace nguyentuanvuduy_2123110226.Services
{
    public interface IOrderService
    {
        Task<(int Total, IEnumerable<OrderSummaryDto> Data)> GetAllAsync(int page, int size, string? status);
        Task<OrderReadDto?> GetByIdAsync(int id);
        Task<OrderTrackDto?> TrackAsync(string orderCode);

        // Thêm tham số int? customerId để biết là Khách hay Thành viên
        Task<(bool IsSuccess, int StatusCode, string Message, OrderCreateResponseDto? Data)> CreateAsync(int? customerId, OrderCreateDto dto);

        Task<(bool IsSuccess, int StatusCode, string Message)> UpdateStatusAsync(int id, OrderStatusUpdateDto dto);
        Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id);

        // Thêm hàm này để React hiển thị lịch sử mua hàng của User
        Task<IEnumerable<OrderSummaryDto>> GetMyOrdersAsync(int customerId);
    }
}