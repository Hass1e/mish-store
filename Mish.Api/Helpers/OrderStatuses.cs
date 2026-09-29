 namespace Mish.Api.Helpers;

public static class OrderStatuses
{
    public const string Pending    = "Pending";
    public const string Paid       = "Paid";
    public const string Processing = "Processing";
    public const string Shipped    = "Shipped";
    public const string Delivered  = "Delivered";
    public const string Cancelled  = "Cancelled";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Pending, Paid, Processing, Shipped, Delivered, Cancelled
    };

    public static bool IsValid(string status) =>
        All.Contains(status, StringComparer.OrdinalIgnoreCase);
}