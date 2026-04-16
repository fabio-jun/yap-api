using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

// Bookmark uses composite PK (PostId, UserId) — same pattern as Like.
public class BookmarkRepository : IBookmarkRepository
{
    private readonly AppDbContext _context;

    public BookmarkRepository(AppDbContext context)
    {
        _context = context;
    }

    // Checks if a bookmark exists for a specific post + user combination.
    public async Task<Bookmark?> GetAsync(int postId, int userId)
    {
        return await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.PostId == postId && b.UserId == userId);
    }

    // Returns all posts bookmarked by a user, ordered by most recently bookmarked.
    // Chained Include + ThenInclude pattern:
    //   Include(b => b.Post!) — eagerly loads the Post entity from each Bookmark
    //   ThenInclude(p => p.Author) — then eagerly loads the Post's Author (nested include)
    // Select(b => b.Post!) — projects to return Post entities instead of Bookmark entities.
    // The '!' null-forgiving operator tells the compiler Post won't be null at runtime.
    public async Task<IEnumerable<Post>> GetByUserAsync(int userId)
    {
        return await _context.Bookmarks
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Include(b => b.Post!)
                .ThenInclude(p => p.Author)
            .Select(b => b.Post!)
            .ToListAsync();
    }

    public async Task AddAsync(Bookmark bookmark)
    {
        await _context.Bookmarks.AddAsync(bookmark);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Bookmark bookmark)
    {
        _context.Bookmarks.Remove(bookmark);
        await _context.SaveChangesAsync();
    }
}
