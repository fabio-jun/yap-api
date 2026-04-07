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

public class AuthService : IAuthService
{
    //External dependencies used for dependency Injection
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;

    //Constructor: DI
    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository; // Reference to the object that the DI creates
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
    }

    //Receives a DTO and returns an AuthResponse(tokens)
    //RegisterRequest is the DTO
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ArgumentException("Email already registered.");

        var existingUserName = await _userRepository.GetByUserNameAsync(request.UserName);
        if (existingUserName != null)
            throw new ArgumentException("Username already taken.");

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User"
        };

        await _userRepository.AddAsync(user);

        // Generate tokens
        return await GenerateAuthResponse(user);
    }

    //Find user by email
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new ArgumentException("Email ou senha inválidos.");
        //Generates tokens
        return await GenerateAuthResponse(user);
    }


    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            throw new ArgumentException("Invalid refresh token.");

        // Revokes old token
        token.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(token);

        // Search for the user and generates new tokens
        var user = await _userRepository.GetByIdAsync(token.UserId);
        return await GenerateAuthResponse(user!);
    }

    private async Task<AuthResponse> GenerateAuthResponse(User user)
    {
        // Generates JWT Token
        var accessToken = GenerateJwtToken(user); 
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(), // Random unique string
            ExpiresAt = expiresAt,
            UserId = user.Id
        // Instruction so ; ends it
        };
        

        // Saves refresh token at the DB
        await _refreshTokenRepository.AddAsync(refreshToken);

        // Returns the DTO with two tokens
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = expiresAt
        };
    }

    private string GenerateJwtToken(User user)
    {
        // Gets the secret key from appsettings.json
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        // Claims: information inside the token
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        // Serializes to string
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}