using System.ComponentModel.DataAnnotations;

namespace Mish.Api.Models;

public class Permission
{
    [Key]
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty; // "orders.update_status"
    
    [MaxLength(255)]
    public string? Description { get; set; }
    
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}