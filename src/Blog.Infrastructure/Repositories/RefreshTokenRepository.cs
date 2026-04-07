using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class RefreshTokenRepository: IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    // Searches for a refresh token in the database that matches the provided token
    public async Task<RefreshToken?>GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);
    }
    
    // Saves the new refresh token in the DB and persists with SaveChangesAsync()
    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

}

