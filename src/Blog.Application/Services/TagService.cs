using Blog.Domain.Interfaces;
using Blog.Application.Cache;
using Blog.Application.DTOs.Tags;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

// Service that handles tag (hashtag) queries.
public class TagService : ITagService
{
    private static readonly TimeSpan TagsTtl = TimeSpan.FromMinutes(30);

    private readonly ITagRepository _tagRepository;
    private readonly ICacheService _cache;

    public TagService(ITagRepository tagRepository, ICacheService cache)
    {
        _tagRepository = tagRepository;
        _cache = cache;
    }

    // Returns all tags ordered by popularity, with the count of posts using each one.
    public async Task<IEnumerable<TagResponse>> GetAllAsync()
    {
        var cached = await _cache.GetAsync<List<TagResponse>>(CacheKeys.AllTags());
        if (cached != null)
        {
            return cached;
        }

        var tags = await _tagRepository.GetAllAsync();

        // LINQ .Select() — projects each Tag entity into a TagResponse DTO
        var result = tags.Select(tag => new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            // Count how many posts are associated with this tag via the PostTags join table
            // '?.' — if PostTags is null, return null; '?? 0' — if null, default to 0
            PostCount = tag.PostTags?.Count ?? 0
        }).ToList();

        await _cache.SetAsync(CacheKeys.AllTags(), result, TagsTtl);
        return result;
    }
}
