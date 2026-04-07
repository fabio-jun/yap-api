using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;

    public LikeService(ILikeRepository likeRepository)
    {
        _likeRepository = likeRepository;
    }

    // Toggles like on a post
    // If the user already liked it, removes the like (unlike)
    // If the user hasn't liked it, adds a new like
    // Returns true if liked, false if unliked
    public async Task<bool> ToggleLikeAsync(int postId, int userId)
    {
        var existingLike = await _likeRepository.GetAsync(postId, userId);

        if (existingLike != null)
        {
            // Already liked — remove it
            await _likeRepository.DeleteAsync(existingLike);
            return false;
        }

        // Not liked yet — add a new like
        var like = new Like
        {
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _likeRepository.AddAsync(like);
        return true;
    }

    // Returns the total number of likes for a post
    public async Task<int> GetCountAsync(int postId)
    {
        return await _likeRepository.GetCountByPostIdAsync(postId);
    }

    // Checks if a specific user has liked a post
    public async Task<bool> HasLikedAsync(int postId, int userId)
    {
        var like = await _likeRepository.GetAsync(postId, userId);
        return like != null;
    }
}
