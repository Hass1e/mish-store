using Mish.Api.Services; 
using Microsoft.EntityFrameworkCore; 
using Mish.Api.Data;               
using System.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Mish.Api.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Mish.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    var envFilePath = Path.GetFullPath(
        Path.Combine(builder.Environment.ContentRootPath, "..", ".env"));
    if (File.Exists(envFilePath))
    {
        DotNetEnv.Env.Load(envFilePath);
        builder.Configuration.AddEnvironmentVariables();
    }
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // ← Без этого Swagger не заработает!
builder.Services.AddOptions<AuthCookieSettings>()
    .Bind(builder.Configuration.GetSection("AuthCookie"))
    .Validate(settings => settings.ExpirationMinutes > 0, "AuthCookie:ExpirationMinutes должен быть больше нуля.")
    .ValidateOnStart();
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

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtSecret = jwtSettings["SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    if (!builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "JwtSettings:SecretKey must be configured outside Development.");
    }

    jwtSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["JwtSettings:SecretKey"] = jwtSecret
    });
    Console.WriteLine("JwtSettings:SecretKey is empty; using a temporary development-only key.");
}

var secretKey = Encoding.UTF8.GetBytes(jwtSecret);
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
        limiterOptions.PermitLimit = 40; // Количество запросов
        limiterOptions.Window = TimeSpan.FromMinutes(1); // Временной промежуток
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0; // Сколько запросов можно поставить в очередь, если лимит почти достигнут
    });

    // Можно добавить еще политику для авторизации (более строгую)
    options.AddSlidingWindowLimiter(policyName: "auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10; // Всего попыток
        limiterOptions.Window = TimeSpan.FromMinutes(1); 
        limiterOptions.SegmentsPerWindow = 5;
    });
});


var app = builder.Build();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Unspecified
});
app.UseCors("AllowReact");
app.UseStaticFiles();

app.UseAuthentication(); // ← Сначала аутентификация (проверка токена)
app.UseAuthorization();  // ← Потом авторизация (проверка ролей)


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        await DbSeeder.SeedProductsAsync(context);
        await DbSeeder.SeedPermissionsAsync(context);
        await DbSeeder.SeedCategoriesAsync(context);
        await DbSeeder.SeedAdminUserAsync(context, builder.Configuration["SeedAdmin:Password"]);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Ошибка при инициализации БД: {ex}");
        throw;
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