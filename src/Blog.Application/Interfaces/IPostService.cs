using Blog.Application.DTOs;
using Blog.Application.DTOs.Posts;

namespace Blog.Application.Interfaces;

// Service interface for post (yap) operations.
// currentUserId is optional (int?) because some endpoints are public — unauthenticated users
// can view posts but won't see HasLiked/HasBookmarked status.
public interface IPostService
{
    // Returns all posts (most recent first), with like/bookmark info for the current user
    Task<IEnumerable<PostResponse>> GetAllAsync(int? currentUserId = null);

    // Returns posts by a specific user
    Task<IEnumerable<PostResponse>> GetByUserIdAsync(int userId, int? currentUserId = null);

    // Returns paginated posts — uses PagedResponse<T> wrapper with metadata
    Task<PagedResponse<PostResponse>> GetAllPagedAsync(int page, int pageSize, int? currentUserId = null);

    // Returns posts from users the authenticated user follows (personalized feed)
    Task<IEnumerable<PostResponse>> GetFeedAsync(int userId);

    // Full-text search on post content (case-insensitive via PostgreSQL ILike)
    Task<IEnumerable<PostResponse>> SearchAsync(string query, int? currentUserId = null);

    // Returns posts filtered by hashtag
    Task<IEnumerable<PostResponse>> GetByTagAsync(string tagName, int? currentUserId = null);

    // Returns a single post by ID, with like info for the current user
    Task<PostResponse> GetByIdAsync(int id, int? currentUserId = null);

    // Creates a new post (max 280 characters), extracts hashtags
    Task<PostResponse> CreateAsync(CreatePostRequest request, int authorId);

    // Updates a post — only the author or an admin can update
    Task<PostResponse> UpdateAsync(int postId, UpdatePostRequest request, int userId, string userRole);

    // Deletes a post — only the author or an admin can delete
    Task DeleteAsync(int postId, int userId, string userRole);
}
