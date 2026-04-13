using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.Interfaces;
using Blog.Application.DTOs.Likes;

namespace Blog.Application.Services;

public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;
    private readonly IPostRepository _postRepository;
    private readonly INotificationService _notificationService;

    public LikeService(
        ILikeRepository likeRepository,
        IPostRepository postRepository,
        INotificationService notificationService)
    {
        _likeRepository = likeRepository;
        _postRepository = postRepository;
        _notificationService = notificationService;
    }

    public async Task<bool> ToggleLikeAsync(int postId, int userId)
    {
        var existingLike = await _likeRepository.GetAsync(postId, userId);

        if (existingLike != null)
        {
            await _likeRepository.DeleteAsync(existingLike);

            var post = await _postRepository.GetByIdAsync(postId);
            if (post != null)
            {
                await _notificationService.DeleteNotificationAsync(
                    NotificationType.Like, userId, post.AuthorId, postId);
            }

            return false;
        }

        var like = new Like
        {
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _likeRepository.AddAsync(like);

        var likedPost = await _postRepository.GetByIdAsync(postId);
        if (likedPost != null)
        {
            await _notificationService.CreateNotificationAsync(
                NotificationType.Like, userId, likedPost.AuthorId, postId);
        }

        return true;
    }

    public async Task<int> GetCountAsync(int postId)
    {
        return await _likeRepository.GetCountByPostIdAsync(postId);
    }

    public async Task<bool> HasLikedAsync(int postId, int userId)
    {
        var like = await _likeRepository.GetAsync(postId, userId);
        return like != null;
    }

    public async Task<List<LikedUserResponse>> GetUsersAsync(int postId)
    {
        var likes = await _likeRepository.GetByPostIdWithUsersAsync(postId);

        return likes
            .Where(l => l.User != null)
            .Select(l => new LikedUserResponse
            {
                Id = l.User!.Id,
                UserName = l.User.UserName,
                DisplayName = l.User.DisplayName,
                ProfileImageUrl = l.User.ProfileImageUrl
            })
            .ToList();
    }
}
