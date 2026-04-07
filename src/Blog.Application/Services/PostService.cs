using System.Text.RegularExpressions;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.DTOs;
using Blog.Application.DTOs.Posts;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

public class PostService : IPostService
{
    // External dependencies used for dependency injection
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly IBookmarkRepository _bookmarkRepository;

    // Constructor: DI
    public PostService(IPostRepository postRepository, ITagRepository tagRepository,
        ILikeRepository likeRepository, IBookmarkRepository bookmarkRepository)
    {
        _postRepository = postRepository;
        _tagRepository = tagRepository;
        _likeRepository = likeRepository;
        _bookmarkRepository = bookmarkRepository;
    }

    // Returns all posts (most recent first), with like info for the current user
    public async Task<IEnumerable<PostResponse>> GetAllAsync(int? currentUserId = null)
    {
        var posts = await _postRepository.GetAllAsync();
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
                HasLiked = currentUserId.HasValue
                    && await _likeRepository.GetAsync(post.Id, currentUserId.Value) != null,
                HasBookmarked = currentUserId.HasValue
                    && await _bookmarkRepository.GetAsync(post.Id, currentUserId.Value) != null
            });
        }

        return responses;
    }

    // Returns paginated posts
    public async Task<PagedResponse<PostResponse>> GetAllPagedAsync(int page, int pageSize, int? currentUserId = null)
    {
        var (posts, totalCount) = await _postRepository.GetAllPagedAsync(page, pageSize);
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
                HasLiked = currentUserId.HasValue
                    && await _likeRepository.GetAsync(post.Id, currentUserId.Value) != null,
                HasBookmarked = currentUserId.HasValue
                    && await _bookmarkRepository.GetAsync(post.Id, currentUserId.Value) != null
            });
        }

        return new PagedResponse<PostResponse>
        {
            Items = responses,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    // Searches posts by content
    public async Task<IEnumerable<PostResponse>> SearchAsync(string query, int? currentUserId = null)
    {
        var posts = await _postRepository.SearchAsync(query);
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
                HasLiked = currentUserId.HasValue
                    && await _likeRepository.GetAsync(post.Id, currentUserId.Value) != null,
                HasBookmarked = currentUserId.HasValue
                    && await _bookmarkRepository.GetAsync(post.Id, currentUserId.Value) != null
            });
        }

        return responses;
    }

    // Returns posts filtered by tag
    public async Task<IEnumerable<PostResponse>> GetByTagAsync(string tagName, int? currentUserId = null)
    {
        var posts = await _postRepository.GetByTagAsync(tagName);
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
                HasLiked = currentUserId.HasValue
                    && await _likeRepository.GetAsync(post.Id, currentUserId.Value) != null,
                HasBookmarked = currentUserId.HasValue
                    && await _bookmarkRepository.GetAsync(post.Id, currentUserId.Value) != null
            });
        }

        return responses;
    }

    // Returns posts from users the authenticated user follows
    public async Task<IEnumerable<PostResponse>> GetFeedAsync(int userId)
    {
        var posts = await _postRepository.GetFeedAsync(userId);
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
                HasLiked = await _likeRepository.GetAsync(post.Id, userId) != null,
                HasBookmarked = await _bookmarkRepository.GetAsync(post.Id, userId) != null
            });
        }

        return responses;
    }

    // Returns a single post by ID, with like info for the current user
    public async Task<PostResponse> GetByIdAsync(int id, int? currentUserId = null)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
            throw new KeyNotFoundException("Post not found.");

        return new PostResponse
        {
            Id = post.Id,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            AuthorId = post.AuthorId,
            AuthorName = post.Author?.UserName ?? string.Empty,
            ImageUrl = post.ImageUrl,
            LikeCount = await _likeRepository.GetCountByPostIdAsync(post.Id),
            HasLiked = currentUserId.HasValue
                && await _likeRepository.GetAsync(post.Id, currentUserId.Value) != null
        };
    }

    // Creates a new post (max 280 characters)
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

        // Extract hashtags from content and associate them with the post
        await ExtractAndSaveTags(post, request.Content);

        return new PostResponse
        {
            Id = post.Id,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            AuthorId = post.AuthorId,
            AuthorName = string.Empty,
            ImageUrl = post.ImageUrl
        };
    }

    // Updates a post (author or admin only)
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

        // Clear old tag associations and extract new ones
        post.PostTags?.Clear();
        await _postRepository.UpdateAsync(post);
        await ExtractAndSaveTags(post, request.Content);

        return new PostResponse
        {
            Id = post.Id,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            AuthorId = post.AuthorId,
            AuthorName = post.Author?.UserName ?? string.Empty,
            ImageUrl = post.ImageUrl
        };
    }

    // Deletes a post (author or admin only)
    public async Task DeleteAsync(int postId, int userId, string userRole)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new KeyNotFoundException("Post not found.");

        if (post.AuthorId != userId && userRole != "Admin")
            throw new UnauthorizedAccessException("Not authorized.");

        await _postRepository.DeleteAsync(post);
    }

    // Extracts hashtags from text and saves them as Tag + PostTag in the DB
    private async Task ExtractAndSaveTags(Post post, string content)
    {
        var hashtags = Regex.Matches(content, @"#(\w+)")
            .Select(m => m.Groups[1].Value.ToLower())
            .Distinct()
            .ToList();

        foreach (var tagName in hashtags)
        {
            // Check if the tag already exists in the DB
            var tag = await _tagRepository.GetByNameAsync(tagName);

            // If not, create a new one
            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                await _tagRepository.AddAsync(tag);
            }

            // Create the association between the post and the tag
            post.PostTags ??= new List<PostTag>();
            post.PostTags.Add(new PostTag { PostId = post.Id, TagId = tag.Id });
        }

        // Save the associations to the database
        await _postRepository.UpdateAsync(post);
    }
}
