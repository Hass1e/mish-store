using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mish.Api.Data; // Или где у тебя лежит AppDbContext
using Mish.Api.Models;
using Mish.Api.Dtos;
using Mish.Api.Extensions;  
using Mish.Api.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace Mish.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Доступно только авторизованным пользователям
[EnableRateLimiting("fixed")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;

    public RolesController(AppDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    // GET: api/roles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll()
    {
        var roles = await _context.Roles
            .Select(r => new RoleDto 
            { 
                Id = r.Id, 
                Name = r.Name, 
                Description = r.Description 
            })
            .ToListAsync();

        return Ok(roles);
    }


 // ← ТОЛЬКО ДЛЯ АДМИНОВ И МОДЕРАТОРОВ
    [HttpPut("change-role")]
    [RequirePermission("users.manage_roles")] 
    public async Task<IActionResult> ChangeRole([FromBody] ChangeUserRoleRequest request)
    {
        var result = await _userService.ChangeRoleAsync(request.Username, request.RoleId);
        if (!result.RoleExists)
            return BadRequest(new { message = "Указанная роль не существует" });
        if (!result.UserExists)
            return NotFound(new { message = "Пользователь не найден" });

        return Ok(new { message = $"Роль пользователя {request.Username} успешно изменена" });
    }








}

// Простой DTO для ответа (не светим лишние поля вроде навигационных свойств)
/// <summary>
/// DTO для представления роли. [Response]
/// </summary>
public class RoleDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}