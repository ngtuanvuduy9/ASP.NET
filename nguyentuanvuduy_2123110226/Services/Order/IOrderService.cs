using nguyentuanvuduy_2123110226.DTOs;

namespace nguyentuanvuduy_2123110226.Services
{
    public interface IOrderService
    {
        Task<(int Total, IEnumerable<OrderSummaryDto> Data)> GetAllAsync(int page, int size, string? status);
        Task<OrderReadDto?> GetByIdAsync(int id);
        Task<OrderTrackDto?> TrackAsync(string orderCode);
        Task<(bool IsSuccess, int StatusCode, string Message, OrderCreateResponseDto? Data)> CreateAsync(OrderCreateDto dto);
        Task<(bool IsSuccess, int StatusCode, string Message)> UpdateStatusAsync(int id, OrderStatusUpdateDto dto);
        Task<(bool IsSuccess, int StatusCode, string Message)> DeleteAsync(int id);
    }
}