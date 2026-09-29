using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mish.Api.Data;
using Mish.Api.Services;
using Moq;

namespace Mish.Api.Tests.Services;

internal sealed class ProductServiceTestFixture : IDisposable
{
    private readonly string _contentRootPath;

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

    public AppDbContext Context { get; }

    public ProductService Service { get; }

    public void Dispose()
    {
        Context.Dispose();

        if (Directory.Exists(_contentRootPath))
        {
            Directory.Delete(_contentRootPath, recursive: true);
        }
    }
}
