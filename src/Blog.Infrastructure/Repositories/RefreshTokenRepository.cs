using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

// Manages refresh tokens for JWT authentication with token rotation.
// On each token refresh, the old token is revoked (IsRevoked = true) and a new one is created.
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    // Searches for a refresh token by its string value.
    // Used during the /refresh endpoint to validate the token before issuing a new pair.
    // FirstOrDefaultAsync — returns null if the token doesn't exist (expired/revoked tokens stay in DB).
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);
    }

    // Saves a newly generated refresh token to the database.
    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    // Updates an existing refresh token (e.g., setting IsRevoked = true during rotation).
    // Update() marks the entity as Modified — SaveChangesAsync generates an UPDATE for all columns.
    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }
}
