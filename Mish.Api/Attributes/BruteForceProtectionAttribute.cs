using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Memory;

namespace Mish.Api.Attributes;

public sealed class BruteForceProtectionAttribute : ActionFilterAttribute
{
    private readonly IMemoryCache _cache;

    private const int MaxPerMinute = 5;
    private const int MaxPerHour = 10;
    private const int MaxPerDay = 20;

    public BruteForceProtectionAttribute(IMemoryCache cache)
    {
        _cache = cache;
    }

    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var keyPrefix = $"bruteforce_login_{context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

        if (IsLimitExceeded(keyPrefix))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status429TooManyRequests);
            return;
        }

        var executedContext = await next();
        if (executedContext.Result is IStatusCodeActionResult { StatusCode: StatusCodes.Status401Unauthorized })
        {
            IncrementCounter($"{keyPrefix}_min", TimeSpan.FromMinutes(1));
            IncrementCounter($"{keyPrefix}_hour", TimeSpan.FromHours(1));
            IncrementCounter($"{keyPrefix}_day", TimeSpan.FromDays(1));
        }
    }

    private bool IsLimitExceeded(string keyPrefix)
    {
        _cache.TryGetValue($"{keyPrefix}_min", out int minuteCount);
        _cache.TryGetValue($"{keyPrefix}_hour", out int hourCount);
        _cache.TryGetValue($"{keyPrefix}_day", out int dayCount);

        return minuteCount >= MaxPerMinute
            || hourCount >= MaxPerHour
            || dayCount >= MaxPerDay;
    }

    private void IncrementCounter(string key, TimeSpan expiration)
    {
        _cache.TryGetValue(key, out int currentCount);
        _cache.Set(key, currentCount + 1, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        });
    }
}
