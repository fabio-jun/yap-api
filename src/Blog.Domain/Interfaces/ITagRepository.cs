using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface ITagRepository
{
    Task<Tag?> GetByNameAsync(string name);
    Task<IEnumerable<Tag>> GetAllAsync();
    Task AddAsync(Tag tag);
}