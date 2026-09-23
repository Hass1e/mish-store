using Mish.Api.Models;

namespace Mish.Api.Services;

public interface IUserService
{
    Task<User?> GetByIdAsync(int userId);
    Task ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    Task DeleteUserAsync(int userId);
}