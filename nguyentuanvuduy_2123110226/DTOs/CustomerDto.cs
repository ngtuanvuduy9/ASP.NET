using System.ComponentModel.DataAnnotations;

namespace nguyentuanvuduy_2123110226.DTOs
{
    public record CustomerReadDto(int Id, string FullName, string Email, int Points);

    public record CustomerCreateDto(
        [Required][StringLength(100)] string FullName,
        [Required][EmailAddress][StringLength(100)] string Email,
        [Required][StringLength(50)] string Password
    );

    public record CustomerUpdateDto(
        [Required][StringLength(100)] string FullName,
        [Required][EmailAddress][StringLength(100)] string Email,
        [Required] int Points
    );
}