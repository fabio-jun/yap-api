using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface IRepostRepository
{
    // Get a repost by user ID and post ID
    Task<Repost?> GetAsync(int userId, int postId);
    // Return all reposts
    Task<List<Repost>> GetAllAsync();
    // Return a list of all reposts by a specific user
    Task<List<Repost>> GetByUserIdAsync(int userId);
    // Return the reposts that should appear for an user in their feed
    Task<List<Repost>> GetFeedAsync(int userId);
    // Return the number of reposts for a specific post
    Task<int> GetCountByPostIdAsync(int postId);
    // Optimized method to get repost counts for multiple post IDs at once
    // Avoids making one query per post
    Task<Dictionary<int, int>> GetCountsByPostIdsAsync(IEnumerable<int> postIds);
    // Return the IDs of posts that a user has reposted from a given list of post IDs
    Task<HashSet<int>> GetRepostedPostIdsAsync(int userId, IEnumerable<int> postIds);
    
    Task AddAsync(Repost repost);
    Task UpdateAsync(Repost repost);
    Task DeleteAsync(Repost repost);
}
