using Blog.Application.DTOs.Comments;

namespace Blog.Application.Interfaces;

// Service interface for comment operations.
public interface ICommentService
{
    // Returns all comments for a specific post
    Task<IEnumerable<CommentResponse>> GetByPostIdAsync(int postId);

    // Creates a new comment on a post
    Task<CommentResponse> CreateAsync(int postId, CreateCommentRequest request, int authorId);

    // Deletes a comment — only the author or an admin can delete
    Task DeleteAsync(int commentId, int userId, string userRole);
}
