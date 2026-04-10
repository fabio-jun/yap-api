using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

// Repository interface for Tag data access.
public interface ITagRepository
{
    // Finds a tag by name — used during hashtag extraction to check if tag already exists
    Task<Tag?> GetByNameAsync(string name);

    // Returns all tags ordered by popularity (number of posts using each tag)
    Task<IEnumerable<Tag>> GetAllAsync();

    Task AddAsync(Tag tag);
}
