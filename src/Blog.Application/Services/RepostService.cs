using Blog.Application.DTOs.Reposts;
using Blog.Application.Interfaces;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;

namespace Blog.Application.Services;

// Service that handles simple re-yaps, quote re-yaps, and per-user repost state.
public class RepostService : IRepostService
{
    private readonly IRepostRepository _repostRepository;
    private readonly IPostRepository _postRepository;

    // Repositories are injected by ASP.NET Core's DI container.
    public RepostService(IRepostRepository repostRepository, IPostRepository postRepository)
    {
        _repostRepository = repostRepository;
        _postRepository = postRepository;
    }

    // Toggles a simple re-yap. Returns the updated count and viewer state.
    public async Task<RepostStateResponse> ToggleAsync(int postId, int userId)
    {
        var post = await GetRepostablePost(postId, userId);
        var existing = await _repostRepository.GetAsync(userId, post.Id);

        if (existing != null)
        {
            // Re-yapping an already re-yapped post removes the repost.
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

    // Creates or updates a quote re-yap with the authenticated user's comment.
    public async Task<RepostStateResponse> QuoteAsync(int postId, QuoteRepostRequest request, int userId)
    {
        var content = request.Content.Trim();
        // Quote content follows the same 280-character limit as regular yaps.
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Quote re-yap content is required.");

        if (content.Length > 280)
            throw new ArgumentException("Quote re-yap must be 280 characters or less.");

        var post = await GetRepostablePost(postId, userId);
        var existing = await _repostRepository.GetAsync(userId, post.Id);

        if (existing != null)
        {
            // A quote re-yap reuses the existing repost row and refreshes its timestamp.
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

    // Returns repost count plus the viewer's own repost/quote state when authenticated.
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

    // Loads the target yap and applies business rules shared by both repost flows.
    private async Task<Post> GetRepostablePost(int postId, int userId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new KeyNotFoundException("Post not found.");

        // Users cannot re-yap their own yaps.
        if (post.AuthorId == userId)
            throw new ArgumentException("You cannot re-yap your own yap.");

        return post;
    }

    // Builds the compact response consumed by the repost button/menu.
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
