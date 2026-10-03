// Dtos/Users/ChangeUserRoleRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Mish.Api.Dtos;
/// <summary>
/// DTO для изменения роли пользователя. [Request]
/// </summary>
public class ChangeUserRoleRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 100 символов")]
    public string Username { get; set; } = string.Empty;
    
    [Range(1, int.MaxValue)]
    public int RoleId { get; set; } // Только ID, никаких строк!
}