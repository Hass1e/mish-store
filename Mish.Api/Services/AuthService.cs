using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Mish.Api.Data;
using Mish.Api.Models;
using Mish.Api.Dtos;
using BCrypt.Net;

namespace Mish.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<User> RegisterAsync(RegisterRequest request)
    {
        var normalizedUsername = request.Username.Trim().ToLower();

        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername))
            throw new ArgumentException("Пользователь с таким именем уже существует");

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12); 

        var user = new User
        {
            Username = normalizedUsername,
            PasswordHash = passwordHash,
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
{
    var normalizedUsername = request.Username.Trim().ToLower();

    // 1. Поиск пользователя
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsername);

    if (user is null)
        throw new UnauthorizedAccessException("Неверное имя пользователя или пароль");

    // 2. Проверка хеша пароля
    if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        throw new UnauthorizedAccessException("Неверное имя пользователя или пароль");

    // 3. Генерация токена
    var token = GenerateJwtToken(user);
    var expirationMinutes = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"]!);

    return new AuthResponse(
        UserId: user.Id,
        Username: user.Username,
        Role: user.Role,
        AccessToken: token,
        ExpiresAt: DateTime.UtcNow.AddMinutes(expirationMinutes)
    );
}

    public Task<User?> GetByIdAsync(int userId)
    {
        return _context.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == userId);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)
        );
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Iat, 
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(jwtSettings["AccessTokenExpirationMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}