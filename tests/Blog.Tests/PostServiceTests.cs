using Blog.Application.DTOs.Posts;
using Blog.Application.Services;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using NSubstitute;

namespace Blog.Tests;

public class PostServiceTests
{
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly ILikeRepository _likeRepository = Substitute.For<ILikeRepository>();
    private readonly IBookmarkRepository _bookmarkRepository = Substitute.For<IBookmarkRepository>();
    private readonly IRepostRepository _repostRepository = Substitute.For<IRepostRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly Blog.Application.Interfaces.INotificationService _notificationService = Substitute.For<Blog.Application.Interfaces.INotificationService>();
    private readonly IBlockRepository _blockRepository = Substitute.For<IBlockRepository>();
    private readonly PostService _sut;

    public PostServiceTests()
    {
        _sut = new PostService(_postRepository, _tagRepository, _likeRepository, _bookmarkRepository, _repostRepository, _userRepository, _notificationService, _blockRepository);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPostsOrderedWithLikeInfo()
    {
        var posts = new List<Post>
        {
            new() { Id = 1, Content = "Hello World", AuthorId = 1, Author = new User { Id = 1, UserName = "user1", Email = "u1@test.com", PasswordHash = "h", Role = "User" }, CreatedAt = DateTime.UtcNow }
        };
        _postRepository.GetAllAsync().Returns(posts);
        _likeRepository.GetCountByPostIdAsync(1).Returns(5);
        _likeRepository.GetAsync(1, 10).Returns(new Like { PostId = 1, UserId = 10 });

        var result = (await _sut.GetAllAsync(10)).ToList();

        Assert.Single(result);
        Assert.Equal("Hello World", result[0].Content);
        Assert.Equal(5, result[0].LikeCount);
        Assert.True(result[0].HasLiked);
    }

    [Fact]
    public async Task GetAllAsync_FiltersBlockedUsersForViewer()
    {
        var posts = new List<Post>
        {
            new() { Id = 1, Content = "Hidden", AuthorId = 2, Author = new User { Id = 2, UserName = "blocked", Email = "b@test.com", PasswordHash = "h", Role = "User" } },
            new() { Id = 2, Content = "Visible", AuthorId = 3, Author = new User { Id = 3, UserName = "visible", Email = "v@test.com", PasswordHash = "h", Role = "User" } }
        };
        _postRepository.GetAllAsync().Returns(posts);
        _blockRepository.GetBlockedUserIdsForViewerAsync(1).Returns(new HashSet<int> { 2 });

        var result = (await _sut.GetAllAsync(1)).ToList();

        Assert.Single(result);
        Assert.Equal("Visible", result[0].Content);
    }

    [Fact]
    public async Task GetAllAsync_WithoutUser_HasLikedIsFalse()
    {
        var posts = new List<Post>
        {
            new() { Id = 1, Content = "Hello World", AuthorId = 1, Author = new User { Id = 1, UserName = "user1", Email = "u1@test.com", PasswordHash = "h", Role = "User" }, CreatedAt = DateTime.UtcNow }
        };
        _postRepository.GetAllAsync().Returns(posts);
        _likeRepository.GetCountByPostIdAsync(1).Returns(0);

        var result = (await _sut.GetAllAsync()).ToList();

        Assert.False(result[0].HasLiked);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsKeyNotFoundException()
    {
        _postRepository.GetByIdAsync(99).Returns((Post?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(99));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPostWithLikeInfo()
    {
        var post = new Post { Id = 1, Content = "Test", AuthorId = 1, Author = new User { Id = 1, UserName = "user1", Email = "u@t.com", PasswordHash = "h", Role = "User" }, CreatedAt = DateTime.UtcNow };
        _postRepository.GetByIdAsync(1).Returns(post);
        _likeRepository.GetCountByPostIdAsync(1).Returns(3);

        var result = await _sut.GetByIdAsync(1);

        Assert.Equal("Test", result.Content);
        Assert.Equal(3, result.LikeCount);
    }

    [Fact]
    public async Task CreateAsync_ValidContent_CreatesPost()
    {
        var request = new CreatePostRequest { Content = "New yap" };

        var result = await _sut.CreateAsync(request, 1);

        Assert.Equal("New yap", result.Content);
        await _postRepository.Received(1).AddAsync(Arg.Any<Post>());
    }

    [Fact]
    public async Task CreateAsync_Over280Chars_ThrowsArgumentException()
    {
        var request = new CreatePostRequest { Content = new string('a', 281) };

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(request, 1));
    }

    [Fact]
    public async Task CreateAsync_Exactly280Chars_Succeeds()
    {
        var request = new CreatePostRequest { Content = new string('a', 280) };

        var result = await _sut.CreateAsync(request, 1);

        Assert.Equal(280, result.Content.Length);
    }

    [Fact]
    public async Task CreateAsync_WithHashtags_ExtractsAndSavesTags()
    {
        var request = new CreatePostRequest { Content = "Hello #hashtag #test" };
        _tagRepository.GetByNameAsync("hashtag").Returns((Tag?)null);
        _tagRepository.GetByNameAsync("test").Returns((Tag?)null);

        await _sut.CreateAsync(request, 1);

        await _tagRepository.Received(1).AddAsync(Arg.Is<Tag>(t => t.Name == "hashtag"));
        await _tagRepository.Received(1).AddAsync(Arg.Is<Tag>(t => t.Name == "test"));
    }

    [Fact]
    public async Task CreateAsync_WithMention_CreatesMentionNotification()
    {
        var request = new CreatePostRequest { Content = "Hello @alice" };
        _userRepository.GetByUserNameAsync("alice").Returns(new User { Id = 9, UserName = "alice", Email = "a@test.com", PasswordHash = "h", Role = "User" });

        await _sut.CreateAsync(request, 1);

        await _notificationService.Received(1).CreateNotificationAsync(
            NotificationType.Mention, 1, 9, Arg.Any<int?>());
    }

    [Fact]
    public async Task UpdateAsync_NotAuthor_ThrowsUnauthorizedAccessException()
    {
        var post = new Post { Id = 1, Content = "Old", AuthorId = 1, Author = new User { Id = 1, UserName = "u", Email = "e", PasswordHash = "h", Role = "User" } };
        _postRepository.GetByIdAsync(1).Returns(post);
        var request = new UpdatePostRequest { Content = "New" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.UpdateAsync(1, request, 999, "User"));
    }

    [Fact]
    public async Task UpdateAsync_Admin_CanUpdateAnyPost()
    {
        var post = new Post { Id = 1, Content = "Old", AuthorId = 1, Author = new User { Id = 1, UserName = "u", Email = "e", PasswordHash = "h", Role = "User" } };
        _postRepository.GetByIdAsync(1).Returns(post);
        var request = new UpdatePostRequest { Content = "Updated by admin" };

        var result = await _sut.UpdateAsync(1, request, 999, "Admin");

        Assert.Equal("Updated by admin", result.Content);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsKeyNotFoundException()
    {
        _postRepository.GetByIdAsync(99).Returns((Post?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(99, 1, "User"));
    }

    [Fact]
    public async Task DeleteAsync_NotAuthorNotAdmin_ThrowsUnauthorizedAccessException()
    {
        var post = new Post { Id = 1, Content = "Test", AuthorId = 1, Author = new User { Id = 1, UserName = "u", Email = "e", PasswordHash = "h", Role = "User" } };
        _postRepository.GetByIdAsync(1).Returns(post);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.DeleteAsync(1, 999, "User"));
    }

    [Fact]
    public async Task DeleteAsync_Author_Succeeds()
    {
        var post = new Post { Id = 1, Content = "Test", AuthorId = 1, Author = new User { Id = 1, UserName = "u", Email = "e", PasswordHash = "h", Role = "User" } };
        _postRepository.GetByIdAsync(1).Returns(post);

        await _sut.DeleteAsync(1, 1, "User");

        await _postRepository.Received(1).DeleteAsync(post);
    }

    [Fact]
    public async Task SearchAsync_ReturnsMatchingPosts()
    {
        var posts = new List<Post>
        {
            new() { Id = 1, Content = "Hello world", AuthorId = 1, Author = new User { Id = 1, UserName = "u", Email = "e", PasswordHash = "h", Role = "User" } }
        };
        _postRepository.SearchAsync("world").Returns(posts);
        _likeRepository.GetCountByPostIdAsync(1).Returns(0);

        var result = (await _sut.SearchAsync("world")).ToList();

        Assert.Single(result);
        Assert.Equal("Hello world", result[0].Content);
    }

    [Fact]
    public async Task GetByTagAsync_ReturnsFilteredPosts()
    {
        var posts = new List<Post>
        {
            new() { Id = 1, Content = "#test one", AuthorId = 1, Author = new User { Id = 1, UserName = "u", Email = "e", PasswordHash = "h", Role = "User" } }
        };
        _postRepository.GetByTagAsync("test").Returns(posts);
        _likeRepository.GetCountByPostIdAsync(1).Returns(2);

        var result = (await _sut.GetByTagAsync("test")).ToList();

        Assert.Single(result);
        Assert.Equal(2, result[0].LikeCount);
    }

    [Fact]
    public async Task GetFeedAsync_ReturnsPostsFromFollowedUsers()
    {
        var posts = new List<Post>
        {
            new() { Id = 1, Content = "Feed post", AuthorId = 2, Author = new User { Id = 2, UserName = "followed", Email = "e", PasswordHash = "h", Role = "User" } }
        };
        _postRepository.GetFeedAsync(1).Returns(posts);
        _likeRepository.GetCountByPostIdAsync(1).Returns(1);
        _likeRepository.GetAsync(1, 1).Returns(new Like { PostId = 1, UserId = 1 });

        var result = (await _sut.GetFeedAsync(1)).ToList();

        Assert.Single(result);
        Assert.Equal("Feed post", result[0].Content);
        Assert.True(result[0].HasLiked);
    }

    [Fact]
    public async Task GetAllPagedAsync_ReturnsPaginatedResponse()
    {
        var posts = new List<Post>
        {
            new() { Id = 1, Content = "Post 1", AuthorId = 1, Author = new User { Id = 1, UserName = "u", Email = "e", PasswordHash = "h", Role = "User" } }
        };
        _postRepository.GetAllAsync().Returns(posts);
        _likeRepository.GetCountByPostIdAsync(1).Returns(0);

        var result = await _sut.GetAllPagedAsync(1, 10);

        Assert.Single(result.Items);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task GetFeedAsync_IncludesRepostsFromFollowedUsers()
    {
        var originalAuthor = new User { Id = 2, UserName = "original", Email = "o@test.com", PasswordHash = "h", Role = "User" };
        var reposter = new User { Id = 3, UserName = "reposter", Email = "r@test.com", PasswordHash = "h", Role = "User" };
        var repost = new Repost
        {
            Id = 7,
            UserId = 3,
            User = reposter,
            PostId = 1,
            Post = new Post { Id = 1, Content = "Original yap", AuthorId = 2, Author = originalAuthor },
            CreatedAt = DateTime.UtcNow,
            QuoteContent = "Look at this"
        };
        _postRepository.GetFeedAsync(1).Returns(new List<Post>());
        _repostRepository.GetFeedAsync(1).Returns(new List<Repost> { repost });
        _repostRepository.GetCountByPostIdAsync(1).Returns(1);

        var result = (await _sut.GetFeedAsync(1)).ToList();

        Assert.Single(result);
        Assert.True(result[0].IsRepost);
        Assert.Equal(7, result[0].RepostId);
        Assert.Equal("reposter", result[0].RepostedByUserName);
        Assert.Equal("Look at this", result[0].QuoteContent);
        Assert.Equal("Original yap", result[0].Content);
    }
}
