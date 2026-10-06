using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Mish.Api.Attributes;
using System.Net;

namespace Mish.Api.Tests.Attributes;

/// <summary>
/// Тесты защиты от брутфорса (Brute Force Protection).
/// Проверяет механизм ограничения попыток входа по IP-адресу.
/// Лимиты: 5 попыток/минута, 10/час, 20/день.
/// </summary>
public class BruteForceProtectionTests
{
    private const string ClientIp = "127.0.0.1";
    private const string CounterKeyPrefix = $"bruteforce_login_{ClientIp}";

    /// <summary>
    /// Тест 1: Успешный вход НЕ увеличивает счётчики неудачных попыток.
    /// </summary>
    [Fact]
    public async Task Successful_Login_Does_Not_Increment_Counters()
    {
        using var cache = CreateCache();
        var filter = new BruteForceProtectionAttribute(cache);
        var context = CreateContext();
        var nextCallCount = 0;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCallCount++;
            return Task.FromResult(CreateExecutedContext(context, new OkResult()));
        });

        Assert.Equal(1, nextCallCount);
        Assert.Null(context.Result);
        AssertCounterDoesNotExist(cache, "_min");
        AssertCounterDoesNotExist(cache, "_hour");
        AssertCounterDoesNotExist(cache, "_day");
    }

    /// <summary>
    /// Тест 2: Неудачный вход увеличивает все три счётчика (_min, _hour, _day).
    /// </summary>
    [Fact]
    public async Task Failed_Login_Increments_All_Counters()
    {
        using var cache = CreateCache();
        var filter = new BruteForceProtectionAttribute(cache);
        var context = CreateContext();

        await filter.OnActionExecutionAsync(
            context,
            () => Task.FromResult(CreateExecutedContext(
                context,
                new UnauthorizedObjectResult(new { message = "Invalid credentials" }))));

        AssertCounterEquals(cache, "_min", 1);
        AssertCounterEquals(cache, "_hour", 1);
        AssertCounterEquals(cache, "_day", 1);
    }

    /// <summary>
    /// Тест 3: Ответы с кодами 400/500 (не 401) не увеличивают счётчики.
    /// Защита должна реагировать только на Unauthorized (401).
    /// </summary>
    [Theory]
    [InlineData(400)]
    [InlineData(500)]
    public async Task NonUnauthorized_Response_Does_Not_Increment_Counters(int statusCode)
    {
        using var cache = CreateCache();
        var filter = new BruteForceProtectionAttribute(cache);
        var context = CreateContext();

        await filter.OnActionExecutionAsync(
            context,
            () => Task.FromResult(CreateExecutedContext(
                context,
                new ObjectResult(null) { StatusCode = statusCode })));

        AssertCounterDoesNotExist(cache, "_min");
        AssertCounterDoesNotExist(cache, "_hour");
        AssertCounterDoesNotExist(cache, "_day");
    }

    /// <summary>
    /// Тест 4: 5 неудачных попыток разрешены, 6-я блокируется с кодом 429.
    /// Проверяет что лимиты работают корректно.
    /// </summary>
    [Fact]
    public async Task Fifth_Failed_Login_Is_Allowed_And_Next_Login_Is_Rejected()
    {
        using var cache = CreateCache();
        var filter = new BruteForceProtectionAttribute(cache);
        var nextCallCount = 0;

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var context = CreateContext();
            await filter.OnActionExecutionAsync(
                context,
                () =>
                {
                    nextCallCount++;
                    return Task.FromResult(CreateExecutedContext(
                        context,
                        new UnauthorizedObjectResult(new { message = "Invalid credentials" })));
                });

            Assert.Null(context.Result);
        }

        var blockedContext = CreateContext();
        await filter.OnActionExecutionAsync(
            blockedContext,
            () =>
            {
                nextCallCount++;
                return Task.FromResult(CreateExecutedContext(blockedContext, new OkResult()));
            });

        Assert.Equal(5, nextCallCount);
        var result = Assert.IsType<StatusCodeResult>(blockedContext.Result);
        Assert.Equal(StatusCodes.Status429TooManyRequests, result.StatusCode);
        AssertCounterEquals(cache, "_min", 5);
    }

    /// <summary>
    /// Тест 5: При достижении любого лимита (5/10/20) следующий вход блокируется.
    /// </summary>
    [Theory]
    [InlineData("_min", 5)]
    [InlineData("_hour", 10)]
    [InlineData("_day", 20)]
    public async Task Reaching_Any_Limit_Rejects_Next_Login(string window, int limit)
    {
        using var cache = CreateCache();
        cache.Set($"{CounterKeyPrefix}{window}", limit);
        var filter = new BruteForceProtectionAttribute(cache);
        var context = CreateContext();
        var nextCallCount = 0;

        await filter.OnActionExecutionAsync(
            context,
            () =>
            {
                nextCallCount++;
                return Task.FromResult(CreateExecutedContext(context, new OkResult()));
            });

        Assert.Equal(0, nextCallCount);
        var result = Assert.IsType<StatusCodeResult>(context.Result);
        Assert.Equal(StatusCodes.Status429TooManyRequests, result.StatusCode);
    }

    private static MemoryCache CreateCache() => new(new MemoryCacheOptions());

    private static ActionExecutingContext CreateContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(ClientIp);
        var actionContext = new ActionContext(
            httpContext,
            new Microsoft.AspNetCore.Routing.RouteData(),
            new ActionDescriptor());

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());
    }

    private static ActionExecutedContext CreateExecutedContext(
        ActionExecutingContext context,
        IActionResult result)
    {
        return new ActionExecutedContext(
            context,
            new List<IFilterMetadata>(),
            new object())
        {
            Result = result
        };
    }

    private static void AssertCounterEquals(MemoryCache cache, string window, int expected)
    {
        Assert.True(cache.TryGetValue($"{CounterKeyPrefix}{window}", out int actual));
        Assert.Equal(expected, actual);
    }

    private static void AssertCounterDoesNotExist(MemoryCache cache, string window)
    {
        Assert.False(cache.TryGetValue($"{CounterKeyPrefix}{window}", out _));
    }
}
