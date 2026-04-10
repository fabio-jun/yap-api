using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

// Repository interface — defines the contract for accessing User data.
// Lives in Domain layer so the Application layer can depend on it WITHOUT knowing
// about EF Core or the database. The actual implementation is in Infrastructure.
// This is the Repository Pattern: the interface is the abstraction, the implementation is the detail.
public interface IUserRepository
{
    // Task<T> — represents an asynchronous operation that returns T.
    // All methods are async because database calls are I/O-bound operations.
    // '?' after User means the method can return null (user not found).
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUserNameAsync(string userName);
    Task<User?> GetByEmailAsync(string email);

    // IEnumerable<T> — represents a read-only collection of elements.
    // Used for returning multiple results from queries.
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> SearchAsync(string query);
    Task<IEnumerable<User>> GetSuggestedAsync(int userId, int count);

    // Write operations — no return value (Task without <T>)
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
}
