using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?>GetByTokenAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
}