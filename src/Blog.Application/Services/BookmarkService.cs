using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.DTOs.Posts;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

public class BookmarkService : IBookmarkService
{
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly ILikeRepository _likeRepository;

    public BookmarkService(IBookmarkRepository bookmarkRepository, ILikeRepository likeRepository)
    {
        _bookmarkRepository = bookmarkRepository;
        _likeRepository = likeRepository;
    }

    public async Task<bool> ToggleBookmarkAsync(int postId, int userId)
    {
        var existing = await _bookmarkRepository.GetAsync(postId, userId);

        if (existing != null)
        {
            await _bookmarkRepository.DeleteAsync(existing);
            return false;
        }

        await _bookmarkRepository.AddAsync(new Bookmark
        {
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        });
        return true;
    }

    public async Task<bool> HasBookmarkedAsync(int postId, int userId)
    {
        return await _bookmarkRepository.GetAsync(postId, userId) != null;
    }

    public async Task<IEnumerable<PostResponse>> GetBookmarksAsync(int userId)
    {
        var posts = await _bookmarkRepository.GetByUserAsync(userId);
        var responses = new List<PostResponse>();

        foreach (var post in posts)
        {
            responses.Add(new PostResponse
            {
                Id = post.Id,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                AuthorId = post.AuthorId,
                AuthorName = post.Author?.UserName ?? string.Empty,
                AuthorProfileImageUrl = post.Author?.ProfileImageUrl,
                ImageUrl = post.ImageUrl,
                LikeCount = await _likeRepository.GetCountByPostIdAsync(post.Id),
                HasLiked = await _likeRepository.GetAsync(post.Id, userId) != null
            });
        }

        return responses;
    }
}
