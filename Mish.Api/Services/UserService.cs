using Microsoft.EntityFrameworkCore;
using Mish.Api.Data;      // Твой DbContext
using Mish.Api.Models;    // Твоя модель User

namespace Mish.Api.Services;

public class UserService : IUserService  // ← Имя должно быть ТОЧНО UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new UnauthorizedAccessException("Пользователь не найден");

        // Проверка пароля (используй свой хешер!)
        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Неверный старый пароль");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<(bool RoleExists, bool UserExists)> ChangeRoleAsync(string username, int roleId)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
            return (false, false);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
            return (true, false);

        user.Role = role.Name;
        await _context.SaveChangesAsync();
        return (true, true);
    }
}