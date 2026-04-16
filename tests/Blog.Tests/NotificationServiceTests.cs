using Blog.Application.Cache;
using Blog.Application.DTOs;
using Blog.Application.DTOs.Notifications;
using Blog.Application.Interfaces;
using Blog.Application.Services;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using NSubstitute;

namespace Blog.Tests;

public class NotificationServiceTests
{
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _sut = new NotificationService(_notificationRepository, _cacheService);
    }

    [Fact]
    public async Task GetNotificationsAsync_CacheHit_ReturnsCachedNotificationsWithoutRepository()
    {
        var cached = new PagedResponse<NotificationResponse>
        {
            Items = new List<NotificationResponse>
            {
                new() { Id = 1, Type = "like", ActorId = 2, ActorUsername = "alice" }
            },
            Page = 1,
            PageSize = 20,
            TotalCount = 1
        };
        _cacheService.GetAsync<PagedResponse<NotificationResponse>>(CacheKeys.Notifications(1)).Returns(cached);

        var result = await _sut.GetNotificationsAsync(1, 1, 20);

        Assert.Single(result.Items);
        Assert.Equal("alice", result.Items.Single().ActorUsername);
        await _notificationRepository.DidNotReceive().GetByUserIdAsync(1, 1, 20);
        await _notificationRepository.DidNotReceive().GetTotalCountAsync(1);
    }

    [Fact]
    public async Task GetNotificationsAsync_CacheMiss_PopulatesNotificationsCache()
    {
        var notifications = new List<Notification>
        {
            new()
            {
                Id = 1,
                Type = NotificationType.Like,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                ActorId = 2,
                UserId = 1,
                Actor = new User { Id = 2, UserName = "alice", Email = "a@b.com", PasswordHash = "x", Role = "User" }
            }
        };
        _cacheService.GetAsync<PagedResponse<NotificationResponse>>(CacheKeys.Notifications(1)).Returns((PagedResponse<NotificationResponse>?)null);
        _notificationRepository.GetByUserIdAsync(1, 1, 20).Returns(notifications);
        _notificationRepository.GetTotalCountAsync(1).Returns(1);

        var result = await _sut.GetNotificationsAsync(1, 1, 20);

        Assert.Single(result.Items);
        await _cacheService.Received(1).SetAsync(
            CacheKeys.Notifications(1),
            Arg.Is<PagedResponse<NotificationResponse>>(response => response.TotalCount == 1 && response.Items.Single().ActorUsername == "alice"),
            TimeSpan.FromSeconds(15),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateNotificationAsync_CreatesNotification()
    {
        _notificationRepository.ExistsAsync(NotificationType.Like, 2, 1, 10).Returns(false);

        await _sut.CreateNotificationAsync(NotificationType.Like, 2, 1, 10);

        await _notificationRepository.Received(1).CreateAsync(Arg.Is<Notification>(n =>
            n.Type == NotificationType.Like &&
            n.ActorId == 2 &&
            n.UserId == 1 &&
            n.PostId == 10 &&
            !n.IsRead
        ));
    }

    [Fact]
    public async Task CreateNotificationAsync_SelfAction_DoesNotCreate()
    {
        await _sut.CreateNotificationAsync(NotificationType.Like, 1, 1, 10);

        await _notificationRepository.DidNotReceive().CreateAsync(Arg.Any<Notification>());
    }

    [Fact]
    public async Task CreateNotificationAsync_Duplicate_DoesNotCreate()
    {
        _notificationRepository.ExistsAsync(NotificationType.Like, 2, 1, 10).Returns(true);

        await _sut.CreateNotificationAsync(NotificationType.Like, 2, 1, 10);

        await _notificationRepository.DidNotReceive().CreateAsync(Arg.Any<Notification>());
    }

    [Fact]
    public async Task DeleteNotificationAsync_CallsRepository()
    {
        await _sut.DeleteNotificationAsync(NotificationType.Follow, 2, 1, null);

        await _notificationRepository.Received(1).DeleteByTypeAsync(NotificationType.Follow, 2, 1, null);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCount()
    {
        _notificationRepository.GetUnreadCountAsync(1).Returns(5);

        var result = await _sut.GetUnreadCountAsync(1);

        Assert.Equal(5, result);
    }

    [Fact]
    public async Task MarkAsReadAsync_CallsRepository()
    {
        await _sut.MarkAsReadAsync(42, 1);

        await _notificationRepository.Received(1).MarkAsReadAsync(42, 1);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_CallsRepository()
    {
        await _sut.MarkAllAsReadAsync(1);

        await _notificationRepository.Received(1).MarkAllAsReadAsync(1);
    }

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        await _sut.DeleteAsync(42, 1);

        await _notificationRepository.Received(1).DeleteAsync(42, 1);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsMappedNotifications()
    {
        var notifications = new List<Notification>
        {
            new()
            {
                Id = 1,
                Type = NotificationType.Like,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                ActorId = 2,
                UserId = 1,
                PostId = 10,
                Actor = new User { Id = 2, UserName = "alice", Email = "a@b.com", PasswordHash = "x", Role = "User" },
                Post = new Post { Id = 10, Content = "Hello world", AuthorId = 1 }
            }
        };
        _notificationRepository.GetRecentByUserIdAsync(1, 8).Returns(notifications);

        var result = await _sut.GetRecentAsync(1, 8);

        Assert.Single(result);
        Assert.Equal("like", result[0].Type);
        Assert.Equal("alice", result[0].ActorUsername);
        Assert.Equal("Hello world", result[0].PostContentPreview);
    }

    [Fact]
    public async Task GetRecentAsync_TruncatesLongPostContent()
    {
        var longContent = new string('a', 100);
        var notifications = new List<Notification>
        {
            new()
            {
                Id = 1,
                Type = NotificationType.Comment,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                ActorId = 2,
                UserId = 1,
                PostId = 10,
                Actor = new User { Id = 2, UserName = "bob", Email = "b@b.com", PasswordHash = "x", Role = "User" },
                Post = new Post { Id = 10, Content = longContent, AuthorId = 1 }
            }
        };
        _notificationRepository.GetRecentByUserIdAsync(1, 8).Returns(notifications);

        var result = await _sut.GetRecentAsync(1, 8);

        Assert.Equal(53, result[0].PostContentPreview!.Length);
        Assert.EndsWith("...", result[0].PostContentPreview!);
    }

    [Fact]
    public async Task CreateNotificationAsync_FollowType_PostIdIsNull()
    {
        _notificationRepository.ExistsAsync(NotificationType.Follow, 2, 1, null).Returns(false);

        await _sut.CreateNotificationAsync(NotificationType.Follow, 2, 1, null);

        await _notificationRepository.Received(1).CreateAsync(Arg.Is<Notification>(n =>
            n.Type == NotificationType.Follow &&
            n.PostId == null
        ));
    }
}
