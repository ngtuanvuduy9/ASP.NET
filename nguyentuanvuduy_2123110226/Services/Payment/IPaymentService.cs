using nguyentuanvuduy_2123110226.DTOs;

namespace nguyentuanvuduy_2123110226.Services
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentSummaryDto>> GetAllAsync();
        Task<(bool IsSuccess, int StatusCode, string Message)> UpdatePaymentStatusAsync(int id, PaymentStatusUpdateDto dto);
    }
}