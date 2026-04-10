using System.Text.RegularExpressions;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.DTOs;
using Blog.Application.DTOs.Posts;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

// Service that handles all post (yap) business logic.
// Orchestrates multiple repositories and maps entities to DTOs.
public class PostService : IPostService
{
    // Dependencies injected via constructor — each repository handles one entity's data access
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly IBookmarkRepository _bookmarkRepository;

    public PostService(IPostRepository postRepository, ITagRepository tagRepository,
        ILikeRepository likeRepository, IBookmarkRepository bookmarkRepository)
    {
        _postRepository = postRepository;
        _tagRepository = tagRepository;
        _likeRepository = likeRepository;
        _bookmarkRepository = bookmarkRepository;
    }

    // Returns all posts (most recent first), enriched with like/bookmark info for the current user.
    // 'int? currentUserId = null' — optional parameter with default value.
    // If the user is not authenticated, currentUserId is null and HasLiked/HasBookmarked will be false.
    public async Task<IEnumerable<PostResponse>> GetAllAsync(int? currentUserId = null)
    {
        var posts = await _postRepository.GetAllAsync();
        var responses = new List<PostResponse>();

        // Manual mapping: Entity → DTO. No AutoMapper — explicit and visible.
        foreach (var post in posts)
        {
            responses.Add(new PostResponse
            {
                Id = post.Id,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                AuthorId = post.AuthorId,
                // '?.' — null-conditional operator. If Author is null, returns null instead of throwing.
                // '?? string.Empty' — null-coalescing operator. If left side is null, use empty string.
                AuthorName = post.Author?.UserName ?? string.Empty,
                AuthorProfileImageUrl = post.Author?.ProfileImageUrl,
                ImageUrl = post.ImageUrl,
                // Each post needs a separate COUNT query for likes
                LikeCount = await _likeRepository.GetCountByPostIdAsync(post.Id),
                // '.HasValue' checks if the nullable int has a value (not null)
                // '&&' short-circuits: if HasValue is false, the right side is not evaluated
                HasLiked = currentUserId.HasValue
                    && await _likeRepository.GetAsync(post.Id, currentUserId.Value) != null,
                HasBookmarked = currentUserId.HasValue
                    && await _bookmarkRepository.GetAsync(post.Id, currentUserId.Value) != null
            });
        }

        return responses;
    }

    // Returns posts by a specific user (for profile page)
    public async Task<IEnumerable<PostResponse>> GetByUserIdAsync(int userId, int? currentUserId = null)
    {
        var posts = await _postRepository.GetByUserIdAsync(userId);
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

    // Returns paginated posts — wraps results in PagedResponse<T> with metadata.
    public async Task<PagedResponse<PostResponse>> GetAllPagedAsync(int page, int pageSize, int? currentUserId = null)
    {
        // Tuple destructuring: (Items, TotalCount) = await ...
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

    // Full-text search on post content
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

    // Returns posts filtered by hashtag
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

    // Returns posts from users the authenticated user follows (personalized feed)
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

    // Returns a single post by ID with like info
    public async Task<PostResponse> GetByIdAsync(int id, int? currentUserId = null)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
            // KeyNotFoundException is caught by ExceptionMiddleware and mapped to HTTP 404
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

    // Creates a new post: validates length, saves to DB, extracts hashtags.
    public async Task<PostResponse> CreateAsync(CreatePostRequest request, int authorId)
    {
        if (request.Content.Length > 280)
            // ArgumentException is caught by ExceptionMiddleware and mapped to HTTP 400
            throw new ArgumentException("Post must be 280 characters or less.");

        var post = new Post
        {
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            AuthorId = authorId,
            ImageUrl = request.ImageUrl
        };

        // INSERT INTO Posts (...) — the post.Id is auto-generated by the DB and set on the entity
        await _postRepository.AddAsync(post);

        // Extract hashtags (e.g., #dev, #react) from content and create Tag + PostTag records
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

    // Updates a post — checks authorization (author or admin only)
    public async Task<PostResponse> UpdateAsync(int postId, UpdatePostRequest request, int userId, string userRole)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new KeyNotFoundException("Post not found.");

        // Authorization check: only the author or an admin can update
        if (post.AuthorId != userId && userRole != "Admin")
            // UnauthorizedAccessException is caught by ExceptionMiddleware → HTTP 403
            throw new UnauthorizedAccessException("Not authorized.");

        if (request.Content.Length > 280)
            throw new ArgumentException("Post must be 280 characters or less.");

        // Mutate the entity — EF Core tracks changes and generates the UPDATE SQL
        post.Content = request.Content;
        post.ImageUrl = request.ImageUrl;
        post.UpdatedAt = DateTime.UtcNow;

        // Clear old tag associations and re-extract from new content
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

    // Deletes a post — checks authorization (author or admin only)
    public async Task DeleteAsync(int postId, int userId, string userRole)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new KeyNotFoundException("Post not found.");

        if (post.AuthorId != userId && userRole != "Admin")
            throw new UnauthorizedAccessException("Not authorized.");

        // DELETE FROM Posts WHERE Id = postId
        await _postRepository.DeleteAsync(post);
    }

    // Private helper — extracts hashtags from text using regex and creates Tag + PostTag records.
    private async Task ExtractAndSaveTags(Post post, string content)
    {
        // Regex.Matches finds all #word patterns in the content.
        // @"#(\w+)" — @ is a verbatim string (no escaping needed for backslashes).
        // (\w+) captures the word after # (letters, digits, underscore).
        var hashtags = Regex.Matches(content, @"#(\w+)")
            .Select(m => m.Groups[1].Value.ToLower()) // Extract captured group, normalize to lowercase
            .Distinct()  // Remove duplicates (e.g., #dev #dev → just "dev")
            .ToList();   // Materialize to List for iteration

        foreach (var tagName in hashtags)
        {
            // Check if the tag already exists in the DB
            var tag = await _tagRepository.GetByNameAsync(tagName);

            // If not, create a new Tag record
            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                await _tagRepository.AddAsync(tag);
            }

            // Create the join table record linking this post to this tag
            // '??=' — null-coalescing assignment: only assign if left side is null
            post.PostTags ??= new List<PostTag>();
            post.PostTags.Add(new PostTag { PostId = post.Id, TagId = tag.Id });
        }

        // Save the PostTag associations to the database
        await _postRepository.UpdateAsync(post);
    }
}
