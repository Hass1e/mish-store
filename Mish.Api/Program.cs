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
using Mish.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // ← Без этого Swagger не заработает!
builder.Services.AddOptions<AuthCookieSettings>()
    .Bind(builder.Configuration.GetSection("AuthCookie"))
    .Validate(settings => settings.ExpirationMinutes > 0, "AuthCookie:ExpirationMinutes должен быть больше нуля.")
    .ValidateOnStart();
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
    options.UseSqlite("Data Source=Data/app.db"));


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
builder.Services.AddMemoryCache(); // ← Для кэширования входов пользователей

// 1. Добавляем сервис Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Глобальный обработчик ошибок (что возвращать, если лимит исчерпан)
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Слишком много запросов. Пожалуйста, подождите.", token);
    };

    // 2. Создаем политику "FixedWindow" (Фиксированное окно)
    // Например: 30 запросов в минуту
    options.AddFixedWindowLimiter(policyName: "fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 35; // Количество запросов
        limiterOptions.Window = TimeSpan.FromMinutes(1); // Временной промежуток
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0; // Сколько запросов можно поставить в очередь, если лимит почти достигнут
    });

    // Можно добавить еще политику для авторизации (более строгую)
    options.AddSlidingWindowLimiter(policyName: "auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5; // Всего попыток
        limiterOptions.Window = TimeSpan.FromMinutes(1); 
        limiterOptions.SegmentsPerWindow = 0;
    });
});


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
                    ImageUrls = new List<string> { "/uploads/T-shirt/T-shirt1.png" },
                    SKU = "TSH-001",
                    IsActive = true,
                    IsNew = true
                },
                new Product 
                { 
                    Name = "Худи Mish", 
                    Price = 3490, 
                    Description = "Тёплое худи", 
                    ImageUrls = new List<string> { "/uploads/placeholder.jpg" },
                    SKU = "HOO-001",
                    IsActive = true
                },
                new Product 
                { 
                    Name = "Кепка Mish", 
                    Price = 990, 
                    Description = "Кепка с логотипом", 
                    ImageUrls = new List<string> { "/uploads/placeholder.jpg" },
                    SKU = "CAP-001",
                    IsActive = true
                }
            );
            await context.SaveChangesAsync();
            Console.WriteLine("База данных успешно заполнена!");
        }

        var legacySeedImages = new[]
        {
            (Sku: "TSH-001", OldUrl: "/images/tshirt.jpg", NewUrl: "/uploads/T-shirt/T-shirt1.png"),
            (Sku: "HOO-001", OldUrl: "/images/hoodie.jpg", NewUrl: "/uploads/placeholder.jpg"),
            (Sku: "CAP-001", OldUrl: "/images/cap.jpg", NewUrl: "/uploads/placeholder.jpg")
        };
        foreach (var imageFix in legacySeedImages)
        {
            var product = await context.Products.FirstOrDefaultAsync(p => p.SKU == imageFix.Sku);
            if (product?.ImageUrls?.Contains(imageFix.OldUrl) == true)
            {
                product.ImageUrls = product.ImageUrls
                    .Select(url => url == imageFix.OldUrl ? imageFix.NewUrl : url)
                    .ToList();
            }
        }
        await context.SaveChangesAsync();

        await DbSeeder.SeedPermissionsAsync(context);
        await DbSeeder.SeedCategoriesAsync(context);
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

app.UseRateLimiter();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();