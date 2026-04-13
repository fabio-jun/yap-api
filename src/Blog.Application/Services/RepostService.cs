using Blog.Application.DTOs.Reposts;
using Blog.Application.Interfaces;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;

namespace Blog.Application.Services;

public class RepostService : IRepostService
{
    private readonly IRepostRepository _repostRepository;
    private readonly IPostRepository _postRepository;

    public RepostService(IRepostRepository repostRepository, IPostRepository postRepository)
    {
        _repostRepository = repostRepository;
        _postRepository = postRepository;
    }

    public async Task<RepostStateResponse> ToggleAsync(int postId, int userId)
    {
        var post = await GetRepostablePost(postId, userId);
        var existing = await _repostRepository.GetAsync(userId, post.Id);

        if (existing != null)
        {
            await _repostRepository.DeleteAsync(existing);
            return await BuildState(post.Id, userId);
        }

        await _repostRepository.AddAsync(new Repost
        {
            UserId = userId,
            PostId = post.Id,
            CreatedAt = DateTime.UtcNow
        });

        return await BuildState(post.Id, userId);
    }

    public async Task<RepostStateResponse> QuoteAsync(int postId, QuoteRepostRequest request, int userId)
    {
        var content = request.Content.Trim();
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Quote re-yap content is required.");

        if (content.Length > 280)
            throw new ArgumentException("Quote re-yap must be 280 characters or less.");

        var post = await GetRepostablePost(postId, userId);
        var existing = await _repostRepository.GetAsync(userId, post.Id);

        if (existing != null)
        {
            existing.QuoteContent = content;
            existing.CreatedAt = DateTime.UtcNow;
            await _repostRepository.UpdateAsync(existing);
            return await BuildState(post.Id, userId);
        }

        await _repostRepository.AddAsync(new Repost
        {
            UserId = userId,
            PostId = post.Id,
            QuoteContent = content,
            CreatedAt = DateTime.UtcNow
        });

        return await BuildState(post.Id, userId);
    }

    public async Task<RepostStateResponse> GetStateAsync(int postId, int? userId)
    {
        var count = await _repostRepository.GetCountByPostIdAsync(postId);
        if (!userId.HasValue)
        {
            return new RepostStateResponse { Count = count };
        }

        var repost = await _repostRepository.GetAsync(userId.Value, postId);
        return new RepostStateResponse
        {
            Reposted = repost != null,
            Count = count,
            RepostId = repost?.Id,
            QuoteContent = repost?.QuoteContent
        };
    }

    private async Task<Post> GetRepostablePost(int postId, int userId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new KeyNotFoundException("Post not found.");

        if (post.AuthorId == userId)
            throw new ArgumentException("You cannot re-yap your own yap.");

        return post;
    }

    private async Task<RepostStateResponse> BuildState(int postId, int userId)
    {
        var count = await _repostRepository.GetCountByPostIdAsync(postId);
        var repost = await _repostRepository.GetAsync(userId, postId);
        return new RepostStateResponse
        {
            Reposted = repost != null,
            Count = count,
            RepostId = repost?.Id,
            QuoteContent = repost?.QuoteContent
        };
    }
}
