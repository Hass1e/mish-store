// Dtos/Users/ChangeUserRoleRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Mish.Api.Dtos;

public class ChangeUserRoleRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Range(1, int.MaxValue)]
    public int RoleId { get; set; } // Только ID, никаких строк!
}