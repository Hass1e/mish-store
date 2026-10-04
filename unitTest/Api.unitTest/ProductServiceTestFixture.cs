using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mish.Api.Data;
using Mish.Api.Services;
using Moq;

namespace Mish.Api.Tests.Services;

/// <summary>
/// Общая тестовая среда для изолированных тестов ProductService.
/// Создаёт отдельную In-Memory базу данных и временную папку загрузок для каждого теста.
/// </summary>
internal sealed class ProductServiceTestFixture : IDisposable
{
    /// <summary>
    /// Путь к временной корневой папке, созданной для этого экземпляра fixture.
    /// </summary>
    private readonly string _contentRootPath;

    /// <summary>
    /// Инициализирует In-Memory базу данных, временную папку и ProductService с тестовой конфигурацией.
    /// </summary>
    public ProductServiceTestFixture()
    {
        _contentRootPath = Path.Combine(Path.GetTempPath(), "Mish.Api.UnitTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_contentRootPath, "wwwroot", "uploads"));

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);

        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(env => env.ContentRootPath).Returns(_contentRootPath);

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiSettings:BaseUrl"] = "http://localhost"
            })
            .Build();

        Service = new ProductService(Context, environment.Object, configuration);
    }

    /// <summary>
    /// Контекст In-Memory базы данных, используемый тестом для подготовки и проверки данных.
    /// </summary>
    public AppDbContext Context { get; }

    /// <summary>
    /// Экземпляр ProductService, работающий с контекстом и окружением этого fixture.
    /// </summary>
    public ProductService Service { get; }

    /// <summary>
    /// Освобождает контекст базы данных и удаляет временную папку теста.
    /// </summary>
    public void Dispose()
    {
        Context.Dispose();

        if (Directory.Exists(_contentRootPath))
        {
            Directory.Delete(_contentRootPath, recursive: true);
        }
    }
}
