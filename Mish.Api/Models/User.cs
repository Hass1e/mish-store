// Models/User.cs

using System.ComponentModel.DataAnnotations; // Для [Required] и [MaxLength]

namespace Mish.Api.Models;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty; // Никогда не храни plaintext!
    
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "User"; // "Admin" или "User"
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}