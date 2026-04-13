using Blog.Application.DTOs.Reposts;
using Blog.Application.Services;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using NSubstitute;

namespace Blog.Tests;

public class RepostServiceTests
{
    private readonly IRepostRepository _repostRepository = Substitute.For<IRepostRepository>();
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly RepostService _sut;

    public RepostServiceTests()
    {
        _sut = new RepostService(_repostRepository, _postRepository);
    }

    [Fact]
    public async Task ToggleAsync_NotReposted_CreatesRepost()
    {
        _postRepository.GetByIdAsync(10).Returns(new Post { Id = 10, Content = "test", AuthorId = 1 });
        _repostRepository.GetAsync(2, 10).Returns((Repost?)null);

        var result = await _sut.ToggleAsync(10, 2);

        Assert.False(result.Reposted);
        await _repostRepository.Received(1).AddAsync(Arg.Is<Repost>(r =>
            r.UserId == 2 &&
            r.PostId == 10 &&
            r.QuoteContent == null));
    }

    [Fact]
    public async Task ToggleAsync_AlreadyReposted_DeletesRepost()
    {
        var repost = new Repost { Id = 1, UserId = 2, PostId = 10 };
        _postRepository.GetByIdAsync(10).Returns(new Post { Id = 10, Content = "test", AuthorId = 1 });
        _repostRepository.GetAsync(2, 10).Returns(repost, (Repost?)null);

        await _sut.ToggleAsync(10, 2);

        await _repostRepository.Received(1).DeleteAsync(repost);
    }

    [Fact]
    public async Task QuoteAsync_CreatesQuoteRepost()
    {
        _postRepository.GetByIdAsync(10).Returns(new Post { Id = 10, Content = "test", AuthorId = 1 });
        _repostRepository.GetAsync(2, 10).Returns((Repost?)null);

        await _sut.QuoteAsync(10, new QuoteRepostRequest { Content = "Worth reading" }, 2);

        await _repostRepository.Received(1).AddAsync(Arg.Is<Repost>(r =>
            r.UserId == 2 &&
            r.PostId == 10 &&
            r.QuoteContent == "Worth reading"));
    }

    [Fact]
    public async Task QuoteAsync_ExistingRepost_UpdatesQuoteContent()
    {
        var repost = new Repost { Id = 1, UserId = 2, PostId = 10 };
        _postRepository.GetByIdAsync(10).Returns(new Post { Id = 10, Content = "test", AuthorId = 1 });
        _repostRepository.GetAsync(2, 10).Returns(repost);

        await _sut.QuoteAsync(10, new QuoteRepostRequest { Content = "Updated quote" }, 2);

        await _repostRepository.Received(1).UpdateAsync(Arg.Is<Repost>(r =>
            r.Id == 1 &&
            r.QuoteContent == "Updated quote"));
    }

    [Fact]
    public async Task QuoteAsync_Over280Chars_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.QuoteAsync(10, new QuoteRepostRequest { Content = new string('a', 281) }, 2));
    }

    [Fact]
    public async Task ToggleAsync_SelfRepost_ThrowsArgumentException()
    {
        _postRepository.GetByIdAsync(10).Returns(new Post { Id = 10, Content = "test", AuthorId = 2 });

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.ToggleAsync(10, 2));
    }

    [Fact]
    public async Task GetStateAsync_ReturnsCountAndUserState()
    {
        _repostRepository.GetCountByPostIdAsync(10).Returns(3);
        _repostRepository.GetAsync(2, 10).Returns(new Repost { Id = 5, UserId = 2, PostId = 10, QuoteContent = "quote" });

        var result = await _sut.GetStateAsync(10, 2);

        Assert.True(result.Reposted);
        Assert.Equal(3, result.Count);
        Assert.Equal(5, result.RepostId);
        Assert.Equal("quote", result.QuoteContent);
    }
}
