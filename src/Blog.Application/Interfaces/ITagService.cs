using Blog.Application.DTOs.Tags;

namespace Blog.Application.Interfaces;

// Service interface for tag (hashtag) operations.
public interface ITagService
{
    // Returns all tags ordered by popularity (most used first), with post count
    Task<IEnumerable<TagResponse>> GetAllAsync();
}
