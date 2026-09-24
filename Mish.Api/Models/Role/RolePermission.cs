using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mish.Api.Models;

[Table("RolePermissions")] // Явное имя таблицы
public class RolePermission
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int RoleId { get; set; }
    [ForeignKey(nameof(RoleId))]
    public Role Role { get; set; } = null!;
    
    [Required]
    public int PermissionId { get; set; }
    [ForeignKey(nameof(PermissionId))]
    public Permission Permission { get; set; } = null!;
}