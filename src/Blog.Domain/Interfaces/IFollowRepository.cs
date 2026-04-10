using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

// Repository interface for Follow data access.
public interface IFollowRepository
{
    // Checks if followerId is following followingId. Returns null if no relationship exists.
    Task<Follow?> GetAsync(int followerId, int followingId);

    // Returns the list of User objects that follow userId
    Task<IEnumerable<User>> GetFollowersAsync(int userId);

    // Returns the list of User objects that userId follows
    Task<IEnumerable<User>> GetFollowingAsync(int userId);

    // Count queries — used for profile stats (SELECT COUNT(*))
    Task<int> GetFollowersCountAsync(int userId);
    Task<int> GetFollowingCountAsync(int userId);

    Task AddAsync(Follow follow);
    Task DeleteAsync(Follow follow);
}
