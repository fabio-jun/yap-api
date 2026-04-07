using Blog.Application.Services;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using NSubstitute;

namespace Blog.Tests;

public class FollowServiceTests
{
    private readonly IFollowRepository _followRepository = Substitute.For<IFollowRepository>();
    private readonly FollowService _sut;

    public FollowServiceTests()
    {
        _sut = new FollowService(_followRepository);
    }

    [Fact]
    public async Task ToggleFollowAsync_SelfFollow_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.ToggleFollowAsync(1, 1));
    }

    [Fact]
    public async Task ToggleFollowAsync_NotFollowing_AddsFollowAndReturnsTrue()
    {
        _followRepository.GetAsync(1, 2).Returns((Follow?)null);

        var result = await _sut.ToggleFollowAsync(1, 2);

        Assert.True(result);
        await _followRepository.Received(1).AddAsync(Arg.Is<Follow>(f => f.FollowerId == 1 && f.FollowingId == 2));
    }

    [Fact]
    public async Task ToggleFollowAsync_AlreadyFollowing_RemovesFollowAndReturnsFalse()
    {
        var existing = new Follow { FollowerId = 1, FollowingId = 2 };
        _followRepository.GetAsync(1, 2).Returns(existing);

        var result = await _sut.ToggleFollowAsync(1, 2);

        Assert.False(result);
        await _followRepository.Received(1).DeleteAsync(existing);
    }

    [Fact]
    public async Task GetFollowersAsync_ReturnsMappedUsers()
    {
        var users = new List<User>
        {
            new() { Id = 1, UserName = "follower1", Email = "f1@test.com", PasswordHash = "h", Role = "User", CreatedAt = DateTime.UtcNow }
        };
        _followRepository.GetFollowersAsync(2).Returns(users);

        var result = (await _sut.GetFollowersAsync(2)).ToList();

        Assert.Single(result);
        Assert.Equal("follower1", result[0].UserName);
    }

    [Fact]
    public async Task GetFollowingAsync_ReturnsMappedUsers()
    {
        var users = new List<User>
        {
            new() { Id = 2, UserName = "followed1", Email = "f2@test.com", PasswordHash = "h", Role = "User", CreatedAt = DateTime.UtcNow }
        };
        _followRepository.GetFollowingAsync(1).Returns(users);

        var result = (await _sut.GetFollowingAsync(1)).ToList();

        Assert.Single(result);
        Assert.Equal("followed1", result[0].UserName);
    }

    [Fact]
    public async Task IsFollowingAsync_Following_ReturnsTrue()
    {
        _followRepository.GetAsync(1, 2).Returns(new Follow { FollowerId = 1, FollowingId = 2 });

        var result = await _sut.IsFollowingAsync(1, 2);

        Assert.True(result);
    }

    [Fact]
    public async Task IsFollowingAsync_NotFollowing_ReturnsFalse()
    {
        _followRepository.GetAsync(1, 2).Returns((Follow?)null);

        var result = await _sut.IsFollowingAsync(1, 2);

        Assert.False(result);
    }
}
