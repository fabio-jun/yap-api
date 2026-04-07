using Blog.Application.DTOs.Users;
using Blog.Application.Services;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using NSubstitute;

namespace Blog.Tests;

public class UserServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IFollowRepository _followRepository = Substitute.For<IFollowRepository>();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_userRepository, _followRepository);
    }

    [Fact]
    public async Task GetByIdAsync_UserExists_ReturnsUserResponseWithFollowCounts()
    {
        var user = new User { Id = 1, UserName = "testuser", Email = "test@test.com", PasswordHash = "h", Role = "User", CreatedAt = DateTime.UtcNow };
        _userRepository.GetByIdAsync(1).Returns(user);
        _followRepository.GetFollowersCountAsync(1).Returns(10);
        _followRepository.GetFollowingCountAsync(1).Returns(5);

        var result = await _sut.GetByIdAsync(1);

        Assert.Equal("testuser", result.UserName);
        Assert.Equal(10, result.FollowersCount);
        Assert.Equal(5, result.FollowingCount);
    }

    [Fact]
    public async Task GetByIdAsync_UserNotFound_ThrowsKeyNotFoundException()
    {
        _userRepository.GetByIdAsync(99).Returns((User?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(99));
    }

    [Fact]
    public async Task UpdateAsync_ValidData_UpdatesAndReturnsUserResponse()
    {
        var user = new User { Id = 1, UserName = "old", Email = "old@test.com", PasswordHash = "h", Role = "User", CreatedAt = DateTime.UtcNow };
        _userRepository.GetByIdAsync(1).Returns(user);
        _followRepository.GetFollowersCountAsync(1).Returns(0);
        _followRepository.GetFollowingCountAsync(1).Returns(0);

        var request = new UpdateUserRequest { userName = "newname", Email = "new@test.com" };
        var result = await _sut.UpdateAsync(1, request);

        Assert.Equal("newname", result.UserName);
        Assert.Equal("new@test.com", result.Email);
        await _userRepository.Received(1).UpdateAsync(user);
    }

    [Fact]
    public async Task UpdateAsync_UserNotFound_ThrowsKeyNotFoundException()
    {
        _userRepository.GetByIdAsync(99).Returns((User?)null);
        var request = new UpdateUserRequest { userName = "x", Email = "x@x.com" };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(99, request));
    }

    [Fact]
    public async Task SearchAsync_ReturnsMatchingUsersWithFollowCounts()
    {
        var users = new List<User>
        {
            new() { Id = 1, UserName = "alice", Email = "alice@test.com", PasswordHash = "h", Role = "User", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, UserName = "alisson", Email = "alisson@test.com", PasswordHash = "h", Role = "User", CreatedAt = DateTime.UtcNow }
        };
        _userRepository.SearchAsync("ali").Returns(users);
        _followRepository.GetFollowersCountAsync(1).Returns(3);
        _followRepository.GetFollowingCountAsync(1).Returns(1);
        _followRepository.GetFollowersCountAsync(2).Returns(7);
        _followRepository.GetFollowingCountAsync(2).Returns(2);

        var result = (await _sut.SearchAsync("ali")).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("alice", result[0].UserName);
        Assert.Equal(3, result[0].FollowersCount);
        Assert.Equal("alisson", result[1].UserName);
        Assert.Equal(7, result[1].FollowersCount);
    }
}
