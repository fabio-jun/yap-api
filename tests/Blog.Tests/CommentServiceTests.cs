using Blog.Application.DTOs.Comments;
using Blog.Application.Interfaces;
using Blog.Application.Services;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using NSubstitute;

namespace Blog.Tests;

public class CommentServiceTests
{
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly INotificationService _notificationService = Substitute.For<INotificationService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly CommentService _sut;

    public CommentServiceTests()
    {
        _sut = new CommentService(_commentRepository, _postRepository, _notificationService, _userRepository);
    }

    [Fact]
    public async Task GetByPostIdAsync_ReturnsComments()
    {
        var comments = new List<Comment>
        {
            new() { Id = 1, Content = "Nice post", PostId = 1, AuthorId = 2, Author = new User { Id = 2, UserName = "commenter", Email = "c@t.com", PasswordHash = "h", Role = "User" }, CreatedAt = DateTime.UtcNow }
        };
        _commentRepository.GetByPostIdAsync(1).Returns(comments);

        var result = (await _sut.GetByPostIdAsync(1)).ToList();

        Assert.Single(result);
        Assert.Equal("Nice post", result[0].Content);
        Assert.Equal("commenter", result[0].AuthorName);
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsCommentResponse()
    {
        var request = new CreateCommentRequest { Content = "Great yap" };
        _postRepository.GetByIdAsync(1).Returns(new Post { Id = 1, AuthorId = 5, Content = "test" });

        var result = await _sut.CreateAsync(1, request, 2);

        Assert.Equal("Great yap", result.Content);
        Assert.Equal(2, result.AuthorId);
        await _commentRepository.Received(1).AddAsync(Arg.Is<Comment>(c => c.Content == "Great yap" && c.PostId == 1 && c.AuthorId == 2));
        await _notificationService.Received(1).CreateNotificationAsync(
            NotificationType.Comment, 2, 5, 1);
    }

    [Fact]
    public async Task CreateReplyAsync_CreatesReplyForParentComment()
    {
        var request = new CreateCommentRequest { Content = "Reply" };
        _commentRepository.GetByIdWithPostAsync(10).Returns(new Comment { Id = 10, Content = "Parent", PostId = 1, AuthorId = 3 });
        _postRepository.GetByIdAsync(1).Returns(new Post { Id = 1, AuthorId = 5, Content = "test" });

        var result = await _sut.CreateReplyAsync(1, 10, request, 2);

        Assert.Equal(10, result.ParentCommentId);
        await _commentRepository.Received(1).AddAsync(Arg.Is<Comment>(c =>
            c.Content == "Reply" &&
            c.PostId == 1 &&
            c.AuthorId == 2 &&
            c.ParentCommentId == 10));
    }

    [Fact]
    public async Task GetByPostIdAsync_ReturnsCommentTree()
    {
        var comments = new List<Comment>
        {
            new() { Id = 1, Content = "Parent", PostId = 1, AuthorId = 2, Author = new User { Id = 2, UserName = "parent", Email = "p@test.com", PasswordHash = "h", Role = "User" }, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Content = "Child", PostId = 1, AuthorId = 3, ParentCommentId = 1, Author = new User { Id = 3, UserName = "child", Email = "c@test.com", PasswordHash = "h", Role = "User" }, CreatedAt = DateTime.UtcNow }
        };
        _commentRepository.GetByPostIdAsync(1).Returns(comments);

        var result = (await _sut.GetByPostIdAsync(1)).ToList();

        Assert.Single(result);
        Assert.Single(result[0].Replies);
        Assert.Equal("Child", result[0].Replies[0].Content);
    }

    [Fact]
    public async Task CreateAsync_WithMention_CreatesMentionNotification()
    {
        var request = new CreateCommentRequest { Content = "hi @alice" };
        _postRepository.GetByIdAsync(1).Returns(new Post { Id = 1, AuthorId = 5, Content = "test" });
        _userRepository.GetByUserNameAsync("alice").Returns(new User { Id = 9, UserName = "alice", Email = "a@test.com", PasswordHash = "h", Role = "User" });

        await _sut.CreateAsync(1, request, 2);

        await _notificationService.Received(1).CreateNotificationAsync(
            NotificationType.Mention, 2, 9, 1);
    }

    [Fact]
    public async Task DeleteAsync_CommentNotFound_ThrowsKeyNotFoundException()
    {
        _commentRepository.GetByIdAsync(99).Returns((Comment?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(99, 1, "User"));
    }

    [Fact]
    public async Task DeleteAsync_NotAuthorNotAdmin_ThrowsUnauthorizedAccessException()
    {
        var comment = new Comment { Id = 1, Content = "Test", PostId = 1, AuthorId = 2 };
        _commentRepository.GetByIdAsync(1).Returns(comment);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.DeleteAsync(1, 999, "User"));
    }

    [Fact]
    public async Task DeleteAsync_Author_Succeeds()
    {
        var comment = new Comment { Id = 1, Content = "Test", PostId = 1, AuthorId = 2 };
        _commentRepository.GetByIdAsync(1).Returns(comment);

        await _sut.DeleteAsync(1, 2, "User");

        await _commentRepository.Received(1).DeleteAsync(comment);
    }

    [Fact]
    public async Task DeleteAsync_Admin_CanDeleteAnyComment()
    {
        var comment = new Comment { Id = 1, Content = "Test", PostId = 1, AuthorId = 2 };
        _commentRepository.GetByIdAsync(1).Returns(comment);

        await _sut.DeleteAsync(1, 999, "Admin");

        await _commentRepository.Received(1).DeleteAsync(comment);
    }
}
