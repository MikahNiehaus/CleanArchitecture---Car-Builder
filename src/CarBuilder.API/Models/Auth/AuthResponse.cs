namespace CarBuilder.API.Models.Auth;

public record AuthResponse(
    string Token,
    string Email,
    string? FirstName,
    string? LastName
);
