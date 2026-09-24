using Mish.Api.Models;

namespace Mish.Api.Services;

public interface IUserService
{
    Task<User?> GetByIdAsync(int userId);
    Task ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    Task DeleteUserAsync(int userId);
    Task<(bool RoleExists, bool UserExists)> ChangeRoleAsync(string username, int roleId);
}