using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.DTOs.Comments;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

// Service that handles comment business logic: CRUD + authorization checks.
public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;

    public CommentService(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    // Returns all comments for a post, mapped to DTOs.
    public async Task<IEnumerable<CommentResponse>> GetByPostIdAsync(int postId)
    {
        var comments = await _commentRepository.GetByPostIdAsync(postId);

        // LINQ .Select() — transforms each element. Like Array.map() in JavaScript.
        // 'c =>' is a lambda expression: c is the input parameter, the right side is the return value.
        return comments.Select(c => new CommentResponse
        {
            Id = c.Id,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            AuthorId = c.AuthorId,
            AuthorName = c.Author?.UserName ?? string.Empty
        });
    }

    // Creates a new comment on a post.
    public async Task<CommentResponse> CreateAsync(int postId, CreateCommentRequest request, int authorId)
    {
        // Create the entity from the DTO + route param + JWT claim
        var comment = new Comment
        {
            Content = request.Content,
            PostId = postId,
            AuthorId = authorId,
            CreatedAt = DateTime.UtcNow
        };

        // INSERT INTO Comments — EF Core sets comment.Id after SaveChangesAsync
        await _commentRepository.AddAsync(comment);

        return new CommentResponse
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            AuthorId = comment.AuthorId,
            AuthorName = string.Empty
        };
    }

    // Deletes a comment — only the author or admin can delete.
    public async Task DeleteAsync(int commentId, int userId, string userRole)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);

        if (comment == null)
        {
            // Caught by ExceptionMiddleware → HTTP 404
            throw new KeyNotFoundException("Comment not found.");
        }

        // Authorization: only the comment author or an admin can delete
        if (comment.AuthorId != userId && userRole != "Admin")
        {
            // Caught by ExceptionMiddleware → HTTP 403
            throw new UnauthorizedAccessException("Not Authorized.");
        }

        await _commentRepository.DeleteAsync(comment);
    }
}
