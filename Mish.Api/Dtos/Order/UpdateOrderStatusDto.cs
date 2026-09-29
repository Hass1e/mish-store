using System.ComponentModel.DataAnnotations;

namespace Mish.Api.DTOs;

public class UpdateOrderStatusDto
{
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}