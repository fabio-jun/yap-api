using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly AppDbContext _context;

    public PostRepository(AppDbContext context)
    {
        _context = context;
    }

    // Without Include, p.Author would be null (EF Core uses lazy loading only if explicitly configured).
    // OrderByDescending — most recent posts first (SQL: ORDER BY "CreatedAt" DESC).
    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await _context.Posts
            .Include(p => p.Author)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    // Where() — filters the query (SQL WHERE clause).
    public async Task<IEnumerable<Post>> GetByUserIdAsync(int userId)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.AuthorId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    // Pagination using Skip/Take pattern:
    // Skip((page - 1) * pageSize) — skips previous pages (SQL OFFSET)
    // Take(pageSize) — returns only one page worth of results (SQL LIMIT)
    // Returns a tuple (Items, TotalCount) — C# value tuple for multiple return values.
    // CountAsync() runs a separate COUNT(*) query for total pagination metadata.
    public async Task<(IEnumerable<Post> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize)
    {
        var query = _context.Posts
            .Include(p => p.Author)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            // Skip register from previous pages
            .Skip((page - 1) * pageSize)
            // Takes the number of records defined by pageSize
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    // Feed query — returns posts only from users the current user follows.
    // Uses a subquery: Any() checks if a Follow record exists linking the user to the post's author.
    public async Task<IEnumerable<Post>> GetFeedAsync(int userId)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Where(p => _context.Follows.Any(f => f.FollowerId == userId && f.FollowedId == p.AuthorId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    // EF.Functions.ILike — PostgreSQL case-insensitive pattern matching.
    // $"%{query}%" — searches for the query string anywhere in the content.
    public async Task<IEnumerable<Post>> SearchAsync(string query)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Where(p => EF.Functions.ILike(p.Content, $"%{query}%"))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    // Filters posts by tag name using a navigation chain:
    // p.PostTags! — the null-forgiving operator (!) tells the compiler PostTags won't be null at runtime.
    // If the PostTag doesn't exist, its an empty collection, not null
    // Any(pt => pt.Tag!.Name == tagName.ToLower()) — checks if any associated tag matches.
    // This traverses: Post -> PostTags (join table) -> Tag.Name
    public async Task<IEnumerable<Post>> GetByTagAsync(string tagName)
    {
        return await _context.Posts 
            .Include(p => p.Author)
            .Where(p => p.PostTags!.Any(pt => pt.Tag!.Name == tagName.ToLower()))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    // FirstOrDefaultAsync — returns the first match or null.
    // Unlike FindAsync (which uses PK + cache), this supports Include() for eager loading.
    public async Task<Post?> GetByIdAsync(int id)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Post post)
    {
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Post post)
    {
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Post post)
    {
        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
    }
}
