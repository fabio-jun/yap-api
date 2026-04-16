using Blog.Application.Cache;
using Blog.Application.DTOs.Tags;
using Blog.Application.Interfaces;
using Blog.Application.Services;
using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using NSubstitute;

namespace Blog.Tests;

public class TagServiceTests
{
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly TagService _sut;

    public TagServiceTests()
    {
        _sut = new TagService(_tagRepository, _cacheService);
    }

    [Fact]
    public async Task GetAllAsync_CacheHit_ReturnsCachedTagsWithoutRepository()
    {
        var cached = new List<TagResponse> { new() { Id = 1, Name = "dotnet", PostCount = 4 } };
        _cacheService.GetAsync<List<TagResponse>>(CacheKeys.AllTags()).Returns(cached);

        var result = (await _sut.GetAllAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("dotnet", result[0].Name);
        await _tagRepository.DidNotReceive().GetAllAsync();
    }

    [Fact]
    public async Task GetAllAsync_CacheMiss_PopulatesAllTagsCache()
    {
        var tags = new List<Tag>
        {
            new()
            {
                Id = 1,
                Name = "dotnet",
                PostTags = new List<PostTag> { new() { PostId = 1, TagId = 1 }, new() { PostId = 2, TagId = 1 } }
            }
        };
        _cacheService.GetAsync<List<TagResponse>>(CacheKeys.AllTags()).Returns((List<TagResponse>?)null);
        _tagRepository.GetAllAsync().Returns(tags);

        var result = (await _sut.GetAllAsync()).ToList();

        Assert.Single(result);
        await _cacheService.Received(1).SetAsync(
            CacheKeys.AllTags(),
            Arg.Is<List<TagResponse>>(responses => responses.Single().PostCount == 2),
            TimeSpan.FromMinutes(30),
            Arg.Any<CancellationToken>());
    }
}
