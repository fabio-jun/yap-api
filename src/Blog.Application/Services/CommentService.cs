using System.Security.Cryptography.X509Certificates;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Blog.Application.DTOs.Comments;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;

    public CommentService(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<IEnumerable<CommentResponse>> GetByPostIdAsync(int postId)
    {
        // GetByPostIdAsync method from the service layer
        var comments = await _commentRepository.GetByPostIdAsync(postId);

        return comments.Select(c => new CommentResponse
        {
            Id = c.Id,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            AuthorId = c.AuthorId,
            AuthorName = c.Author?.UserName ?? string.Empty
        });
    }

    public async Task<CommentResponse> CreateAsync(int postId, CreateCommentRequest request, int authorId)
    {
        var comment = new Comment
        {
            Content = request.Content,
            PostId = postId,
            AuthorId = authorId,
            CreatedAt = DateTime.UtcNow
        };

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

        public async Task DeleteAsync(int commentId, int userId, string userRole)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);

            if (comment == null)
            {
                throw new KeyNotFoundException("Comment not found.");
            }

            if (comment.AuthorId != userId && userRole != "Admin")
            {
                throw new UnauthorizedAccessException("Not Authorized.");
            }

            await _commentRepository.DeleteAsync(comment);
        }
}
