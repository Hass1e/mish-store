namespace Mish.Api.Configuration;

public sealed class AuthCookieSettings
{
    public bool Secure { get; set; } = true;
    public SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;
    public int ExpirationMinutes { get; set; } = 4320;
}
