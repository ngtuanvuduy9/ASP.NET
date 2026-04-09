namespace nguyentuanvuduy_2123110226.DTOs
{
    // Dùng record cho gọn nhẹ, Frontend chỉ cần thông tin cơ bản của bánh
    public record FavoriteReadDto(
        int ProductId,
        string ProductName,
        string ImageUrl,
        decimal Price,
        DateTime AddedAt
    );
}