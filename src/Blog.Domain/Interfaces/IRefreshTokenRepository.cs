using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    // Finds a refresh token by its string value 
    Task<RefreshToken?> GetByTokenAsync(string token);

    // Saves a newly generated refresh token
    Task AddAsync(RefreshToken refreshToken);

    // Marks a token as revoked to prevent reuse
    Task UpdateAsync(RefreshToken refreshToken);
}
