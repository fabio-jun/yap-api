using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

// Repository interface for RefreshToken data access.
// Used by AuthService to manage token lifecycle (create, find, revoke).
public interface IRefreshTokenRepository
{
    // Finds a refresh token by its string value (the GUID sent by the client)
    Task<RefreshToken?> GetByTokenAsync(string token);

    // Saves a newly generated refresh token
    Task AddAsync(RefreshToken refreshToken);

    // Marks a token as revoked (IsRevoked = true) to prevent reuse
    Task UpdateAsync(RefreshToken refreshToken);
}
