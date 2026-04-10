using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

// Concrete implementation of IUserRepository using EF Core.
// This class encapsulates all database access for User entities.
// Registered as AddScoped in Program.cs — one instance per HTTP request.
public class UserRepository : IUserRepository
{
    // readonly — can only be assigned in the constructor, prevents accidental reassignment
    // Underscore prefix (_) is a C# naming convention for private fields
    private readonly AppDbContext _context;

    // Constructor Dependency Injection (DI):
    // ASP.NET Core's DI container automatically provides an AppDbContext instance.
    // This is the same DbContext instance shared across all repositories in a single request (Scoped lifetime).
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    // FindAsync() is optimized for PK lookups — it first checks the local cache (Change Tracker)
    // before hitting the database. More efficient than FirstOrDefaultAsync for PK queries.
    // Returns null if not found (User? — nullable return type).
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    // FirstOrDefaultAsync: returns the first entity matching the lambda predicate, or null if none found.
    // u => u.UserName == userName — lambda expression: u is the parameter, the body is the condition.
    public async Task<User?> GetByUserNameAsync(string userName)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
    }

    public async Task<User?> GetByEmailAsync(string Email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
    }

    // EF.Functions.ILike() — PostgreSQL-specific case-insensitive LIKE.
    // $"%{query}%" — string interpolation building the LIKE pattern (% = any characters).
    // Translates to SQL: WHERE "UserName" ILIKE '%query%'
    public async Task<IEnumerable<User>> SearchAsync(string query)
    {
        return await _context.Users
            .Where(u => EF.Functions.ILike(u.UserName, $"%{query}%"))
            .ToListAsync();
    }

    // Returns suggested users (users the current user doesn't follow).
    // Subquery with !Any() — translates to SQL: WHERE NOT EXISTS (SELECT 1 FROM Follows ...)
    // OrderBy(u => Guid.NewGuid()) — randomizes results by sorting on a new GUID per row.
    // Take(count) — limits results (SQL LIMIT).
    public async Task<IEnumerable<User>> GetSuggestedAsync(int userId, int count)
    {
        return await _context.Users
            .Where(u => u.Id != userId
                && !_context.Follows.Any(f => f.FollowerId == userId && f.FollowingId == u.Id))
            .OrderBy(u => Guid.NewGuid())
            .Take(count)
            .ToListAsync();
    }

    // ToListAsync() — materializes the IQueryable into a List by executing the SQL query.
    // Until ToListAsync() is called, the query is just an expression tree (deferred execution).
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    // AddAsync() stages the entity for INSERT in the Change Tracker.
    // SaveChangesAsync() flushes all pending changes to the database as a single transaction.
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    // Update() marks the entity as Modified in the Change Tracker.
    // EF Core will generate an UPDATE statement for all columns on SaveChangesAsync().
    // Note: Update() is synchronous (no async version) — only SaveChanges hits the DB.
    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    // Remove() marks the entity for DELETE in the Change Tracker.
    // The actual DELETE SQL executes on SaveChangesAsync().
    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
