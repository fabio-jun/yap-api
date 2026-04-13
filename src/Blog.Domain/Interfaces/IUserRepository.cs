using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface IUserRepository
{
    // All methods are async because DB calls are I/O-bound operations.
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUserNameAsync(string userName);
    Task<User?> GetByEmailAsync(string email);

    // IEnumerable<T> — represents a read-only collection of elements.
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> SearchAsync(string query);
    Task<IEnumerable<User>> GetSuggestedAsync(int userId, int count);

    // Write operations — no return value
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
}
