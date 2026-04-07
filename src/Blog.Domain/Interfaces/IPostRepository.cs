using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface IPostRepository
{
    Task<IEnumerable<Post>> GetAllAsync();
    Task<(IEnumerable<Post> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize);
    Task<IEnumerable<Post>> GetFeedAsync(int userId);
    Task<IEnumerable<Post>> SearchAsync(string query);
    Task<IEnumerable<Post>> GetByTagAsync(string tagName);
    Task<Post?> GetByIdAsync(int id);
    Task AddAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Post post);
}
