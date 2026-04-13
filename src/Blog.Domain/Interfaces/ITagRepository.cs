using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

// Repository interface for Tag data access.
public interface ITagRepository
{
    // Get a tag by its name. Returns null if not found.
    Task<Tag?> GetByNameAsync(string name);
    // Returns all tags
    Task<IEnumerable<Tag>> GetAllAsync();

    Task AddAsync(Tag tag);
}
