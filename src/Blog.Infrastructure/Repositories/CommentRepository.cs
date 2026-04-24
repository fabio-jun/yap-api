using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    // Returns all comments for a specific post.
    // Where() filters by PostId
    // Include(c => c.Author) eager loads the comment author's data.
    // OrderBy(c => c.CreatedAt) shows comments from oldest to newest.
    public async Task<IEnumerable<Comment>> GetByPostIdAsync(int postId)
    {
        return await _context.Comments
            .Where(c => c.PostId == postId)
            .Include(c => c.Author)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    // FirstOrDefaultAsync - returns the first comment matching the ID, or null.
    // Include(c => c.Author) loads the author together with the comment.
    public async Task<Comment?> GetByIdAsync(int id)
    {
        return await _context.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // Returns a comment with its related Post loaded.
    public async Task<Comment?> GetByIdWithPostAsync(int id)
    {
        return await _context.Comments
            .Include(c => c.Post)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // AddAsync() stages the comment for INSERT in the Change Tracker.
    // SaveChangesAsync() flushes the INSERT to the database.
    public async Task AddAsync(Comment comment)
    {
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
    }

    // Remove() marks the comment for DELETE in the Change Tracker.
    // The actual DELETE SQL executes on SaveChangesAsync().
    public async Task DeleteAsync(Comment comment)
    {
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }

    // Counts all comments (including replies) for a specific post.
    public async Task<int> GetCountByPostIdAsync(int postId)
    {
        return await _context.Comments.CountAsync(c => c.PostId == postId);
    }
}
