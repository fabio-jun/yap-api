using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.DTOs.Auth;
using Blog.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Blog.Application.Services;

// Service that handles authentication: register, login, and token refresh.
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    // IConfiguration gives access to appsettings.json values
    private readonly IConfiguration _configuration;

    // Constructor — Dependency Injection (DI).
    // ASP.NET Core's DI container creates instances of the dependencies and passes them here.
    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
    }

    // Registers a new user: validates uniqueness, hashes password, generates tokens.
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if email is already registered
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ArgumentException("Email already registered.");

        // Check if username is taken
        var existingUserName = await _userRepository.GetByUserNameAsync(request.UserName);
        if (existingUserName != null)
            throw new ArgumentException("Username already taken.");

        // Create the User entity 
        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            // BCrypt.HashPassword() generates a salted hash
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User"
        };

        await _userRepository.AddAsync(user);

        // Generate JWT + refresh token and return them
        return await GenerateAuthResponse(user);
    }

    // Validates credentials and returns tokens.
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        // BCrypt.Verify compares plain password against the stored hash
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new ArgumentException("Invalid e-mail or password.");

        return await GenerateAuthResponse(user);
    }

    // Exchanges a valid refresh token for new tokens (token rotation for security).
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        // Validate: token must exist, not be revoked, and not be expired
        if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            throw new ArgumentException("Invalid refresh token.");

        // Revoke the old token to prevent reuse (token rotation)
        token.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(token);

        // Look up the user and generate fresh tokens
        var user = await _userRepository.GetByIdAsync(token.UserId);
        return await GenerateAuthResponse(user!);
    }

    // Private helper — generates both JWT access token and refresh token.
    private async Task<AuthResponse> GenerateAuthResponse(User user)
    {
        // Generate the short-lived JWT (1 hour)
        var accessToken = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Create a long-lived refresh token (7 days)
        var refreshToken = new RefreshToken
        {
            // Guid.NewGuid() — generates a globally unique random string
            Token = Guid.NewGuid().ToString(),
            ExpiresAt = expiresAt,
            UserId = user.Id
        };

        // Persist refresh token in the database
        await _refreshTokenRepository.AddAsync(refreshToken);

        // Return the DTO with both tokens
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = expiresAt
        };
    }

    // Generates a JWT (JSON Web Token) containing user claims.
    // The token is signed with HMAC-SHA256 using the secret key from appsettings.json.
    private string GenerateJwtToken(User user)
    {
        // Read the secret key from configuration and convert to bytes
        // _configuration["Jwt:Key"] reads the value from appsettings.json
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        // Claims are key-value pairs embedded inside the JWT payload.
        // The frontend and backend can read these without a database query.
        // ClaimTypes.NameIdentifier -> standard claim for user ID
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        // Create the token with issuer, audience, claims, expiration, and signing credentials
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],         // Who issued the token
            audience: _configuration["Jwt:Audience"],     // Who the token is for
            claims: claims,                               // User info embedded in the token
            expires: DateTime.UtcNow.AddHours(1),         // Token valid for 1 hour
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        // Serialize the token object to a compact JWT string (header.payload.signature)
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
