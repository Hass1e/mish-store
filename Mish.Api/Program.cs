using Mish.Api.Services; 
using Microsoft.EntityFrameworkCore; 
using Mish.Api.Data;               
using Mish.Api.Models;
//using Microsoft.OpenApi.Models; // ← Для кастомизации (опционально)
using Swashbuckle.AspNetCore.SwaggerGen; // ← Обязательно для AddSwaggerGen
using JwtBearerDefaults = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Mish.Api.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // ← Без этого Swagger не заработает!
//builder.Services.AddImageSharp(); // Добавляем поддержку ImageSharp
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoriesService, CategoriesService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService,UserService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); 
});

// === ДОБАВЛЕНО: Регистрация SQLite ===
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));


// Program.cs (секция builder.Services)

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
{
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,      // ← ОТКЛЮЧАЕМ
            ValidateAudience = false,    // ← ОТКЛЮЧАЕМ
            ValidateLifetime = true,     // ← Оставляем проверку времени
            ValidateIssuerSigningKey = true, // ← Оставляем проверку ключа
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["accessToken"];

                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"[DEBUG] Токен из куки найден! Длина: {token.Length}");
                    context.Token = token;
                }
                else
                {
                    Console.WriteLine("[DEBUG] Кука accessToken пуста или отсутствует!");
                }

                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[JWT ERROR] Тип: {context.Exception.GetType().Name}");
                Console.WriteLine($"[JWT ERROR] Сообщение: {context.Exception.Message}");
                Console.WriteLine($"[JWT ERROR] StackTrace: {context.Exception.StackTrace}");
                return Task.CompletedTask;
            }
        };
    }); 


builder.Services.AddAuthorization();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.Secure = CookieSecurePolicy.None;
});


builder.Services.AddPermissionPolicies();

var app = builder.Build();
// Твой CORS middleware - оставляем!

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Unspecified
});
app.UseCors("AllowReact");
app.UseStaticFiles();

app.UseAuthentication(); // ← Сначала аутентификация (проверка токена)
app.UseAuthorization();  // ← Потом авторизация (проверка ролей)


// === ДОБАВЛЕНО: Автоматическое заполнение БД при старте ===
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        
        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new Product 
                { 
                    Name = "Футболка Mish", 
                    Price = 1990, 
                    Description = "Базовая футболка", 
                    ImageUrls = new List<string> { "/images/tshirt.jpg" }, // ✅ Массив!
                    SKU = "TSH-001",
                    IsActive = true,
                    IsNew = true
                },
                new Product 
                { 
                    Name = "Худи Mish", 
                    Price = 3490, 
                    Description = "Тёплое худи", 
                    ImageUrls = new List<string> { "/images/hoodie.jpg" }, // ✅ Массив!
                    SKU = "HOO-001",
                    IsActive = true
                },
                new Product 
                { 
                    Name = "Кепка Mish", 
                    Price = 990, 
                    Description = "Кепка с логотипом", 
                    ImageUrls = new List<string> { "/images/cap.jpg" }, // ✅ Массив!
                    SKU = "CAP-001",
                    IsActive = true
                }
            );
            await context.SaveChangesAsync();
            Console.WriteLine("База данных успешно заполнена!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при инициализации БД: {ex.Message}");
    }
}




if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var builder = WebApplication.CreateBuilder(args);

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedPermissionsAsync(db);
    await DbSeeder.SeedCategoriesAsync(db);
    
}

app.Run();