using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class BookmarkRepository : IBookmarkRepository
{
    private readonly AppDbContext _context;

    public BookmarkRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Bookmark?> GetAsync(int postId, int userId)
    {
        return await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.PostId == postId && b.UserId == userId);
    }

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
