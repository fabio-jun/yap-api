using Blog.Application.DTOs.Tags;

namespace Blog.Application.Interfaces;

public interface ITagService
{
    // Return all tags ordered by popularity
    Task<IEnumerable<TagResponse>> GetAllAsync();
}