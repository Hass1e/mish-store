// DTOs/LoginRequest.cs
public record LoginRequest(string Username, string Password);

// DTOs/AuthResponse.cs  
public record AuthResponse(
    int UserId,
    string Username,
    string Role,
    string AccessToken,
    DateTime ExpiresAt
);