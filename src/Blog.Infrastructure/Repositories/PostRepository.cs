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

    // Returns all posts ordered by most recent first
    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await _context.Posts
            .Include(p => p.Author)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    // Returns all posts with pagination
    public async Task<(IEnumerable<Post> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize)
    {
        var query = _context.Posts
            .Include(p => p.Author)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // Returns posts from users that the given user follows
    public async Task<IEnumerable<Post>> GetFeedAsync(int userId)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Where(p => _context.Follows.Any(f => f.FollowerId == userId && f.FollowingId == p.AuthorId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    // Searches posts by content
    public async Task<IEnumerable<Post>> SearchAsync(string query)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Where(p => EF.Functions.ILike(p.Content, $"%{query}%"))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    // Returns posts that have a specific tag
    public async Task<IEnumerable<Post>> GetByTagAsync(string tagName)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.PostTags!.Any(pt => pt.Tag!.Name == tagName.ToLower()))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    // Returns a single post by ID
    public async Task<Post?> GetByIdAsync(int id)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // Inserts a new post
    public async Task AddAsync(Post post)
    {
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();
    }

    // Updates an existing post
    public async Task UpdateAsync(Post post)
    {
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
    }

    // Deletes a post
    public async Task DeleteAsync(Post post)
    {
        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
    }
}
