using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

// Concrete implementation of ICommentRepository using EF Core.
// Handles CRUD operations for comments on posts.
public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    // Returns all comments for a specific post.
    // Include(c => c.Author) — eager loads the User who wrote the comment,
    // so we can display the author's username/avatar without a second query.
    public async Task<IEnumerable<Comment>> GetByPostIdAsync(int postId)
    {
        return await _context.Comments
            .Where(c => c.PostId == postId)
            .Include(c => c.Author)
            .ToListAsync();
    }

    // FirstOrDefaultAsync — returns the first match or null if no comment has this ID.
    // Include ensures the Author navigation property is populated.
    public async Task<Comment?> GetByIdAsync(int id)
    {
        return await _context.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // AddAsync — stages the comment for INSERT in the Change Tracker.
    // SaveChangesAsync — executes the INSERT SQL against the database.
    public async Task AddAsync(Comment comment)
    {
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
    }

    // Remove — marks the entity for DELETE (synchronous, no DB hit yet).
    // SaveChangesAsync — executes the DELETE SQL.
    public async Task DeleteAsync(Comment comment)
    {
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }
}
