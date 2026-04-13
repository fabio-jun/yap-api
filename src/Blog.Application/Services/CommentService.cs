using System.Text.RegularExpressions;
using Blog.Application.DTOs.Comments;
using Blog.Application.DTOs.Mentions;
using Blog.Application.Interfaces;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;

namespace Blog.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        INotificationService notificationService,
        IUserRepository userRepository)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _notificationService = notificationService;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<CommentResponse>> GetByPostIdAsync(int postId)
    {
        var comments = (await _commentRepository.GetByPostIdAsync(postId)).ToList();
        var byParent = comments
            .Where(c => c.ParentCommentId.HasValue)
            .GroupBy(c => c.ParentCommentId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.CreatedAt).ToList());

        var responses = new List<CommentResponse>();
        foreach (var comment in comments.Where(c => !c.ParentCommentId.HasValue).OrderBy(c => c.CreatedAt))
        {
            responses.Add(await MapToResponse(comment, byParent));
        }

        return responses;
    }

    public async Task<CommentResponse> CreateAsync(int postId, CreateCommentRequest request, int authorId)
    {
        return await CreateInternalAsync(postId, request, authorId, null);
    }

    public async Task<CommentResponse> CreateReplyAsync(int postId, int parentCommentId, CreateCommentRequest request, int authorId)
    {
        var parent = await _commentRepository.GetByIdWithPostAsync(parentCommentId);
        if (parent == null || parent.PostId != postId)
            throw new KeyNotFoundException("Parent comment not found.");

        return await CreateInternalAsync(postId, request, authorId, parentCommentId);
    }

    public async Task DeleteAsync(int commentId, int userId, string userRole)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);

        if (comment == null)
            throw new KeyNotFoundException("Comment not found.");

        if (comment.AuthorId != userId && userRole != "Admin")
            throw new UnauthorizedAccessException("Not Authorized.");

        await _commentRepository.DeleteAsync(comment);
    }

    private async Task<CommentResponse> CreateInternalAsync(
        int postId,
        CreateCommentRequest request,
        int authorId,
        int? parentCommentId)
    {
        var comment = new Comment
        {
            Content = request.Content,
            PostId = postId,
            AuthorId = authorId,
            ParentCommentId = parentCommentId,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment);

        var post = await _postRepository.GetByIdAsync(postId);
        if (post != null)
        {
            await _notificationService.CreateNotificationAsync(
                NotificationType.Comment, authorId, post.AuthorId, postId);
        }

        var mentionedUsers = await NotifyMentions(request.Content, authorId, postId);

        return new CommentResponse
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            AuthorId = comment.AuthorId,
            AuthorName = string.Empty,
            ParentCommentId = comment.ParentCommentId,
            MentionedUsers = mentionedUsers
        };
    }

    private async Task<CommentResponse> MapToResponse(
        Comment comment,
        Dictionary<int, List<Comment>> byParent)
    {
        var response = new CommentResponse
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            AuthorId = comment.AuthorId,
            AuthorName = comment.Author?.UserName ?? string.Empty,
            ParentCommentId = comment.ParentCommentId,
            MentionedUsers = await ResolveMentions(comment.Content)
        };

        if (byParent.TryGetValue(comment.Id, out var replies))
        {
            foreach (var reply in replies)
            {
                response.Replies.Add(await MapToResponse(reply, byParent));
            }
        }

        return response;
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
}
