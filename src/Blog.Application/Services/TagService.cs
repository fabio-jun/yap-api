using Blog.Domain.Interfaces;
using Blog.Application.DTOs.Tags;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    // Returns all tags with the count of posts using each one
    public async Task<IEnumerable<TagResponse>> GetAllAsync()
    {
        var tags = await _tagRepository.GetAllAsync();

        return tags.Select(tag => new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            // Count how many posts are associated with this tag
            PostCount = tag.PostTags?.Count ?? 0
        });
    }
}
