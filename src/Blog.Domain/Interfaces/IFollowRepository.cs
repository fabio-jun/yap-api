using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface IFollowRepository
{
    Task<Follow?> GetAsync(int followerId, int followingId);
    Task<IEnumerable<User>> GetFollowersAsync(int userId);
    Task<IEnumerable<User>> GetFollowingAsync(int userId);
    Task<int> GetFollowersCountAsync(int userId);
    Task<int> GetFollowingCountAsync(int userId);
    Task AddAsync(Follow follow);
    Task DeleteAsync(Follow follow);
}
