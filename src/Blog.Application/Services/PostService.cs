using System.Text.RegularExpressions;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.DTOs;
using Blog.Application.DTOs.Mentions;
using Blog.Application.DTOs.Posts;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly IRepostRepository _repostRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IBlockRepository _blockRepository;

    public PostService(
        IPostRepository postRepository,
        ITagRepository tagRepository,
        ILikeRepository likeRepository,
        IBookmarkRepository bookmarkRepository,
        IRepostRepository repostRepository,
        IUserRepository userRepository,
        INotificationService notificationService,
        IBlockRepository blockRepository)
    {
        _postRepository = postRepository;
        _tagRepository = tagRepository;
        _likeRepository = likeRepository;
        _bookmarkRepository = bookmarkRepository;
        _repostRepository = repostRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _blockRepository = blockRepository;
    }

    public async Task<IEnumerable<PostResponse>> GetAllAsync(int? currentUserId = null)
    {
        var posts = await _postRepository.GetAllAsync();
        var reposts = await _repostRepository.GetAllAsync() ?? [];
        return await BuildTimeline(
            await FilterBlockedPosts(posts, currentUserId),
            await FilterBlockedReposts(reposts, currentUserId),
            currentUserId);
    }

    public async Task<IEnumerable<PostResponse>> GetByUserIdAsync(int userId, int? currentUserId = null)
    {
        var posts = await _postRepository.GetByUserIdAsync(userId);
        var reposts = await _repostRepository.GetByUserIdAsync(userId) ?? [];
        return await BuildTimeline(
            await FilterBlockedPosts(posts, currentUserId),
            await FilterBlockedReposts(reposts, currentUserId),
            currentUserId);
    }

    public async Task<PagedResponse<PostResponse>> GetAllPagedAsync(int page, int pageSize, int? currentUserId = null)
    {
        var posts = await _postRepository.GetAllAsync();
        var reposts = await _repostRepository.GetAllAsync() ?? [];
        var all = (await BuildTimeline(
            await FilterBlockedPosts(posts, currentUserId),
            await FilterBlockedReposts(reposts, currentUserId),
            currentUserId)).ToList();

        return new PagedResponse<PostResponse>
        {
            Items = all.Skip((page - 1) * pageSize).Take(pageSize),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        };
    }

    public async Task<IEnumerable<PostResponse>> SearchAsync(string query, int? currentUserId = null)
    {
        var posts = await _postRepository.SearchAsync(query);
        return await MapPosts(await FilterBlockedPosts(posts, currentUserId), currentUserId);
    }

    public async Task<IEnumerable<PostResponse>> GetByTagAsync(string tagName, int? currentUserId = null)
    {
        var posts = await _postRepository.GetByTagAsync(tagName);
        return await MapPosts(await FilterBlockedPosts(posts, currentUserId), currentUserId);
    }

    public async Task<IEnumerable<PostResponse>> GetFeedAsync(int userId)
    {
        var posts = await _postRepository.GetFeedAsync(userId);
        var reposts = await _repostRepository.GetFeedAsync(userId) ?? [];
        return await BuildTimeline(
            await FilterBlockedPosts(posts, userId),
            await FilterBlockedReposts(reposts, userId),
            userId);
    }

    public async Task<PostResponse> GetByIdAsync(int id, int? currentUserId = null)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
            throw new KeyNotFoundException("Post not found.");

        return await MapPost(post, currentUserId);
    }

    public async Task<PostResponse> CreateAsync(CreatePostRequest request, int authorId)
    {
        if (request.Content.Length > 280)
            throw new ArgumentException("Post must be 280 characters or less.");

        var post = new Post
        {
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            AuthorId = authorId,
            ImageUrl = request.ImageUrl
        };

        await _postRepository.AddAsync(post);
        await ExtractAndSaveTags(post, request.Content);
        var mentionedUsers = await NotifyMentions(request.Content, authorId, post.Id);

        return new PostResponse
        {
            Id = post.Id,
            OriginalPostId = post.Id,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            AuthorId = post.AuthorId,
            AuthorName = string.Empty,
            ImageUrl = post.ImageUrl,
            MentionedUsers = mentionedUsers
        };
    }

    public async Task<PostResponse> UpdateAsync(int postId, UpdatePostRequest request, int userId, string userRole)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new KeyNotFoundException("Post not found.");

        if (post.AuthorId != userId && userRole != "Admin")
            throw new UnauthorizedAccessException("Not authorized.");

        if (request.Content.Length > 280)
            throw new ArgumentException("Post must be 280 characters or less.");

        post.Content = request.Content;
        post.ImageUrl = request.ImageUrl;
        post.UpdatedAt = DateTime.UtcNow;

        post.PostTags?.Clear();
        await _postRepository.UpdateAsync(post);
        await ExtractAndSaveTags(post, request.Content);

        return await MapPost(post, userId);
    }

    public async Task DeleteAsync(int postId, int userId, string userRole)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new KeyNotFoundException("Post not found.");

        if (post.AuthorId != userId && userRole != "Admin")
            throw new UnauthorizedAccessException("Not authorized.");

        await _postRepository.DeleteAsync(post);
    }

    private async Task<List<PostResponse>> BuildTimeline(
        IEnumerable<Post> posts,
        IEnumerable<Repost> reposts,
        int? currentUserId)
    {
        var responses = new List<PostResponse>();

        foreach (var post in posts)
        {
            responses.Add(await MapPost(post, currentUserId));
        }

        foreach (var repost in reposts.Where(r => r.Post != null))
        {
            responses.Add(await MapPost(repost.Post!, currentUserId, repost));
        }

        return responses
            .OrderByDescending(p => p.RepostedAt ?? p.CreatedAt)
            .ToList();
    }

    private async Task<List<PostResponse>> MapPosts(IEnumerable<Post> posts, int? currentUserId)
    {
        var responses = new List<PostResponse>();
        foreach (var post in posts)
        {
            responses.Add(await MapPost(post, currentUserId));
        }

        return responses;
    }

    private async Task<IEnumerable<Post>> FilterBlockedPosts(IEnumerable<Post> posts, int? currentUserId)
    {
        if (!currentUserId.HasValue) return posts;

        var blockedIds = await _blockRepository.GetBlockedUserIdsForViewerAsync(currentUserId.Value) ?? [];
        return posts.Where(p => !blockedIds.Contains(p.AuthorId));
    }

    private async Task<IEnumerable<Repost>> FilterBlockedReposts(IEnumerable<Repost> reposts, int? currentUserId)
    {
        if (!currentUserId.HasValue) return reposts;

        var blockedIds = await _blockRepository.GetBlockedUserIdsForViewerAsync(currentUserId.Value) ?? [];
        return reposts.Where(r =>
            !blockedIds.Contains(r.UserId) &&
            r.Post != null &&
            !blockedIds.Contains(r.Post.AuthorId));
    }

    private async Task<PostResponse> MapPost(Post post, int? currentUserId, Repost? repost = null)
    {
        return new PostResponse
        {
            Id = post.Id,
            OriginalPostId = post.Id,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            AuthorId = post.AuthorId,
            AuthorName = post.Author?.UserName ?? string.Empty,
            AuthorProfileImageUrl = post.Author?.ProfileImageUrl,
            ImageUrl = post.ImageUrl,
            LikeCount = await _likeRepository.GetCountByPostIdAsync(post.Id),
            HasLiked = currentUserId.HasValue
                && await _likeRepository.GetAsync(post.Id, currentUserId.Value) != null,
            HasBookmarked = currentUserId.HasValue
                && await _bookmarkRepository.GetAsync(post.Id, currentUserId.Value) != null,
            RepostCount = await _repostRepository.GetCountByPostIdAsync(post.Id),
            HasReposted = currentUserId.HasValue
                && await _repostRepository.GetAsync(currentUserId.Value, post.Id) != null,
            IsRepost = repost != null,
            RepostId = repost?.Id,
            RepostedByUserId = repost?.UserId,
            RepostedByUserName = repost?.User?.UserName,
            RepostedByProfileImageUrl = repost?.User?.ProfileImageUrl,
            RepostedAt = repost?.CreatedAt,
            QuoteContent = repost?.QuoteContent,
            MentionedUsers = await ResolveMentions(post.Content)
        };
    }

    private async Task<List<MentionedUserResponse>> NotifyMentions(string content, int actorId, int postId)
    {
        var mentionedUsers = await ResolveMentions(content);
        foreach (var user in mentionedUsers.Where(u => u.UserId != actorId))
        {
            await _notificationService.CreateNotificationAsync(
                NotificationType.Mention, actorId, user.UserId, postId);
        }

        return mentionedUsers;
    }

    private async Task<List<MentionedUserResponse>> ResolveMentions(string content)
    {
        var usernames = Regex.Matches(content, @"@([A-Za-z0-9_]+)")
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var users = new List<MentionedUserResponse>();
        foreach (var username in usernames)
        {
            var user = await _userRepository.GetByUserNameAsync(username);
            if (user != null)
            {
                users.Add(new MentionedUserResponse { UserId = user.Id, UserName = user.UserName });
            }
        }

        return users;
    }

    private async Task ExtractAndSaveTags(Post post, string content)
    {
        var hashtags = Regex.Matches(content, @"#(\w+)")
            .Select(m => m.Groups[1].Value.ToLower())
            .Distinct()
            .ToList();

        foreach (var tagName in hashtags)
        {
            var tag = await _tagRepository.GetByNameAsync(tagName);

            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                await _tagRepository.AddAsync(tag);
            }

            post.PostTags ??= new List<PostTag>();
            post.PostTags.Add(new PostTag { PostId = post.Id, TagId = tag.Id });
        }

        await _postRepository.UpdateAsync(post);
    }
}
