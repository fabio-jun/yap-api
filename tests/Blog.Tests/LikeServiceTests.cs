using Blog.Application.Interfaces;
using Blog.Application.Services;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using NSubstitute;

namespace Blog.Tests;

public class LikeServiceTests
{
    private readonly ILikeRepository _likeRepository = Substitute.For<ILikeRepository>();
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly INotificationService _notificationService = Substitute.For<INotificationService>();
    private readonly LikeService _sut;

    public LikeServiceTests()
    {
        _sut = new LikeService(_likeRepository, _postRepository, _notificationService);
    }

    [Fact]
    public async Task ToggleLikeAsync_NotLikedYet_AddsLikeAndReturnsTrue()
    {
        _likeRepository.GetAsync(1, 1).Returns((Like?)null);
        _postRepository.GetByIdAsync(1).Returns(new Post { Id = 1, AuthorId = 5, Content = "test" });

        var result = await _sut.ToggleLikeAsync(1, 1);

        Assert.True(result);
        await _likeRepository.Received(1).AddAsync(Arg.Is<Like>(l => l.PostId == 1 && l.UserId == 1));
        await _notificationService.Received(1).CreateNotificationAsync(
            NotificationType.Like, 1, 5, 1);
    }

    [Fact]
    public async Task ToggleLikeAsync_AlreadyLiked_RemovesLikeAndReturnsFalse()
    {
        var existingLike = new Like { PostId = 1, UserId = 1 };
        _likeRepository.GetAsync(1, 1).Returns(existingLike);
        _postRepository.GetByIdAsync(1).Returns(new Post { Id = 1, AuthorId = 5, Content = "test" });

        var result = await _sut.ToggleLikeAsync(1, 1);

        Assert.False(result);
        await _likeRepository.Received(1).DeleteAsync(existingLike);
        await _notificationService.Received(1).DeleteNotificationAsync(
            NotificationType.Like, 1, 5, 1);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCount()
    {
        _likeRepository.GetCountByPostIdAsync(1).Returns(42);

        var result = await _sut.GetCountAsync(1);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task HasLikedAsync_UserLiked_ReturnsTrue()
    {
        _likeRepository.GetAsync(1, 1).Returns(new Like { PostId = 1, UserId = 1 });

        var result = await _sut.HasLikedAsync(1, 1);

        Assert.True(result);
    }

    [Fact]
    public async Task HasLikedAsync_UserNotLiked_ReturnsFalse()
    {
        _likeRepository.GetAsync(1, 1).Returns((Like?)null);

        var result = await _sut.HasLikedAsync(1, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task GetUsersAsync_ReturnsUsersWhoLikedPost()
    {
        _likeRepository.GetByPostIdWithUsersAsync(1).Returns(new List<Like>
        {
            new()
            {
                PostId = 1,
                UserId = 2,
                User = new User
                {
                    Id = 2,
                    UserName = "ana",
                    DisplayName = "Ana",
                    ProfileImageUrl = "https://example.com/ana.jpg",
                    Email = "ana@example.com",
                    PasswordHash = "hash",
                    Role = "User"
                }
            },
            new()
            {
                PostId = 1,
                UserId = 3,
                User = new User
                {
                    Id = 3,
                    UserName = "ben",
                    Email = "ben@example.com",
                    PasswordHash = "hash",
                    Role = "User"
                }
            }
        });

        var result = await _sut.GetUsersAsync(1);

        Assert.Equal(2, result.Count);
        Assert.Equal(2, result[0].Id);
        Assert.Equal("ana", result[0].UserName);
        Assert.Equal("Ana", result[0].DisplayName);
        Assert.Equal("https://example.com/ana.jpg", result[0].ProfileImageUrl);
        Assert.Equal(3, result[1].Id);
        Assert.Equal("ben", result[1].UserName);
    }
}
