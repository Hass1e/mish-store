using Mish.Api.Models;
using Mish.Api.Dtos;

namespace Mish.Api.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<User?> GetByIdAsync(int userId);
}