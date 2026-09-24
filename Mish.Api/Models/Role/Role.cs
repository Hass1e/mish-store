using System.ComponentModel.DataAnnotations;

namespace Mish.Api.Models;

public class Role
{
    [Key]
    public int Id { get; set; }
    
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty; // "Admin", "Seller" и т.д.
    
    [MaxLength(255)]
    public string? Description { get; set; }
    
    // Навигация (не колонка в БД!)
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}