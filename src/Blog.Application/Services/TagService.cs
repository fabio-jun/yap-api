using Blog.Domain.Interfaces;
using Blog.Application.DTOs.Tags;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

// Service that handles tag (hashtag) queries.
public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    // Returns all tags ordered by popularity, with the count of posts using each one.
    public async Task<IEnumerable<TagResponse>> GetAllAsync()
    {
        var tags = await _tagRepository.GetAllAsync();

        // LINQ .Select() — projects each Tag entity into a TagResponse DTO
        return tags.Select(tag => new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            // Count how many posts are associated with this tag via the PostTags join table
            // '?.' — if PostTags is null, return null; '?? 0' — if null, default to 0
            PostCount = tag.PostTags?.Count ?? 0
        });
    }
}
