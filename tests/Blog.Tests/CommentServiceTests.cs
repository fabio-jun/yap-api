using Blog.Application.DTOs.Comments;
using Blog.Application.Services;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using NSubstitute;

namespace Blog.Tests;

public class CommentServiceTests
{
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly CommentService _sut;

    public CommentServiceTests()
    {
        _sut = new CommentService(_commentRepository);
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
        Assert.Equal("Nice post!", result[0].Content);
        Assert.Equal("commenter", result[0].AuthorName);
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsCommentResponse()
    {
        var request = new CreateCommentRequest { Content = "Great yap" };

        var result = await _sut.CreateAsync(1, request, 2);

        Assert.Equal("Great yap", result.Content);
        Assert.Equal(2, result.AuthorId);
        await _commentRepository.Received(1).AddAsync(Arg.Is<Comment>(c => c.Content == "Great yap" && c.PostId == 1 && c.AuthorId == 2));
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
