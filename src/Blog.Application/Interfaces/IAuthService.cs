using Blog.Application.DTOs.Auth;

namespace Blog.Application.Interfaces;

// Service interface for authentication operations.
// Defines the contract — the actual implementation (AuthService) handles JWT generation,
// password hashing, and token management.
public interface IAuthService
{
    // Creates a new user account and returns JWT + refresh token
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    // Validates credentials and returns JWT + refresh token
    Task<AuthResponse> LoginAsync(LoginRequest request);

    // Exchanges a valid refresh token for a new JWT + refresh token pair
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
}
