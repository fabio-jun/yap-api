using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

// Service that handles like toggle logic.
// Uses the toggle pattern: a single endpoint handles both like and unlike.
public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;

    public LikeService(ILikeRepository likeRepository)
    {
        _likeRepository = likeRepository;
    }

    // Toggles like on a post.
    // If the user already liked it → removes the like (returns false).
    // If the user hasn't liked it → adds a new like (returns true).
    public async Task<bool> ToggleLikeAsync(int postId, int userId)
    {
        // Check if a Like record already exists for this user + post combination
        var existingLike = await _likeRepository.GetAsync(postId, userId);

        if (existingLike != null)
        {
            // Already liked — remove it (DELETE FROM Likes WHERE PostId = x AND UserId = y)
            await _likeRepository.DeleteAsync(existingLike);
            return false; // "unliked"
        }

        // Not liked yet — create a new Like record
        var like = new Like
        {
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        // INSERT INTO Likes (...)
        await _likeRepository.AddAsync(like);
        return true; // "liked"
    }

    // Returns the total number of likes for a post (SELECT COUNT(*) FROM Likes WHERE PostId = x)
    public async Task<int> GetCountAsync(int postId)
    {
        return await _likeRepository.GetCountByPostIdAsync(postId);
    }

    // Checks if a specific user has liked a post
    public async Task<bool> HasLikedAsync(int postId, int userId)
    {
        var like = await _likeRepository.GetAsync(postId, userId);
        // Returns true if a Like record exists, false if null
        return like != null;
    }
}
