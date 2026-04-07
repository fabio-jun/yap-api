using Blog.Application.Services;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using NSubstitute;

namespace Blog.Tests;

public class LikeServiceTests
{
    private readonly ILikeRepository _likeRepository = Substitute.For<ILikeRepository>();
    private readonly LikeService _sut;

    public LikeServiceTests()
    {
        _sut = new LikeService(_likeRepository);
    }

    [Fact]
    public async Task ToggleLikeAsync_NotLikedYet_AddsLikeAndReturnsTrue()
    {
        _likeRepository.GetAsync(1, 1).Returns((Like?)null);

        var result = await _sut.ToggleLikeAsync(1, 1);

        Assert.True(result);
        await _likeRepository.Received(1).AddAsync(Arg.Is<Like>(l => l.PostId == 1 && l.UserId == 1));
    }

    [Fact]
    public async Task ToggleLikeAsync_AlreadyLiked_RemovesLikeAndReturnsFalse()
    {
        var existingLike = new Like { PostId = 1, UserId = 1 };
        _likeRepository.GetAsync(1, 1).Returns(existingLike);

        var result = await _sut.ToggleLikeAsync(1, 1);

        Assert.False(result);
        await _likeRepository.Received(1).DeleteAsync(existingLike);
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
}
